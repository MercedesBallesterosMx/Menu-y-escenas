using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.7. Efectos de acción
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: MissileActionEffects2D.cs
/// Responsabilidad: Gobernar los efectos de acción y la cinemática de la trayectoria del misil/bala:
/// 1. Garantiza Criterio 1: Cero afectación gravitatoria (gravityScale = 0).
/// 2. Garantiza Criterio 2: Trayectoria lineal y constante a velocidad fija sin caídas ni desvíos.
/// 3. Genera efectos de acción visuales: TrailRenderer de estela de propulsión, partículas de chispa/humo y efecto de impacto al colisionar.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MissileActionEffects2D : MonoBehaviour
{
    [Header("1. Cinemática y Gravedad Cero (Criterios 1 y 2)")]
    [Tooltip("Velocidad lineal constante de la trayectoria (m/s).")]
    [SerializeField] private float flightSpeed = 16.0f;

    [Tooltip("Dirección de avance del misil/proyectil.")]
    [SerializeField] private Vector2 travelDirection = Vector2.right;

    [Tooltip("Tiempo máximo de vida útil en escena si no colisiona.")]
    [SerializeField] private float maxLifetime = 3.0f;

    [Header("2. Efectos de Acción: Estela y Partículas")]
    [Tooltip("Referencia al componente TrailRenderer que dibuja la estela del misil.")]
    [SerializeField] private TrailRenderer trailRenderer;

    [Tooltip("Color inicial de la estela en la punta del misil (alta energía).")]
    [SerializeField] private Color trailStartColor = new Color(0.2f, 0.85f, 1f, 1f); // Cyan brillante

    [Tooltip("Color final de la estela al disiparse (humo translúcido).")]
    [SerializeField] private Color trailEndColor = new Color(1f, 0.45f, 0.1f, 0f); // Naranja translúcido

    [Tooltip("Ancho inicial de la estela visual.")]
    [SerializeField] private float trailStartWidth = 0.22f;

    [Tooltip("Tiempo de permanencia de la estela en pantalla (segundos).")]
    [SerializeField] private float trailTime = 0.25f;

    [Header("3. Efecto de Impacto / Explosión")]
    [Tooltip("Prefab o efecto visual instanciado al colisionar.")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasCollided = false;

    public float FlightSpeed => flightSpeed;
    public Vector2 TravelDirection => travelDirection;
    public bool HasZeroGravity => rb != null && rb.gravityScale == 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Criterio 1: El proyectil no debe verse afectado por la gravedad
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Gravedad exactamente cero
            rb.linearDamping = 0f; // Cero fricción en el aire para velocidad estrictamente constante
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }

        if (col != null)
        {
            col.isTrigger = true;
        }

        // Configuración automática del TrailRenderer si no existe
        SetupTrailEffect();
    }

    private void Start()
    {
        ApplyConstantLinearVelocity();
        Destroy(gameObject, maxLifetime);
    }

    private void FixedUpdate()
    {
        // Criterio 2: Mantener la trayectoria estrictamente lineal y constante en cada paso de física
        if (!hasCollided && rb != null)
        {
            rb.linearVelocity = travelDirection * flightSpeed;
        }
    }

    /// <summary>
    /// Configura dinámicamente la estela de acción (TrailRenderer) del proyectil
    /// </summary>
    private void SetupTrailEffect()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }
        }

        if (trailRenderer != null)
        {
            trailRenderer.time = trailTime;
            trailRenderer.startWidth = trailStartWidth;
            trailRenderer.endWidth = 0.02f;
            trailRenderer.autodestruct = false;

            // Degradado de color: Cyan brillante -> Naranja propulsión -> Humo
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(trailStartColor, 0.0f),
                    new GradientColorKey(new Color(1f, 0.6f, 0.1f), 0.6f),
                    new GradientColorKey(trailEndColor, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.7f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trailRenderer.colorGradient = gradient;

            // Material sin iluminación para que brille intensamente
            if (trailRenderer.material == null || trailRenderer.material.name.Contains("Default"))
            {
                Shader unlitShader = Shader.Find("Sprites/Default");
                if (unlitShader != null)
                {
                    trailRenderer.material = new Material(unlitShader);
                }
            }
        }
    }

    /// <summary>
    /// Inicializa la dirección de avance y orienta el sprite hacia donde vuela
    /// </summary>
    public void InitializeTrajectory(Vector2 direction, float speed)
    {
        travelDirection = direction.normalized;
        flightSpeed = speed;

        // Inversión horizontal visual
        Vector3 currentScale = transform.localScale;
        currentScale.x = (travelDirection.x < 0f) ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);
        transform.localScale = currentScale;

        ApplyConstantLinearVelocity();
    }

    private void ApplyConstantLinearVelocity()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = travelDirection * flightSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D hitCollider)
    {
        if (hasCollided) return;

        // Ignorar colisiones con el personaje que dispara
        if (hitCollider.CompareTag("Player") || hitCollider.name.Contains("Player"))
        {
            return;
        }

        hasCollided = true;
        SpawnImpactEffect();
        Destroy(gameObject);
    }

    /// <summary>
    /// Instancia chispas / partículas de explosión de impacto
    /// </summary>
    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Efecto de impacto procedimental si no hay prefab asignado
            GameObject impactObj = new GameObject("MissileImpact_Burst");
            impactObj.transform.position = transform.position;

            ParticleSystem ps = impactObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.25f;
            main.startSpeed = 4.0f;
            main.startSize = 0.15f;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.2f), new Color(1f, 0.3f, 0f));
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 15) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            ps.Play();
            Destroy(impactObj, 0.4f);
        }
    }
}
