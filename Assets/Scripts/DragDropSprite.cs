using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.3. Programación de botones y arrastre
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: DragDropSprite.cs
/// Responsabilidad: Permitir el arrastre interactivo universal de objetos (Sprites 2D y Objetos 3D como Cubos)
/// con límites delimitados de movimiento (para evitar que se salga de la pantalla o del área de juego),
/// retorno elástico a posición inicial y soporte para Unity 6 Input System.
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

    [Header("Límites de Movimiento (Delimitación)")]
    [Tooltip("Impide que el objeto salga de los bordes visibles de la pantalla.")]
    [SerializeField] private bool clampToScreenBounds = true;

    [Tooltip("Margen de separación respecto a los bordes de la pantalla.")]
    [Range(0.02f, 0.3f)]
    [SerializeField] private float screenMargin = 0.08f;

    [Tooltip("Activar límites manuales fijos en coordenadas X / Y del mundo.")]
    [SerializeField] private bool useManualWorldLimits = false;
    [SerializeField] private Vector2 minWorldBounds = new Vector2(-6f, -3.5f);
    [SerializeField] private Vector2 maxWorldBounds = new Vector2(6f, 3.5f);

    [Header("Efectos Visuales durante el Arrastre")]
    [Tooltip("Escala multiplicadora aplicada mientras se sostiene el objeto.")]
    [SerializeField] private float dragScaleMultiplier = 1.15f;

    [Tooltip("Orden de renderizado (sorting order) durante el arrastre (solo para Sprites 2D).")]
    [SerializeField] private int dragSortingOrder = 50;

    [Header("Eventos de Interacción")]
    public UnityEvent onBeginDragEvent;
    public UnityEvent onEndDragEvent;

    // Componentes y variables internas
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 screenPoint;
    private Vector3 mouseOffset;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Rigidbody rb3D;
    private Rigidbody2D rb2D;
    private Collider col3D;
    private Collider2D col2D;

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
        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();

        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    private void Update()
    {
        bool pointerDown = false;
        bool pointerHeld = false;
        bool pointerUp = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            pointerDown = Mouse.current.leftButton.wasPressedThisFrame;
            pointerHeld = Mouse.current.leftButton.isPressed;
            pointerUp = Mouse.current.leftButton.wasReleasedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!pointerDown && !pointerHeld)
        {
            pointerDown = Input.GetMouseButtonDown(0);
            pointerHeld = Input.GetMouseButton(0);
            pointerUp = Input.GetMouseButtonUp(0);
        }
#endif

        if (pointerDown && !isDragging)
        {
            if (CheckMouseRaycastHit())
            {
                StartDrag();
            }
        }
        else if (isDragging)
        {
            if (pointerUp || !pointerHeld)
            {
                EndDrag();
            }
            else
            {
                UpdateDrag();
            }
        }
    }

    private Vector3 GetPointerScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            return new Vector3(pos.x, pos.y, 0f);
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector3.zero;
#endif
    }

    private bool CheckMouseRaycastHit()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return false;

        Vector3 mouseScreen = GetPointerScreenPosition();
        Ray ray = targetCamera.ScreenPointToRay(mouseScreen);

        // Detección 3D (Cubo, Esferas, etc.)
        if (col3D != null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                if (hit.collider == col3D || hit.transform == transform)
                {
                    return true;
                }
            }
        }

        // Detección 2D (Sprites)
        if (col2D != null)
        {
            Vector2 worldPoint2D = targetCamera.ScreenToWorldPoint(mouseScreen);
            RaycastHit2D hit2D = Physics2D.Raycast(worldPoint2D, Vector2.zero);
            if (hit2D.collider != null && (hit2D.collider == col2D || hit2D.transform == transform))
            {
                return true;
            }
        }

        return false;
    }

    public void StartDrag()
    {
        if (isDragging) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        StopAllCoroutines(); // Detener cualquier retorno pendiente
        screenPoint = targetCamera.WorldToScreenPoint(transform.position);
        Vector3 mouseScreen = GetPointerScreenPosition();
        mouseScreen.z = screenPoint.z;
        mouseOffset = transform.position - targetCamera.ScreenToWorldPoint(mouseScreen);

        isDragging = true;

        if (rb3D != null) rb3D.isKinematic = true;
        if (rb2D != null) rb2D.bodyType = RigidbodyType2D.Kinematic;

        transform.localScale = initialScale * dragScaleMultiplier;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = dragSortingOrder;
        }

        onBeginDragEvent?.Invoke();
    }

    public void UpdateDrag()
    {
        if (!isDragging) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        Vector3 mouseScreen = GetPointerScreenPosition();
        mouseScreen.z = screenPoint.z;
        Vector3 newWorldPos = targetCamera.ScreenToWorldPoint(mouseScreen) + mouseOffset;

        // 1. Aplicar límite de bordes de pantalla
        if (clampToScreenBounds)
        {
            Vector3 vp = targetCamera.WorldToViewportPoint(newWorldPos);
            vp.x = Mathf.Clamp(vp.x, screenMargin, 1f - screenMargin);
            vp.y = Mathf.Clamp(vp.y, screenMargin, 1f - screenMargin);
            newWorldPos = targetCamera.ViewportToWorldPoint(vp);
        }

        // 2. Aplicar límites manuales de coordenadas mundiales si están habilitados
        if (useManualWorldLimits)
        {
            newWorldPos.x = Mathf.Clamp(newWorldPos.x, minWorldBounds.x, maxWorldBounds.x);
            newWorldPos.y = Mathf.Clamp(newWorldPos.y, minWorldBounds.y, maxWorldBounds.y);
        }

        transform.position = newWorldPos;
    }

    public void EndDrag()
    {
        if (!isDragging) return;
        isDragging = false;

        transform.localScale = initialScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        if (rb3D != null) rb3D.isKinematic = false;
        if (rb2D != null) rb2D.bodyType = RigidbodyType2D.Dynamic;

        if (returnToOriginOnRelease)
        {
            StartCoroutine(SmoothReturnRoutine());
        }

        onEndDragEvent?.Invoke();
    }

    // Callbacks nativos de Unity (compatibilidad cruzada)
    private void OnMouseDown()
    {
        StartDrag();
    }

    private void OnMouseDrag()
    {
        UpdateDrag();
    }

    private void OnMouseUp()
    {
        EndDrag();
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
