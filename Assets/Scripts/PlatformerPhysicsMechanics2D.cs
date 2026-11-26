using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.3. Programación de mecánicas y físicas
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlatformerPhysicsMechanics2D.cs
/// Responsabilidad: Implementar y perfeccionar el modelo físico de mecánicas del personaje 2D:
/// 1. Simulación de gravedad mejorada (Better Jump con gravedad incrementada en caída libre).
/// 2. Salto con altura variable condicionado a la duración de la pulsación de teclado (Low Jump).
/// 3. Detección robusta de colisión con el suelo (Ground Check) para prevenir saltos infinitos en el aire.
/// 4. Tolerancia temporal de salto (Coyote Time y Jump Buffer) para un 'game feel' fluido y preciso.
/// 5. Restricción angular de rotación (Freeze Rotation Z) para estabilidad cinemática en impactos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlatformerPhysicsMechanics2D : MonoBehaviour
{
    [Header("1. Configuración de Gravedad y Físicas")]
    [Tooltip("Escala de gravedad personalizada en el Rigidbody2D.")]
    [SerializeField] private float baseGravityScale = 2.2f;

    [Tooltip("Multiplicador de gravedad aplicado durante el descenso (caída) para mayor contundencia.")]
    [SerializeField] private float fallGravityMultiplier = 1.6f;

    [Tooltip("Multiplicador de gravedad si el jugador suelta la tecla de salto antes del punto cúspide.")]
    [SerializeField] private float lowJumpMultiplier = 1.25f;

    [Header("2. Parámetros de Salto e Impulso")]
    [Tooltip("Fuerza del impulso vertical aplicada en el despegue (Calibrado para salto alto y amplio).")]
    [SerializeField] private float jumpForce = 16.5f;

    [Tooltip("Permitir doble salto en el aire para superar plataformas altas y abismos.")]
    [SerializeField] private bool allowDoubleJump = true;

    [Tooltip("Fuerza del impulso del doble salto en el aire.")]
    [SerializeField] private float doubleJumpForce = 15.0f;

    [Tooltip("Tiempo de tolerancia para saltar después de haber dejado el suelo (Coyote Time).")]
    [SerializeField] private float coyoteTime = 0.15f;

    [Tooltip("Tiempo de tolerancia que almacena una pulsación de salto previa al aterrizaje (Jump Buffer).")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("3. Detección de Suelo y Colisión")]
    [Tooltip("Transform situado en los pies del personaje para comprobar contacto con plataformas.")]
    [SerializeField] private Transform groundCheckPoint;

    [Tooltip("Radio de detección esférica de contacto con el suelo.")]
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Tooltip("Máscara de capas que representan superficies sólidas de suelo.")]
    [SerializeField] private LayerMask groundLayer;

    [Header("4. Eventos de Físicas")]
    public UnityEvent onJumpExecuted;
    public UnityEvent onGroundedLand;
    public UnityEvent<bool> onGroundedStateChanged;

    // Componentes internos
    private Rigidbody2D rb;
    private Collider2D col;

    // Variables de estado cinemático
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private float coyoteTimeCounter = 0f;
    private float jumpBufferCounter = 0f;
    private bool jumpButtonPressed = false;
    private bool jumpButtonReleased = false;
    private bool canDoubleJump = false;

    // Propiedades públicas para auditoría
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => rb != null ? rb.linearVelocity.y : 0f;
    public float CurrentGravityScale => rb != null ? rb.gravityScale : 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Criterio 1: Configurar simulación de gravedad y restricción de rotación en Z
        if (rb != null)
        {
            rb.gravityScale = baseGravityScale;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Criterio 3: Auto-configurar punto de comprobación de suelo a la base del colisionador
        if (groundCheckPoint == null)
        {
            GameObject checkObj = new GameObject("GroundCheckPoint_Auto");
            checkObj.transform.SetParent(transform);
            float bottomY = col != null ? -col.bounds.extents.y : -0.7f;
            checkObj.transform.localPosition = new Vector3(0f, bottomY, 0f);
            groundCheckPoint = checkObj.transform;
        }

        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Default", "Ground", "Piso", "Platforms");
        }
    }

    private void Update()
    {
        // 1. Lectura de hardware de teclado para el evento de salto
        ReadJumpInput();

        // 2. Gestión de Coyote Time y Doble Salto
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 3. Gestión de Jump Buffer
        if (jumpButtonPressed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpButtonPressed = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 4. Intentar ejecutar el salto si se cumplen las condiciones físicas
        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f)
            {
                ExecuteJumpImpulse(jumpForce);
            }
            else if (allowDoubleJump && canDoubleJump)
            {
                canDoubleJump = false;
                ExecuteJumpImpulse(doubleJumpForce);
            }
        }
    }

    private void FixedUpdate()
    {
        // 1. Comprobación física de colisión con suelo
        CheckGroundCollision();

        // 2. Modificación dinámica de gravedad para mejorar el 'game feel'
        ApplyBetterJumpPhysics();
    }

    /// <summary>
    /// Lee la entrada del teclado compatible con Unity 6 Input System y Legacy.
    /// </summary>
    private void ReadJumpInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || 
                Keyboard.current.wKey.wasPressedThisFrame || 
                Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jumpButtonPressed = true;
            }

            if (Keyboard.current.spaceKey.wasReleasedThisFrame || 
                Keyboard.current.wKey.wasReleasedThisFrame || 
                Keyboard.current.upArrowKey.wasReleasedThisFrame)
            {
                jumpButtonReleased = true;
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            jumpButtonPressed = true;
        }
        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W))
        {
            jumpButtonReleased = true;
        }
#endif
    }

    /// <summary>
    /// Ejecuta el impulso físico vertical de salto respetando la masa del Rigidbody2D.
    /// </summary>
    private void ExecuteJumpImpulse(float force)
    {
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        // Limpiar velocidad vertical residual para salto uniforme y ágil
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);

        onJumpExecuted?.Invoke();
        Debug.Log($"[PlatformerPhysicsMechanics2D] Salto ejecutado con impulso de {force} m/s.");
    }

    /// <summary>
    /// Aplica multiplicadores de gravedad para un arco de salto ágil y sin flotabilidad.
    /// </summary>
    private void ApplyBetterJumpPhysics()
    {
        if (rb.linearVelocity.y < 0f)
        {
            // Caída rápida: aumenta la gravedad en la fase descendente
            rb.gravityScale = baseGravityScale * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0f && jumpButtonReleased)
        {
            // Salto corto: si el jugador soltó la tecla antes de tiempo, corta la elevación
            rb.gravityScale = baseGravityScale * lowJumpMultiplier;
        }
        else
        {
            // Fase normal ascendente o reposo
            rb.gravityScale = baseGravityScale;
        }

        // Resetear flag de tecla soltada tras evaluar
        if (isGrounded)
        {
            jumpButtonReleased = false;
        }
    }

    /// <summary>
    /// Verifica si los pies del personaje colisionan con el suelo o plataformas sólidas.
    /// </summary>
    private void CheckGroundCollision()
    {
        wasGrounded = isGrounded;

        Vector2 checkPos = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);

        isGrounded = hit != null && hit.gameObject != gameObject;

        // Eventos de aterrizaje
        if (!wasGrounded && isGrounded)
        {
            onGroundedLand?.Invoke();
            Debug.Log($"[PlatformerPhysicsMechanics2D] Colisión con suelo confirmada con '{hit.name}'. Impacto absorbido por física.");
        }

        if (wasGrounded != isGrounded)
        {
            onGroundedStateChanged?.Invoke(isGrounded);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
