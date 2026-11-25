using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.6. El flujo de la interfaz
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: MecanimStateController.cs
/// Responsabilidad: Controlar máquinas de estados de animación en Mecanim (Animator Controller).
/// Garantiza que los elementos permanezcan en estado Idle inicial y que la animación
/// interactiva se reproduzca estrictamente al seleccionar/interactuar con el objeto.
/// </summary>
[RequireComponent(typeof(Animator))]
public class MecanimStateController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Parámetros de Mecanim")]
    [Tooltip("Nombre del parámetro booleano o trigger en el Animator Controller.")]
    [SerializeField] private string isSelectedParam = "IsSelected";

    [Tooltip("Nombre del trigger de acción al hacer clic.")]
    [SerializeField] private string actionTriggerParam = "PlayAction";

    [Tooltip("Si es verdadero, vuelve automáticamente al estado Idle tras un tiempo.")]
    [SerializeField] private bool autoReturnToIdle = true;

    [Tooltip("Tiempo en segundos antes de retornar a Idle.")]
    [SerializeField] private float returnToIdleDelay = 2.5f;

    [Header("Retroalimentación Sonora (Opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectSound;

    private Animator animator;
    private Coroutine returnCoroutine;
    private bool isSelected = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Asegurar que inicie en estado Idle
        SetIdleState();
    }

    /// <summary>
    /// Evento disparado al hacer clic directo con el ratón sobre el collider o UI.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerSelectionAnimation();
    }

    private void OnMouseDown()
    {
        // Soporte adicional para objetos del mundo 2D con Collider2D
        TriggerSelectionAnimation();
    }

    /// <summary>
    /// Activa la animación del estado interactivo en el Animator de Mecanim.
    /// </summary>
    public void TriggerSelectionAnimation()
    {
        isSelected = true;

        if (animator != null)
        {
            if (!string.IsNullOrEmpty(isSelectedParam))
            {
                animator.SetBool(isSelectedParam, true);
            }

            if (!string.IsNullOrEmpty(actionTriggerParam))
            {
                animator.SetTrigger(actionTriggerParam);
            }
        }

        if (audioSource != null && selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }

        Debug.Log($"[MecanimStateController] Objeto '{gameObject.name}' seleccionado. Transición a estado de animación activa.");

        if (autoReturnToIdle)
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnToIdleRoutine());
        }
    }

    /// <summary>
    /// Corrutina para restablecer de forma limpia el estado a Idle.
    /// </summary>
    private IEnumerator ReturnToIdleRoutine()
    {
        yield return new WaitForSeconds(returnToIdleDelay);
        SetIdleState();
    }

    /// <summary>
    /// Restablece el Animator al estado de reposo (Idle).
    /// </summary>
    public void SetIdleState()
    {
        isSelected = false;
        if (animator != null && !string.IsNullOrEmpty(isSelectedParam))
        {
            animator.SetBool(isSelectedParam, false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Efecto sutil opcional al pasar cursor
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Salida de cursor
    }
}
