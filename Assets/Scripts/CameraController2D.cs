using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.4. Configuración de la cámara: videojuego en 2D
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: CameraController2D.cs
/// Responsabilidad: Gobernar el seguimiento cinemático de la cámara sobre el personaje:
/// 1. Seguimiento suave y fluido mediante Vector3.SmoothDamp ejecutado en LateUpdate() para evitar 'jittering'.
/// 2. Encuadre y escala proporcional (Orthographic Size = 5.0f) para que el juego no se vea ni muy lejos ni muy cerca.
/// 3. Desplazamiento (offset) vertical y anticipación horizontal (Look Ahead) según la dirección del movimiento.
/// 4. Delimitación de bordes (Bounding Box con Mathf.Clamp) para evitar mostrar áreas vacías fuera del nivel.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [Header("1. Objetivo de Seguimiento")]
    [Tooltip("Transform del personaje principal que la cámara debe enfocar.")]
    [SerializeField] private Transform target;

    [Tooltip("Distancia relativa (offset) respecto al personaje.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.25f, -10f);

    [Header("2. Suavizado Cinematográfico (Game Feel)")]
    [Tooltip("Tiempo de amortiguamiento para alcanzar al objetivo (SmoothDamp).")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float smoothTime = 0.15f;

    [Tooltip("Anticipación hacia el frente en el eje X según la orientación del jugador.")]
    [Range(0f, 3.0f)]
    [SerializeField] private float lookAheadFactor = 1.2f;

    [Header("3. Escala y Proyección Ortográfica")]
    [Tooltip("Tamaño ortográfico para una escala equilibrada (ni muy lejos ni muy cerca).")]
    [SerializeField] private float targetOrthographicSize = 5.0f;

    [Header("4. Confinamiento de Escena (Límites)")]
    [Tooltip("Habilita la restricción de bordes del escenario.")]
    [SerializeField] private bool enableBounds = true;

    [Tooltip("Límite inferior izquierdo (X mínimo, Y mínimo).")]
    [SerializeField] private Vector2 minBounds = new Vector2(-12.0f, -3.5f);

    [Tooltip("Límite superior derecho (X máximo, Y máximo).")]
    [SerializeField] private Vector2 maxBounds = new Vector2(12.0f, 5.5f);

    // Variables internas
    private Camera cam;
    private Vector3 currentVelocity = Vector3.zero;
    private float lookAheadPos = 0f;

    // Propiedades públicas para auditoría
    public bool HasTarget => target != null;
    public float OrthographicSize => cam != null ? cam.orthographicSize : targetOrthographicSize;
    public bool IsBoundsEnabled => enableBounds;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        // Forzar proyección ortográfica fija y proporcional
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = targetOrthographicSize;
        }

        // Búsqueda automática del jugador si no fue asignado en Inspector
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    private void Start()
    {
        // Posicionar de inmediato la cámara sobre el objetivo al iniciar para evitar paneos abruptos
        if (target != null)
        {
            Vector3 initialPos = target.position + offset;
            if (enableBounds)
            {
                initialPos.x = Mathf.Clamp(initialPos.x, minBounds.x, maxBounds.x);
                initialPos.y = Mathf.Clamp(initialPos.y, minBounds.y, maxBounds.y);
            }
            initialPos.z = offset.z;
            transform.position = initialPos;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcular anticipación hacia el frente según la velocidad X del personaje
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null && Mathf.Abs(targetRb.linearVelocity.x) > 0.1f)
        {
            lookAheadPos = Mathf.Sign(targetRb.linearVelocity.x) * lookAheadFactor;
        }
        else
        {
            lookAheadPos = Mathf.MoveTowards(lookAheadPos, 0f, Time.deltaTime * 3.0f);
        }

        // 2. Determinar punto de destino deseado con offset y anticipación
        Vector3 targetPosition = target.position + offset + new Vector3(lookAheadPos, 0f, 0f);

        // 3. Aplicar confinamiento de límites si está activo
        if (enableBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        // Preservar siempre la profundidad Z de la cámara ortográfica
        targetPosition.z = offset.z;

        // 4. Interpolación suave mediante SmoothDamp para movimiento fluido sin tirones
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (enableBounds)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
