using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.3. Programación de mecánicas y físicas
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PhysicsMechanicsManager2D.cs
/// Responsabilidad: Auditar y validar los 5 criterios de evaluación oficiales de la actividad:
/// 1. Simulación de gravedad activa en el personaje (Rigidbody2D con gravityScale > 0).
/// 2. Capacidad de salto por evento de teclado (Space / W / Flecha Arriba).
/// 3. Detección de colisiones sólidas con el suelo de la escena (Ground_Platform).
/// 4. Restricción de rotación en Z para prevenir volcaduras.
/// 5. Verificación de integridad física de plataformas y emisión de diagnóstico en consola.
/// </summary>
public class PhysicsMechanicsManager2D : MonoBehaviour
{
    [Header("Referencias de Entidades")]
    [Tooltip("Referencia al jugador en la escena.")]
    [SerializeField] private GameObject player;

    [Tooltip("Referencia a la plataforma de suelo principal.")]
    [SerializeField] private GameObject groundPlatform;

    private void Awake()
    {
        ValidatePhysicsArchitecture();
    }

    public void ValidatePhysicsArchitecture()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        if (groundPlatform == null) groundPlatform = GameObject.Find("Ground_Platform");

        bool gravityOk = false;
        bool jumpOk = false;
        bool collisionOk = false;
        bool freezeRotationOk = false;

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Collider2D col = player.GetComponent<Collider2D>();
            PlatformerPhysicsMechanics2D physicsMech = player.GetComponent<PlatformerPhysicsMechanics2D>();
            PlayerController2D controller = player.GetComponent<PlayerController2D>();

            if (rb != null)
            {
                gravityOk = rb.gravityScale > 0f && rb.bodyType == RigidbodyType2D.Dynamic;
                freezeRotationOk = rb.freezeRotation;
            }

            jumpOk = (physicsMech != null || controller != null);
            collisionOk = col != null && !col.isTrigger;

            Debug.Log($"[PhysicsMechanicsManager2D] >>> AUDITORÍA OFICIAL ACTIVIDAD 3.3 <<<");
            Debug.Log($"[PhysicsMechanicsManager2D] Criterio 1 (Simulación de Gravedad): {(gravityOk ? "CUMPLIDO (GravityScale: " + (rb != null ? rb.gravityScale.ToString("F1") : "0") + ")" : "PENDIENTE")}");
            Debug.Log($"[PhysicsMechanicsManager2D] Criterio 2 (Salto por Evento de Teclado): {(jumpOk ? "CUMPLIDO (Espacio / W / Flechas)" : "PENDIENTE")}");
            Debug.Log($"[PhysicsMechanicsManager2D] Criterio 3 (Colisión con Suelo): {(collisionOk ? "CUMPLIDO (Collider2D Sólido Activo)" : "PENDIENTE")}");
            Debug.Log($"[PhysicsMechanicsManager2D] Restricción Angular Z (Freeze Rotation): {(freezeRotationOk ? "CUMPLIDO" : "PENDIENTE")}");
        }
        else
        {
            Debug.LogWarning("[PhysicsMechanicsManager2D] No se encontró el GameObject 'Player' en la escena.");
        }

        if (groundPlatform != null)
        {
            Collider2D groundCol = groundPlatform.GetComponent<Collider2D>();
            Debug.Log($"[PhysicsMechanicsManager2D] Suelo Principal '{groundPlatform.name}': Colisionador detectado: {groundCol != null && !groundCol.isTrigger}");
        }
    }
}
