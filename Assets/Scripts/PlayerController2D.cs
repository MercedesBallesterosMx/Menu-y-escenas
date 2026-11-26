using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.1. La escena principal y la estructura del personaje
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlayerController2D.cs
/// Responsabilidad: Gestionar el movimiento horizontal y salto del personaje 2D mediante
/// detección de eventos del teclado (Input System / Legacy), control de físicas con Rigidbody2D,
/// comprobación de contacto con el suelo y orientación visual del sprite.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Parámetros de Movimiento Horizontal")]
    [Tooltip("Velocidad de desplazamiento lateral del personaje.")]
    [SerializeField] private float moveSpeed = 7.0f;

    [Tooltip("Suavizado de aceleración lateral (SmoothDamp).")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float movementSmoothing = 0.05f;

    [Header("Parámetros de Salto y Físicas")]
    [Tooltip("Fuerza de impulso vertical aplicada en el salto.")]
    [SerializeField] private float jumpForce = 12.0f;

    [Tooltip("Capa que define el suelo para permitir el salto.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Transform que marca el punto de comprobación de suelo a los pies del personaje.")]
    [SerializeField] private Transform groundCheckPoint;

    [Tooltip("Radio del círculo para verificar si toca el suelo.")]
    [SerializeField] private float groundCheckRadius = 0.22f;

    [Header("Orientación del Sprite")]
    [Tooltip("Indica si el sprite mira originalmente hacia la derecha.")]
    [SerializeField] private bool facingRight = true;
    public bool IsFacingRight => facingRight;

    [Header("Eventos de Control para Animación")]
    public UnityEvent<float> onSpeedChanged;     // Envía la velocidad absoluta para transicionar Idle -> Walk
    public UnityEvent<bool> onGroundedChanged;   // Envía si está en el suelo (true) o en el aire (false)
    public UnityEvent onJumpTriggered;          // Se dispara al ejecutar un salto

    // Variables internas de control
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput = 0f;
    private bool jumpRequested = false;
    private bool isGrounded = false;
    private Vector2 currentVelocity = Vector2.zero;

    // Propiedades públicas para lectura desde el controlador de animación
    public float CurrentSpeed => Mathf.Abs(rb2D.linearVelocity.x);
    public float VerticalVelocity => rb2D.linearVelocity.y;
    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Garantizar que el Rigidbody2D no rote por colisiones
        rb2D.freezeRotation = true;

        // Auto-crear punto de verificación de suelo si no fue asignado en el Inspector
        if (groundCheckPoint == null)
        {
            GameObject checkObj = new GameObject("GroundCheckPoint");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            groundCheckPoint = checkObj.transform;
        }

        // Si la máscara de capa no está asignada, usar la capa por defecto o crear filtro
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Default", "Ground", "Piso");
        }
    }

    private void Update()
    {
        // 1. Lectura de eventos de entrada por teclado (Compatible con Unity 6 y Clásico)
        ReadKeyboardInput();

        // 2. Disparar evento de velocidad para el controlador de estados
        onSpeedChanged?.Invoke(Mathf.Abs(horizontalInput));
    }

    private void FixedUpdate()
    {
        // 1. Verificar si el personaje está tocando el suelo mediante OverlapCircle
        CheckGroundStatus();

        // 2. Aplicar desplazamiento físico horizontal
        ApplyMovement();

        // 3. Aplicar impulso de salto si fue solicitado y está en el suelo
        ApplyJump();
    }

    /// <summary>
    /// Lee la entrada del teclado considerando ambos esquemas de Unity.
    /// </summary>
    private void ReadKeyboardInput()
    {
        float h = 0f;
        bool jump = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jump = true;
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Mathf.Approximately(h, 0f))
        {
            h = Input.GetAxisRaw("Horizontal");
        }
        if (!jump && Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
#endif

        horizontalInput = h;
        if (jump)
        {
            jumpRequested = true;
        }
    }

    /// <summary>
    /// Comprueba colisión circular en los pies para determinar el estado de contacto con el suelo.
    /// </summary>
    private void CheckGroundStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        if (groundCheckPoint != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPoint.position, groundCheckRadius, groundLayer);
            for (int i = 0; i < colliders.Length; i++)
            {
                // Ignorar colisiones consigo mismo
                if (colliders[i].gameObject != gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Si cambió el estado de suelo, disparar evento
        if (wasGrounded != isGrounded)
        {
            onGroundedChanged?.Invoke(isGrounded);
        }
    }

    /// <summary>
    /// Aplica velocidad lineal al Rigidbody2D y controla el volteo (Flip) del personaje.
    /// </summary>
    private void ApplyMovement()
    {
        // Si AdvancedPlayerMovement2D está presente en el objeto, él gobierna el desplazamiento horizontal
        if (GetComponent<AdvancedPlayerMovement2D>() != null)
        {
            return;
        }

        Vector2 targetVelocity = new Vector2(horizontalInput * moveSpeed, rb2D.linearVelocity.y);
        rb2D.linearVelocity = Vector2.SmoothDamp(rb2D.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);

        // Controlar el sentido de la orientación (Flip)
        if (horizontalInput > 0.05f && !facingRight)
        {
            FlipCharacter();
        }
        else if (horizontalInput < -0.05f && facingRight)
        {
            FlipCharacter();
        }
    }

    /// <summary>
    /// Ejecuta el impulso de salto vertical con físicas reactivas.
    /// </summary>
    private void ApplyJump()
    {
        if (jumpRequested)
        {
            jumpRequested = false;

            if (isGrounded)
            {
                // Cancelar velocidad vertical previa para consistencia en el impulso
                rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
                rb2D.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                isGrounded = false;

                onJumpTriggered?.Invoke();
            }
        }
    }

    /// <summary>
    /// Invierte la orientación horizontal del personaje mediante escala en X,
    /// asegurando que hijos como FirePoint y colisionadores se volteen simétricamente.
    /// </summary>
    private void FlipCharacter()
    {
        facingRight = !facingRight;

        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * (facingRight ? 1.0f : -1.0f);
        transform.localScale = currentScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false; // Nunca usar flipX simultáneamente para evitar cancelaciones
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el radio de detección de suelo en la vista Scene de Unity
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
