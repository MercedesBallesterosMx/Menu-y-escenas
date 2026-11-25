using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.3. Programación de botones y arrastre
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: DragDropSprite.cs
/// Responsabilidad: Permitir el arrastre funcional de objetos (Sprites 2D y Cubos/Objetos 3D)
/// mediante eventos de ratón (OnMouseDown, OnMouseDrag, OnMouseUp), cálculo de profundidad
/// de pantalla y retorno opcional a posición inicial.
/// </summary>
public class DragDropSprite : MonoBehaviour
{
    [Header("Configuración de Arrastre")]
    [Tooltip("Cámara utilizada para calcular las coordenadas. Si es nula, se usa Camera.main.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Indica si el objeto debe retornar a su posición inicial al soltarlo.")]
    [SerializeField] private bool returnToOriginOnRelease = false;

    [Tooltip("Velocidad de interpolación de movimiento hacia la posición inicial.")]
    [SerializeField] private float returnSpeed = 10f;

    [Header("Efectos Visuales durante el Arrastre")]
    [Tooltip("Escala multiplicadora aplicada mientras se sostiene el objeto.")]
    [SerializeField] private float dragScaleMultiplier = 1.15f;

    [Tooltip("Orden de renderizado (sorting order) durante el arrastre (solo para Sprites 2D).")]
    [SerializeField] private int dragSortingOrder = 50;

    [Header("Eventos de Interacción")]
    public UnityEvent onBeginDragEvent;
    public UnityEvent onEndDragEvent;

    // Variables internas
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 screenPoint;
    private Vector3 mouseOffset;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Rigidbody rb3D;
    private Rigidbody2D rb2D;

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

        rb3D = GetComponent<Rigidbody>();
        rb2D = GetComponent<Rigidbody2D>();

        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        // Calcular profundidad de pantalla del objeto
        screenPoint = targetCamera.WorldToScreenPoint(transform.position);

        // Calcular offset exacto entre el clic del ratón y el centro del objeto
        Vector3 curScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        mouseOffset = transform.position - targetCamera.ScreenToWorldPoint(curScreenPos);

        isDragging = true;

        // Si tiene físicas, desactivar temporalmente cinemática para arrastrar con libertad
        if (rb3D != null) rb3D.isKinematic = true;
        if (rb2D != null) rb2D.bodyType = RigidbodyType2D.Kinematic;

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
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = targetCamera.ScreenToWorldPoint(curScreenPoint) + mouseOffset;

        transform.position = curPosition;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // Restaurar escala y orden de dibujo
        transform.localScale = initialScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        // Restaurar físicas si tenía
        if (rb3D != null) rb3D.isKinematic = false;
        if (rb2D != null) rb2D.bodyType = RigidbodyType2D.Dynamic;

        if (returnToOriginOnRelease)
        {
            StartCoroutine(SmoothReturnRoutine());
        }

        onEndDragEvent?.Invoke();
    }

    private IEnumerator SmoothReturnRoutine()
    {
        while (Vector3.Distance(transform.position, initialPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * returnSpeed);
            yield return null;
        }
        transform.position = initialPosition;
    }

    public void ResetToInitialPosition()
    {
        StopAllCoroutines();
        transform.position = initialPosition;
        transform.localScale = initialScale;
    }
}
