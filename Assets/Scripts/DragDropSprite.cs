using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.3. Programación de botones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: DragDropSprite.cs
/// Responsabilidad: Permitir el arrastre funcional de sprites 2D en el espacio de juego
/// mediante eventos de ratón, cálculo de offset y retorno a posición original o encaje en zonas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DragDropSprite : MonoBehaviour
{
    [Header("Configuración de Arrastre")]
    [Tooltip("Cámara utilizada para calcular las coordenadas mundiales. Si es nula, se usa Camera.main.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Indica si el sprite debe retornar a su posición inicial al soltarlo.")]
    [SerializeField] private bool returnToOriginOnRelease = false;

    [Tooltip("Velocidad de interpolación de movimiento hacia la posición inicial.")]
    [SerializeField] private float returnSpeed = 10f;

    [Header("Efectos Visuales durante el Arrastre")]
    [Tooltip("Escala multiplicadora aplicada mientras se sostiene el sprite.")]
    [SerializeField] private float dragScaleMultiplier = 1.15f;

    [Tooltip("Orden de renderizado (sorting order) durante el arrastre para evitar solapamientos.")]
    [SerializeField] private int dragSortingOrder = 50;

    [Header("Eventos de Interacción")]
    public UnityEvent onBeginDragEvent;
    public UnityEvent onEndDragEvent;

    // Variables internas
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 mouseOffset;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        // Calcular el desplazamiento relativo entre el cursor y el centro del sprite
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseOffset = transform.position - mouseWorldPos;
        mouseOffset.z = 0f; // Mantener la profundidad bidimensional

        isDragging = true;

        // Feedback visual al iniciar arrastre
        transform.localScale = initialScale * dragScaleMultiplier;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = dragSortingOrder;
        }

        onBeginDragEvent?.Invoke();
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        // Actualizar la posición del objeto siguiendo el cursor
        Vector3 currentMousePos = GetMouseWorldPosition();
        Vector3 targetPos = currentMousePos + mouseOffset;
        targetPos.z = initialPosition.z; // Respetar la capa z

        transform.position = targetPos;
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Restaurar escala y orden de dibujo
        transform.localScale = initialScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        if (returnToOriginOnRelease)
        {
            StartCoroutine(SmoothReturnRoutine());
        }

        onEndDragEvent?.Invoke();
    }

    /// <summary>
    /// Convierte la posición de la pantalla del cursor en coordenadas del mundo 2D.
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = Mathf.Abs(targetCamera.transform.position.z - transform.position.z);
        return targetCamera.ScreenToWorldPoint(screenPoint);
    }

    /// <summary>
    /// Corrutina para interpolar el regreso suave del sprite a su posición de origen.
    /// </summary>
    private IEnumerator SmoothReturnRoutine()
    {
        while (Vector3.Distance(transform.position, initialPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * returnSpeed);
            yield return null;
        }
        transform.position = initialPosition;
    }

    /// <summary>
    /// Permite reiniciar la posición del sprite de forma manual (vinculable a un botón de la interfaz UI).
    /// </summary>
    public void ResetToInitialPosition()
    {
        StopAllCoroutines();
        transform.position = initialPosition;
        transform.localScale = initialScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
    }
}
