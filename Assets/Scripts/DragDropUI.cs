using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Script: DragDropUI.cs
/// Responsabilidad: Permitir el arrastre libre de cualquier botón o elemento del Canvas (UI)
/// con el ratón durante la ejecución del juego (Game View).
/// </summary>
public class DragDropUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Se activa al hacer clic
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        Debug.Log($"[DragDropUI] Arrastrando elemento UI: {gameObject.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null && rectTransform != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }

    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}
