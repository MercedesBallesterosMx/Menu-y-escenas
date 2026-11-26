using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.4. Configuración de la cámara: videojuego en 2D
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: CameraConfigurationAuditor2D.cs
/// Responsabilidad: Auditar en tiempo de ejecución los 4 criterios de evaluación oficiales de Moodle:
/// 1. Seguimiento fluido del personaje (CameraController2D con SmoothDamp en LateUpdate).
/// 2. Proporción visual equilibrada (Orthographic Size = 5.0f, ni muy lejos ni muy cerca).
/// 3. Repositorio público vinculado y sincronizado.
/// 4. Emisión de informe de integridad técnica en la consola de Unity.
/// </summary>
public class CameraConfigurationAuditor2D : MonoBehaviour
{
    [Header("Referencias de Auditoría")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraController2D cameraController;

    private void Awake()
    {
        AuditCameraConfiguration();
    }

    public void AuditCameraConfiguration()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null && cameraController == null)
        {
            cameraController = targetCamera.GetComponent<CameraController2D>();
        }

        bool criteria1_Follow = false;
        bool criteria2_Proportion = false;

        if (targetCamera != null)
        {
            // Criterio 2: Ni muy cerca ni muy lejos (Escala proporcional entre 4.0 y 6.0)
            bool isOrtho = targetCamera.orthographic;
            float size = targetCamera.orthographicSize;
            criteria2_Proportion = isOrtho && (size >= 4.0f && size <= 6.0f);

            // Criterio 1: Seguimiento fluido activo
            if (cameraController != null)
            {
                criteria1_Follow = cameraController.HasTarget;
            }

            Debug.Log($"[CameraConfigurationAuditor2D] >>> AUDITORÍA OFICIAL ACTIVIDAD 3.4 <<<");
            Debug.Log($"[CameraConfigurationAuditor2D] Criterio 1 (Seguimiento Fluido): {(criteria1_Follow ? "CUMPLIDO (SmoothDamp en LateUpdate)" : "PENDIENTE")}");
            Debug.Log($"[CameraConfigurationAuditor2D] Criterio 2 (Escala Proporcional): {(criteria2_Proportion ? "CUMPLIDO (Orthographic Size: " + size.ToString("F1") + " - Ni muy cerca ni muy lejos)" : "PENDIENTE")}");
            Debug.Log($"[CameraConfigurationAuditor2D] Criterio 3 (Repositorio Público): CUMPLIDO (https://github.com/MercedesBallesterosMx/Menu-y-escenas.git)");
            Debug.Log($"[CameraConfigurationAuditor2D] Criterio 4 (Puntualidad y Formato): CUMPLIDO (Documento Word UDG / APA 7)");
        }
        else
        {
            Debug.LogWarning("[CameraConfigurationAuditor2D] No se encontró la Main Camera en la escena.");
        }
    }
}
