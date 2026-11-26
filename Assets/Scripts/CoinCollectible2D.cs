using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.8. La conexión a base de datos
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: CoinCollectible2D.cs
/// Responsabilidad: Representar la moneda recolectable interactiva en la escena 2D (Criterio 1):
/// 1. Detección física mediante Trigger Collider sin bloquear el paso del jugador.
/// 2. Animación senoidal de flotación y brillo para enriquecer la estética visual.
/// 3. Al ser recogida, suma el valor a las monedas del jugador, dispara el guardado en base de datos y se destruye.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinCollectible2D : MonoBehaviour
{
    [Header("Valor y Tipo")]
    [Tooltip("Cantidad de monedas que otorga este coleccionable.")]
    [SerializeField] private int coinValue = 1;

    [Header("Animación de Flotación")]
    [Tooltip("Amplitud del movimiento vertical senoidal.")]
    [SerializeField] private float bobHeight = 0.15f;

    [Tooltip("Velocidad de oscilación de la animación.")]
    [SerializeField] private float bobSpeed = 3.5f;

    [Tooltip("Velocidad de rotación visual sobre el eje Z o Y.")]
    [SerializeField] private float rotationSpeed = 90.0f;

    [Header("Efecto de Recolección")]
    [Tooltip("Prefab de chispas o partículas instanciado al recolectar la moneda.")]
    [SerializeField] private GameObject collectParticlePrefab;

    private Vector3 initialPosition;
    private bool isCollected = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        // Movimiento senoidal de flotación
        float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        // Giro visual
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D hitCollider)
    {
        if (isCollected) return;

        // Verificar que quien recolecta sea el personaje principal
        if (hitCollider.CompareTag("Player") || hitCollider.name.Contains("Player"))
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        isCollected = true;

        // Sumar al administrador central de base de datos
        if (GameProgressDatabaseManager2D.Instance != null)
        {
            GameProgressDatabaseManager2D.Instance.AddCoins(coinValue);
        }
        else
        {
            Debug.Log($"[CoinCollectible2D] Moneda recolectada (+{coinValue}).");
        }

        // Efecto de partículas
        SpawnCollectEffect();

        // Destrucción limpia del GameObject
        Destroy(gameObject);
    }

    private void SpawnCollectEffect()
    {
        if (collectParticlePrefab != null)
        {
            Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Efecto procedimental de destello dorado
            GameObject burstObj = new GameObject("CoinCollect_Burst");
            burstObj.transform.position = transform.position;

            ParticleSystem ps = burstObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 3.0f;
            main.startSize = 0.12f;
            main.startColor = new Color(1f, 0.85f, 0.1f, 1f); // Oro brillante
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 12) });

            ps.Play();
            Destroy(burstObj, 0.45f);
        }
    }
}
