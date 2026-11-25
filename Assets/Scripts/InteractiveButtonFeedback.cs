using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.3. Programación de botones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: InteractiveButtonFeedback.cs
/// Responsabilidad: Proveer retroalimentación visual y háptica/auditiva
/// a los botones de la interfaz interactiva mediante animaciones de escala y sonidos.
/// </summary>
public class InteractiveButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Escalado Dinámico")]
    [Tooltip("Escala al pasar el cursor por encima (Hover).")]
    [SerializeField] private float hoverScale = 1.08f;

    [Tooltip("Escala al presionar el botón (Click/Press).")]
    [SerializeField] private float pressScale = 0.94f;

    [Tooltip("Velocidad de transición de escala.")]
    [SerializeField] private float transitionSpeed = 12f;

    [Header("Retroalimentación de Audio (Opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Interpolación fluida hacia la escala objetivo
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        PlayAudio(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressScale;
        PlayAudio(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
    }
}
