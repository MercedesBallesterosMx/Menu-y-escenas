using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.3. Programación de mecánicas y físicas
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlatformEffectorController2D.cs
/// Responsabilidad: Configurar plataformas semi-sólidas unidireccionales (One-Way Platforms)
/// utilizando PlatformEffector2D y BoxCollider2D (usedByEffector = true).
/// Permite que el personaje salte a través de ellas desde abajo sin bloquearse,
/// aterrizando firmemente en la superficie superior.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlatformEffector2D))]
public class PlatformEffectorController2D : MonoBehaviour
{
    [Header("Configuración del Efector Unidireccional")]
    [Tooltip("Ángulo de la superficie de contacto que bloquea al personaje (180° = semicírculo superior).")]
    [SerializeField] private float surfaceArc = 170.0f;

    [Tooltip("Permite atravesar la plataforma hacia abajo al presionar agacharse + salto.")]
    [SerializeField] private bool allowDropThrough = true;

    private PlatformEffector2D effector;
    private BoxCollider2D col;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        effector = GetComponent<PlatformEffector2D>();

        // Configuración requerida de Unity 2D Physics
        col.usedByEffector = true;
        effector.useOneWay = true;
        effector.surfaceArc = surfaceArc;
        effector.useOneWayGrouping = true;
    }
}
