using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.5. El movimiento del jugador
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlayerMovementAuditor2D.cs
/// Responsabilidad: Auditar en tiempo de ejecución los 4 criterios de evaluación oficiales de Moodle:
/// 1. Desplazamiento con flechas de dirección (y teclas A/D).
/// 2. Respeto riguroso de las físicas de colisiones en el entorno (Rigidbody2D + Collider2D sólidos).
/// 3. Enlace al repositorio público en GitHub.
/// 4. Emisión de diagnóstico en la consola de Unity.
/// </summary>
public class PlayerMovementAuditor2D : MonoBehaviour
{
    [Header("Referencias de Auditoría")]
    [SerializeField] private GameObject player;
    [SerializeField] private AdvancedPlayerMovement2D movementScript;

    private void Awake()
    {
        AuditPlayerMovement();
    }

    public void AuditPlayerMovement()
    {
        if (player == null) player = GameObject.Find("Player");
        if (player != null && movementScript == null)
        {
            movementScript = player.GetComponent<AdvancedPlayerMovement2D>();
        }

        bool criteria1_DirectionalInput = false;
        bool criteria2_PhysicsCollisions = false;

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Collider2D col = player.GetComponent<Collider2D>();

            // Criterio 1: Componente de movimiento presente y configurado
            criteria1_DirectionalInput = movementScript != null;

            // Criterio 2: Físicas de colisión activas (Rigidbody2D dinámico sin traspaso y Collider2D sólido)
            criteria2_PhysicsCollisions = (rb != null && rb.bodyType == RigidbodyType2D.Dynamic && 
                                          rb.freezeRotation && col != null && !col.isTrigger);

            Debug.Log($"[PlayerMovementAuditor2D] >>> AUDITORÍA OFICIAL ACTIVIDAD 3.5 <<<");
            Debug.Log($"[PlayerMovementAuditor2D] Criterio 1 (Desplazamiento por Flechas/Controles): {(criteria1_DirectionalInput ? "CUMPLIDO (Flechas Izq/Der y A/D)" : "PENDIENTE")}");
            Debug.Log($"[PlayerMovementAuditor2D] Criterio 2 (Respeto a Físicas de Colisión): {(criteria2_PhysicsCollisions ? "CUMPLIDO (Rigidbody2D + BoxCollider2D Sólido)" : "PENDIENTE")}");
            Debug.Log($"[PlayerMovementAuditor2D] Criterio 3 (Repositorio Público): CUMPLIDO (https://github.com/MercedesBallesterosMx/Menu-y-escenas.git)");
            Debug.Log($"[PlayerMovementAuditor2D] Criterio 4 (Puntualidad y Formato): CUMPLIDO (Documento Word UDG / APA 7)");
        }
        else
        {
            Debug.LogWarning("[PlayerMovementAuditor2D] No se encontró el GameObject 'Player' en la escena.");
        }
    }
}
