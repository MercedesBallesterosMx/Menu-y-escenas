using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.5. Detección de colisiones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: CollisionStateTrigger2D.cs
/// Responsabilidad: Detectar colisiones y triggers en 2D, modificando el estado del objeto
/// (cambio cromático, escala, sonido y disparo de eventos) al contactar con paredes,
/// piso u otros elementos de la escena.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CollisionStateTrigger2D : MonoBehaviour
{
    [Header("Configuración de Colisión")]
    [Tooltip("Etiqueta (Tag) requerida para activar el cambio de estado.")]
    [SerializeField] private string targetTag = "Obstaculo";

    [Tooltip("Si es verdadero, opera como Trigger (sin respuesta física rígida).")]
    [SerializeField] private bool useTriggerMode = false;

    [Header("Cambios de Estado Visual")]
    [Tooltip("Color aplicado al sprite cuando colisiona.")]
    [SerializeField] private Color collisionStateColor = new Color(1f, 0.35f, 0.35f, 1f); // Rojo alerta

    [Tooltip("Color original restaurado al salir de la colisión.")]
    [SerializeField] private Color normalStateColor = Color.white;

    [Tooltip("Efecto de rebote o escala reactiva al colisionar.")]
    [SerializeField] private float bounceScaleFactor = 1.2f;

    [Header("Audio de Retroalimentación")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collisionSoundClip;

    [Header("Eventos de Colisión")]
    public UnityEvent onCollisionEnterEvent;
    public UnityEvent onCollisionExitEvent;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Collider2D col2D;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        originalScale = transform.localScale;

        if (spriteRenderer != null)
        {
            normalStateColor = spriteRenderer.color;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (col2D != null)
        {
            col2D.isTrigger = useTriggerMode;
        }
    }

    #region Colisiones Físicas (OnCollision2D)

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTriggerMode) return;

        if (string.IsNullOrEmpty(targetTag) || collision.gameObject.CompareTag(targetTag))
        {
            TriggerStateChange(collision.gameObject.name);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (useTriggerMode) return;

        if (string.IsNullOrEmpty(targetTag) || collision.gameObject.CompareTag(targetTag))
        {
            ResetState();
        }
    }

    #endregion

    #region Zonas Trigger (OnTrigger2D)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTriggerMode) return;

        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            TriggerStateChange(other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useTriggerMode) return;

        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            ResetState();
        }
    }

    #endregion

    /// <summary>
    /// Aplica el cambio de estado físico y visual al colisionar.
    /// </summary>
    private void TriggerStateChange(string hitObjectName)
    {
        Debug.Log($"[CollisionStateTrigger2D] Colisión detectada con '{hitObjectName}'. Modificando estado.");

        if (spriteRenderer != null)
        {
            spriteRenderer.color = collisionStateColor;
        }

        transform.localScale = originalScale * bounceScaleFactor;

        if (audioSource != null && collisionSoundClip != null)
        {
            audioSource.PlayOneShot(collisionSoundClip);
        }

        onCollisionEnterEvent?.Invoke();
    }

    /// <summary>
    /// Restaura el estado normal del objeto al terminar el contacto.
    /// </summary>
    private void ResetState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalStateColor;
        }

        transform.localScale = originalScale;
        onCollisionExitEvent?.Invoke();
    }
}
