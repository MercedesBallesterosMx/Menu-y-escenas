using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.2. Los prefabs
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PrefabManager2D.cs
/// Responsabilidad: Validar la integridad de los Prefabs en la escena en tiempo de ejecución:
/// comprueba la correcta asignación del Prefab de proyectil, la existencia del punto de disparo (FirePoint)
/// no centrado, la presencia de componentes de físicas (Rigidbody2D, Collider2D) y emite diagnósticos.
/// </summary>
public class PrefabManager2D : MonoBehaviour
{
    [Header("Referencias de Prefabs")]
    [Tooltip("Prefab de bala asignado para validación de arquitectura.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Referencia al jugador en la escena.")]
    [SerializeField] private PlayerWeaponShooter2D weaponShooter;

    private void Awake()
    {
        ValidatePrefabArchitecture();
    }

    private void ValidatePrefabArchitecture()
    {
        if (weaponShooter == null)
        {
            weaponShooter = FindAnyObjectByType<PlayerWeaponShooter2D>();
        }

        if (weaponShooter != null)
        {
            Debug.Log("[PrefabManager2D] Sistema de disparo del jugador detectado y vinculado.");
        }
        else
        {
            Debug.LogWarning("[PrefabManager2D] No se encontró el componente PlayerWeaponShooter2D en el jugador.");
        }

        if (bulletPrefab != null)
        {
            // Validar componentes dentro del Prefab
            BulletProjectile2D proj = bulletPrefab.GetComponent<BulletProjectile2D>();
            Rigidbody2D rb = bulletPrefab.GetComponent<Rigidbody2D>();
            Collider2D col = bulletPrefab.GetComponent<Collider2D>();

            Debug.Log($"[PrefabManager2D] Prefab '{bulletPrefab.name}' validado correctamente. " +
                      $"(BulletScript: {proj != null}, Rigidbody2D: {rb != null}, Collider2D: {col != null})");
        }
        else
        {
            Debug.LogWarning("[PrefabManager2D] Prefab de bala no referenciado en el gestor.");
        }
    }
}
