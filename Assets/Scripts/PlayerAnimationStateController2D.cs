using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.1. La escena principal y la estructura del personaje
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlayerAnimationStateController2D.cs
/// Responsabilidad: Coordinar la máquina de estados de animación (Animator / Mecanim) del personaje 2D,
/// sincronizando los parámetros de transición (Speed, isGrounded, verticalVelocity, jumpTrigger)
/// estrictamente en función de los eventos de teclado y el estado físico del personaje.
/// </summary>
public class PlayerAnimationStateController2D : MonoBehaviour
{
    [Header("Referencias de Componentes")]
    [Tooltip("Controlador de físicas del personaje.")]
    [SerializeField] private PlayerController2D playerController;

    [Tooltip("Componente Animator para máquinas de estado en Mecanim.")]
    [SerializeField] private Animator animator;

    [Tooltip("SpriteRenderer para retroalimentación visual reactiva.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Nombres de Parámetros en el Animator")]
    [Tooltip("Parámetro Float para velocidad de movimiento (0 = Idle, >0.1 = Walk).")]
    [SerializeField] private string speedParam = "Speed";

    [Tooltip("Parámetro Bool para estado de contacto con el suelo.")]
    [SerializeField] private string isGroundedParam = "isGrounded";

    [Tooltip("Parámetro Float para velocidad vertical en salto y caída.")]
    [SerializeField] private string verticalVelocityParam = "VerticalVelocity";

    [Tooltip("Parámetro Trigger para activar la animación de despegue de salto.")]
    [SerializeField] private string jumpTriggerParam = "Jump";

    [Header("Retroalimentación Visual Complementaria")]
    [Tooltip("Habilitar micro-animación procedural de Squash & Stretch en salto.")]
    [SerializeField] private bool enableProceduralFeedback = true;

    // Estado interno
    private Vector3 initialScale;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (playerController != null)
        {
            playerController.onSpeedChanged.AddListener(UpdateSpeed);
            playerController.onGroundedChanged.AddListener(UpdateGroundedState);
            playerController.onJumpTriggered.AddListener(TriggerJumpAnimation);
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.onSpeedChanged.RemoveListener(UpdateSpeed);
            playerController.onGroundedChanged.RemoveListener(UpdateGroundedState);
            playerController.onJumpTriggered.RemoveListener(TriggerJumpAnimation);
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        // Sincronizar velocidad vertical para distinguir ascenso (salto) y descenso (caída)
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat(verticalVelocityParam, playerController.VerticalVelocity);
            animator.SetBool(isGroundedParam, playerController.IsGrounded);
        }
    }

    /// <summary>
    /// Actualiza el parámetro de velocidad para transicionar entre Idle y Walk.
    /// </summary>
    public void UpdateSpeed(float speed)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat(speedParam, speed);
        }
    }

    /// <summary>
    /// Actualiza el parámetro booleano de suelo para alternar entre estados en tierra y aire.
    /// </summary>
    public void UpdateGroundedState(bool isGrounded)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(isGroundedParam, isGrounded);
        }

        if (isGrounded && enableProceduralFeedback)
        {
            // Efecto de Squash al aterrizar
            TriggerSquashAndStretch(1.25f, 0.75f, 0.12f);
        }
    }

    /// <summary>
    /// Dispara el trigger de salto en Mecanim y ejecuta el estiramiento procedural.
    /// </summary>
    public void TriggerJumpAnimation()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger(jumpTriggerParam);
        }

        if (enableProceduralFeedback)
        {
            // Efecto de Stretch al impulsarse en el aire
            TriggerSquashAndStretch(0.8f, 1.3f, 0.15f);
        }
    }

    private void TriggerSquashAndStretch(float scaleXMultiplier, float scaleYMultiplier, float duration)
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }
        feedbackRoutine = StartCoroutine(SquashStretchRoutine(scaleXMultiplier, scaleYMultiplier, duration));
    }

    private IEnumerator SquashStretchRoutine(float targetScaleX, float targetScaleY, float duration)
    {
        Vector3 stretched = new Vector3(
            Mathf.Sign(transform.localScale.x) * Mathf.Abs(initialScale.x) * targetScaleX,
            initialScale.y * targetScaleY,
            initialScale.z
        );

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(stretched, new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(initialScale.x), initialScale.y, initialScale.z), t);
            yield return null;
        }

        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
    }
}
