using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.5. El movimiento del jugador
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: AdvancedPlayerMovement2D.cs
/// Responsabilidad: Gobernar el desplazamiento cinemático y horizontal del héroe:
/// 1. Desplazamiento reactivo con flechas de dirección (Izquierda/Derecha) y teclas A/D.
/// 2. Respeto absoluto a las físicas del entorno (Rigidbody2D en FixedUpdate) impidiendo atravesar colisionadores.
/// 3. Modelo de aceleración y fricción progresiva para control preciso sin derrapes incontrolados.
/// 4. Inversión simétrica del sprite (Flip) y sincronización del punto de disparo (FirePoint).
/// 5. Comunicación de parámetros de velocidad al Animator Controller (Idle <-> Run).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class AdvancedPlayerMovement2D : MonoBehaviour
{
    [Header("1. Configuración de Velocidad y Aceleración")]
    [Tooltip("Velocidad máxima de desplazamiento horizontal.")]
    [SerializeField] private float maxMoveSpeed = 8.0f;

    [Tooltip("Tasa de aceleración lineal para alcanzar la velocidad máxima.")]
    [SerializeField] private float accelerationRate = 45.0f;

    [Tooltip("Tasa de deceleración / frenado al soltar los controles.")]
    [SerializeField] private float decelerationRate = 50.0f;

    [Header("2. Orientación del Personaje")]
    [Tooltip("Indica si el sprite inicia orientado hacia la derecha.")]
    [SerializeField] private bool facingRight = true;

    [Tooltip("Referencia al punto de disparo para invertir su offset horizontal al girar.")]
    [SerializeField] private Transform firePoint;

    [Header("3. Eventos y Comunicación")]
    public UnityEvent<float> onHorizontalSpeedChanged;
    public UnityEvent<bool> onDirectionFlipped;

    // Componentes internos
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Variables de control de entrada
    private float horizontalInput = 0f;
    private float targetVelocityX = 0f;

    // Propiedades públicas para lectura y auditoría
    public float CurrentSpeed => Mathf.Abs(rb != null ? rb.linearVelocity.x : 0f);
    public bool FacingRight => facingRight;
    public float HorizontalInput => horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Criterio 2: Garantizar que el Rigidbody2D respete la física sólida
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Localizar FirePoint hijo si no fue asignado
        if (firePoint == null)
        {
            Transform fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
        }
    }

    private void Update()
    {
        // Criterio 1: Lectura de flechas de dirección y teclas de movimiento
        ReadDirectionalInput();

        // Actualizar parámetros visuales del Animator
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        onHorizontalSpeedChanged?.Invoke(Mathf.Abs(horizontalInput));
    }

    private void FixedUpdate()
    {
        // Criterio 2: Desplazamiento físico respetando colisiones del entorno
        ApplyHorizontalPhysicsMovement();

        // Control de orientación del sprite
        HandleSpriteOrientation();
    }

    /// <summary>
    /// Captura eventos de flechas de dirección y teclas A/D compatibles con Unity 6 y Legacy.
    /// </summary>
    private void ReadDirectionalInput()
    {
        float inputX = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                inputX -= 1.0f;
            }
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                inputX += 1.0f;
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Mathf.Approximately(inputX, 0f))
        {
            inputX = Input.GetAxisRaw("Horizontal");
        }
#endif

        horizontalInput = Mathf.Clamp(inputX, -1.0f, 1.0f);
    }

    /// <summary>
    /// Aplica aceleración física en el eje X preservando la velocidad Y de la gravedad.
    /// Al utilizar linearVelocity en FixedUpdate, el Rigidbody2D colisiona físicamente contra
    /// muros y plataformas sólidas sin atravesarlos (Criterio 2).
    /// </summary>
    private void ApplyHorizontalPhysicsMovement()
    {
        targetVelocityX = horizontalInput * maxMoveSpeed;

        float rate = Mathf.Abs(horizontalInput) > 0.01f ? accelerationRate : decelerationRate;
        float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, rate * Time.fixedDeltaTime);

        // Preservar la velocidad vertical gobernada por la gravedad
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    /// <summary>
    /// Invierte la orientación horizontal del personaje de forma consistente.
    /// </summary>
    private void HandleSpriteOrientation()
    {
        if (horizontalInput > 0.05f && !facingRight)
        {
            FlipCharacter(true);
        }
        else if (horizontalInput < -0.05f && facingRight)
        {
            FlipCharacter(false);
        }
    }

    private void FlipCharacter(bool lookRight)
    {
        facingRight = lookRight;

        // Inversión mediante escala en X para voltear colisionadores y objetos hijos coordinadamente
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * (facingRight ? 1.0f : -1.0f);
        transform.localScale = currentScale;

        onDirectionFlipped?.Invoke(facingRight);
        Debug.Log($"[AdvancedPlayerMovement2D] Orientación actualizada: {(facingRight ? "Derecha (+X)" : "Izquierda (-X)")}");
    }
}
