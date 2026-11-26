using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.6. El movimiento de una bala
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: BulletMovementController2D.cs
/// Responsabilidad: Gobernar la cinemática balística y el ciclo de vida del proyectil:
/// 1. Trayectoria rectilínea uniforme a alta velocidad (16.0 m/s) sin afectación por gravedad (Gravity Scale = 0).
/// 2. Determinación inmediata del vector de desplazamiento según la orientación del jugador sin retardos ni desfases.
/// 3. Detección continua de colisiones sólidas (muros, plataformas, dianas) mediante triggers.
/// 4. Autodestrucción por tiempo de vida (3.0 s) o tras impacto para evitar fugas de memoria.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BulletMovementController2D : MonoBehaviour
{
    [Header("1. Cinemática Balística")]
    [Tooltip("Velocidad lineal de avance del proyectil (m/s).")]
    [SerializeField] private float bulletSpeed = 16.0f;

    [Tooltip("Tiempo máximo de vida útil en la escena antes de autodestruirse.")]
    [SerializeField] private float lifeTime = 3.0f;

    [Tooltip("Dirección horizontal del proyectil (+1 derecha, -1 izquierda).")]
    [SerializeField] private Vector2 travelDirection = Vector2.right;

    [Header("2. Capas de Impacto")]
    [Tooltip("Capas del entorno con las que la bala colisiona e impacta.")]
    [SerializeField] private LayerMask impactLayers;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasImpacted = false;

    // Propiedades públicas para auditoría
    public float Speed => bulletSpeed;
    public Vector2 Direction => travelDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Configuración requerida para proyectil balístico recto
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Trayectoria recta sin caída parabólica
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }

        if (col != null)
        {
            col.isTrigger = true; // Triggers para no desplazar al personaje que dispara
        }

        if (impactLayers.value == 0)
        {
            impactLayers = LayerMask.GetMask("Default", "Ground", "Piso", "Platforms", "Obstacles");
        }
    }

    private void Start()
    {
        // Iniciar trayectoria inmediatamente sin desfases
        LaunchProjectile();

        // Autodestrucción de seguridad para erradicar lag
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Establece la dirección de vuelo del proyectil y sincroniza su orientación visual.
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        travelDirection = newDirection.normalized;

        // Si la dirección es hacia la izquierda, voltear el sprite localmente
        if (travelDirection.x < 0f)
        {
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
        else
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }

        if (rb != null)
        {
            rb.linearVelocity = travelDirection * bulletSpeed;
        }
    }

    private void LaunchProjectile()
    {
        if (rb != null)
        {
            rb.linearVelocity = travelDirection * bulletSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasImpacted) return;

        // Ignorar colisiones con el propio jugador
        if (collision.CompareTag("Player") || collision.name.Contains("Player"))
        {
            return;
        }

        hasImpacted = true;
        Debug.Log($"[BulletMovementController2D] Bala impactó contra '{collision.name}'. Destrucción inmediata.");

        // Destrucción limpia al impactar superficie sólida
        Destroy(gameObject);
    }
}
