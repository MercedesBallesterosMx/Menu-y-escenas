using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.2. Los prefabs
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: BulletProjectile2D.cs
/// Responsabilidad: Gestionar el comportamiento físico de la bala instanciada desde el Prefab:
/// desplazamiento direccional lineal mediante Rigidbody2D, autodestrucción por tiempo de vida
/// (prevención de fugas de memoria) y detección de colisiones con obstáculos y plataformas.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BulletProjectile2D : MonoBehaviour
{
    [Header("Propiedades Físicas del Proyectil")]
    [Tooltip("Velocidad lineal de desplazamiento de la bala.")]
    [SerializeField] private float speed = 14.0f;

    [Tooltip("Tiempo máximo de vida en segundos antes de destruirse automáticamente.")]
    [SerializeField] private float lifeTime = 3.0f;

    [Tooltip("Daño base infligido al colisionar.")]
    [SerializeField] private int damage = 25;

    [Header("Efectos Visuales de Impacto")]
    [Tooltip("Prefab o efecto de partículas generado al impactar.")]
    [SerializeField] private GameObject impactEffectPrefab;

    // Componentes internos
    private Rigidbody2D rb2D;
    private Collider2D col2D;
    private Vector2 moveDirection = Vector2.right;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<Collider2D>();

        // Configuración óptima de físicas para proyectil rápido 2D
        rb2D.gravityScale = 0f; // Vuelo rectilíneo sin caída prematura
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Evita atravesar paredes delgadas
        col2D.isTrigger = true; // Permite atravesar zonas de disparo sin trabar al personaje
    }

    private void Start()
    {
        // Aplicar velocidad inicial en la dirección asignada
        rb2D.linearVelocity = moveDirection * speed;

        // Autodestrucción garantizada tras cumplirse el tiempo de vida
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Configura la dirección y orientación visual de la bala según el frente del personaje.
    /// </summary>
    /// <param name="direction">Vector normalizado de dirección (-1 para izquierda, +1 para derecha).</param>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // Orientar el sprite según el vector de avance
        if (moveDirection.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Si el proyectil ya inició, refrescar la velocidad física
        if (rb2D != null)
        {
            rb2D.linearVelocity = moveDirection * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar colisiones con el personaje que disparó la bala
        if (collision.CompareTag("Player") || collision.isTrigger)
        {
            return;
        }

        // Generar efecto de impacto si está configurado
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"[BulletProjectile2D] Bala impactó con: '{collision.gameObject.name}'. Destruyendo proyectil.");

        // Destruir el proyectil al impactar con suelo, paredes u obstáculos
        Destroy(gameObject);
    }
}
