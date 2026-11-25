using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.5. Detección de colisiones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: InteractiveTargetZone2D.cs
/// Responsabilidad: Definir zonas interactivas de destino (Drop/Snap Zone).
/// Al colisionar con el sprite correspondiente, encaja el objeto en la posición meta,
/// bloquea su física/arrastre y cambia el estado de la escena (evento completado).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InteractiveTargetZone2D : MonoBehaviour
{
    [Header("Configuración de Zona Meta")]
    [Tooltip("Etiqueta o nombre del objeto que debe encajar en esta zona.")]
    [SerializeField] private string acceptedObjectTag = "InteractiveItem";

    [Tooltip("Si es verdadero, el objeto colisionado se ajusta automáticamente al centro de esta zona.")]
    [SerializeField] private bool snapToCenter = true;

    [Tooltip("Color de iluminación de la zona cuando el objeto está dentro.")]
    [SerializeField] private Color zoneActiveColor = new Color(0.2f, 1f, 0.4f, 0.8f);

    [Header("Eventos de Meta")]
    public UnityEvent onTargetSnappedEvent;

    private SpriteRenderer zoneRenderer;
    private Color originalZoneColor;
    private bool isOccupied = false;

    private void Awake()
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (zoneRenderer != null)
        {
            originalZoneColor = zoneRenderer.color;
        }

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true; // Zona detectora tipo trigger
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOccupied) return;

        if (string.IsNullOrEmpty(acceptedObjectTag) || other.CompareTag(acceptedObjectTag))
        {
            SnapObjectIntoZone(other.gameObject);
        }
    }

    private void SnapObjectIntoZone(GameObject targetObj)
    {
        isOccupied = true;

        if (snapToCenter)
        {
            targetObj.transform.position = transform.position;
        }

        // Si tiene Rigidbody, congelar su movimiento
        Rigidbody2D rb = targetObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (zoneRenderer != null)
        {
            zoneRenderer.color = zoneActiveColor;
        }

        Debug.Log($"[InteractiveTargetZone2D] Objeto '{targetObj.name}' encajado exitosamente en la zona '{gameObject.name}'.");
        onTargetSnappedEvent?.Invoke();
    }

    /// <summary>
    /// Restablece la zona permitiendo nuevas interacciones.
    /// </summary>
    public void ResetZone()
    {
        isOccupied = false;
        if (zoneRenderer != null)
        {
            zoneRenderer.color = originalZoneColor;
        }
    }
}
