using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.5. Detección de colisiones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: BoundaryCollisionWall2D.cs
/// Responsabilidad: Configurar límites rígidos físicos (paredes, techo y piso) en 2D,
/// alineando automáticamente el BoxCollider2D a las dimensiones exactas del SpriteRenderer
/// para garantizar que ningún personaje u objeto traspase las superficies.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BoundaryCollisionWall2D : MonoBehaviour
{
    [Header("Configuración de Superficie y Físicas")]
    [Tooltip("Define si esta superficie actúa como suelo, pared izquierda, pared derecha o techo.")]
    [SerializeField] private BoundaryType boundaryType = BoundaryType.Floor;

    [Tooltip("Ajuste manual fino de márgenes (padding) sobre el colisionador.")]
    [SerializeField] private Vector2 colliderPadding = Vector2.zero;

    [Tooltip("Material físico 2D (fricción y rebote). Opcional.")]
    [SerializeField] private PhysicsMaterial2D physicsMaterial;

    public enum BoundaryType
    {
        Floor,
        WallLeft,
        WallRight,
        Ceiling,
        Obstacle
    }

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ConfigureAlignedCollider();
    }

    /// <summary>
    /// Ajusta las dimensiones del BoxCollider2D de forma exacta a los bordes
    /// del SpriteRenderer para evitar cualquier solapamiento o desalineación visual.
    /// </summary>
    [ContextMenu("Auto-Alinear Colisionador al Sprite")]
    public void ConfigureAlignedCollider()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
            boxCollider.size = new Vector2(spriteSize.x + colliderPadding.x, spriteSize.y + colliderPadding.y);
            boxCollider.offset = Vector2.zero;
        }

        if (physicsMaterial != null)
        {
            boxCollider.sharedMaterial = physicsMaterial;
        }

        // Asignar etiqueta para identificación en el sistema de colisiones
        gameObject.tag = "Obstaculo";
    }

    private void OnValidate()
    {
        ConfigureAlignedCollider();
    }
}
