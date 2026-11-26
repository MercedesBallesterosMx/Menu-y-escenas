using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.4. Configuración de la cámara: videojuego en 2D
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: CameraParallaxBackground2D.cs
/// Responsabilidad: Proporcionar profundidad visual mediante efecto Parallax 2D.
/// Conforme la cámara principal se desplaza, el fondo montañoso se traslada a una fracción
/// de la velocidad relativa, simulando distancia atmosférica.
/// </summary>
public class CameraParallaxBackground2D : MonoBehaviour
{
    [Header("Configuración Parallax")]
    [Tooltip("Cámara de referencia para el cálculo de desplazamiento.")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Factor de movimiento relativo respecto a la cámara (0 = estático en mundo, 1 = pegado a cámara).")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxEffectX = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float parallaxEffectY = 0.15f;

    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            lastCameraPosition = cameraTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectX, deltaMovement.y * parallaxEffectY, 0f);

        lastCameraPosition = cameraTransform.position;
    }
}
