using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.1. La escena principal y la estructura del personaje
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: GameSceneInitializer2D.cs
/// Responsabilidad: Configurar y validar la escena principal del videojuego en 2D:
/// asegura que la cámara opere en modo ortográfico, valida la presencia del personaje principal,
/// ajusta la gravedad bidimensional y confirma la correcta disposición de las capas de suelo.
/// </summary>
public class GameSceneInitializer2D : MonoBehaviour
{
    [Header("Configuración de Cámara 2D")]
    [Tooltip("Cámara principal del juego. Si no se asigna, busca Camera.main.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Tamaño ortográfico para delimitar el área visible de la escena.")]
    [SerializeField] private float orthographicCameraSize = 5.0f;

    [Tooltip("Color de fondo de la cámara (Skybox / Fondo 2D).")]
    [SerializeField] private Color backgroundColor = new Color(0.18f, 0.22f, 0.28f, 1.0f);

    [Header("Referencias de Entidades 2D")]
    [Tooltip("Referencia al personaje principal de la escena.")]
    [SerializeField] private GameObject playerCharacter;

    [Tooltip("Nombre de la capa asignada al suelo/plataformas.")]
    [SerializeField] private string groundLayerName = "Ground";

    [Header("Físicas del Mundo 2D")]
    [Tooltip("Magnitud de la gravedad bidimensional.")]
    [SerializeField] private Vector2 worldGravity = new Vector2(0f, -19.62f);

    private void Awake()
    {
        // 1. Configurar físicas del entorno 2D
        Physics2D.gravity = worldGravity;

        // 2. Configurar cámara en modo ortográfico estricto para 2D
        Configure2DCamera();

        // 3. Validar estructura del personaje
        ValidatePlayerStructure();
    }

    private void Configure2DCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = orthographicCameraSize;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = backgroundColor;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            Debug.Log("[GameSceneInitializer2D] Cámara configurada exitosamente en modo Ortográfico 2D.");
        }
        else
        {
            Debug.LogWarning("[GameSceneInitializer2D] No se encontró Main Camera en la escena.");
        }
    }

    private void ValidatePlayerStructure()
    {
        if (playerCharacter == null)
        {
            playerCharacter = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerCharacter != null)
        {
            // Validar Rigidbody2D
            Rigidbody2D rb = playerCharacter.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            // Validar Collider2D
            Collider2D col = playerCharacter.GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning("[GameSceneInitializer2D] El personaje no tiene un Collider2D asignado.");
            }

            // Validar controladores
            PlayerController2D controller = playerCharacter.GetComponent<PlayerController2D>();
            PlayerAnimationStateController2D animController = playerCharacter.GetComponent<PlayerAnimationStateController2D>();

            Debug.Log($"[GameSceneInitializer2D] Personaje '{playerCharacter.name}' validado correctamente. " +
                      $"(Rigidbody2D: {rb != null}, Collider2D: {col != null}, Controller: {controller != null}, AnimController: {animController != null})");
        }
        else
        {
            Debug.LogWarning("[GameSceneInitializer2D] No se encontró el GameObject del personaje principal.");
        }
    }
}
