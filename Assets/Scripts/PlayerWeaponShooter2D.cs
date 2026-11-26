using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.2. Los prefabs
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlayerWeaponShooter2D.cs
/// Responsabilidad: Coordinar la mecánica de disparo de proyectiles desde el personaje:
/// captura eventos de teclado, valida cadencia de disparo (cooldown), instancia el Prefab
/// de la bala desde el punto de disparo alineado (FirePoint) y orienta la trayectoria
/// según el frente activo del jugador (Izquierda/Derecha).
/// </summary>
public class PlayerWeaponShooter2D : MonoBehaviour
{
    [Header("Configuración de Prefab de Bala")]
    [Tooltip("Prefab del proyectil que será instanciado.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Punto de origen de la bala, alineado al frente/arma del personaje (no al centro).")]
    [SerializeField] private Transform firePoint;

    [Header("Parámetros de Disparo")]
    [Tooltip("Tiempo mínimo entre disparos consecutivos en segundos.")]
    [SerializeField] private float fireRate = 0.25f;

    [Tooltip("Desplazamiento horizontal relativo por defecto si firePoint no está asignado.")]
    [SerializeField] private Vector2 defaultFirePointOffset = new Vector2(0.75f, 0.1f);

    [Header("Eventos de Interacción")]
    public UnityEvent onShootEvent;

    // Componentes y control interno
    private PlayerController2D playerController;
    private float nextFireTime = 0f;
    private int totalBulletsFired = 0;

    public int TotalBulletsFired => totalBulletsFired;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();

        // Crear punto de disparo automático si no fue asignado en el Inspector
        if (firePoint == null)
        {
            GameObject fpObj = new GameObject("FirePoint");
            fpObj.transform.SetParent(transform);
            fpObj.transform.localPosition = new Vector3(defaultFirePointOffset.x, defaultFirePointOffset.y, 0f);
            firePoint = fpObj.transform;
        }
    }

    private void Update()
    {
        // 1. Detectar evento de teclado para disparar (Tecla F, J, Clic izquierdo o Ctrl Izquierdo)
        if (IsFireKeyPressed() && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    /// <summary>
    /// Comprueba la pulsación de teclas de disparo tanto en New Input System como en Legacy.
    /// </summary>
    private bool IsFireKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame ||
                Keyboard.current.jKey.wasPressedThisFrame ||
                Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            {
                return true;
            }
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J) || Input.GetButtonDown("Fire1"))
        {
            return true;
        }
#endif
        return false;
    }

    /// <summary>
    /// Instancia el Prefab de la bala y configura su trayectoria según la dirección del personaje.
    /// </summary>
    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[PlayerWeaponShooter2D] No se ha asignado el Prefab de la bala en el Inspector.");
            return;
        }

        // Actualizar cooldown interno para evitar ráfagas descontroladas o llamadas dobles en el mismo frame
        nextFireTime = Time.time + fireRate;

        // 1. Determinar el sentido hacia donde mira el personaje (Derecha = +1, Izquierda = -1)
        float directionX = 1f;

        AdvancedPlayerMovement2D apm = GetComponent<AdvancedPlayerMovement2D>();
        if (apm != null)
        {
            directionX = apm.FacingRight ? 1f : -1f;
        }
        else if (playerController != null)
        {
            directionX = playerController.IsFacingRight ? 1f : -1f;
        }
        else if (transform.localScale.x < 0f)
        {
            directionX = -1f;
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.flipX)
            {
                directionX = -1f;
            }
        }

        Vector2 shootDirection = new Vector2(directionX, 0f);

        // 2. Calcular posición de spawn segura (garantizar que quede frente al jugador y no dentro de su collider)
        float spawnOffsetX = 0.95f;
        if (firePoint != null)
        {
            spawnOffsetX = Mathf.Max(0.85f, Mathf.Abs(firePoint.localPosition.x * Mathf.Abs(transform.localScale.x)));
        }

        float spawnOffsetY = defaultFirePointOffset.y;
        if (firePoint != null)
        {
            spawnOffsetY = firePoint.localPosition.y * Mathf.Abs(transform.localScale.y);
        }

        Vector3 spawnPosition = transform.position + new Vector3(directionX * spawnOffsetX, spawnOffsetY, 0f);

        // 3. Instanciar el Prefab de la bala
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        // Prevenir colisiones entre la bala recién nacida y el propio jugador
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D bulletCollider = bulletObj.GetComponent<Collider2D>();
        if (playerCollider != null && bulletCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, bulletCollider, true);
        }

        // 4. Configurar el vector de dirección en TODOS los scripts del proyectil
        BulletProjectile2D projectile = bulletObj.GetComponent<BulletProjectile2D>();
        if (projectile != null)
        {
            projectile.SetDirection(shootDirection);
        }

        BulletMovementController2D bmc = bulletObj.GetComponent<BulletMovementController2D>();
        if (bmc != null)
        {
            bmc.SetDirection(shootDirection);
        }

        MissileActionEffects2D mae = bulletObj.GetComponent<MissileActionEffects2D>();
        if (mae != null)
        {
            mae.InitializeTrajectory(shootDirection, 16.0f);
        }

        totalBulletsFired++;
        Debug.Log($"[PlayerWeaponShooter2D] Disparo #{totalBulletsFired} ejecutado desde punto alineado {spawnPosition} hacia {shootDirection}.");

        // Efecto visual instantáneo de destello de cañón (Muzzle Flash)
        StartCoroutine(ShowMuzzleFlash(spawnPosition, directionX));

        onShootEvent?.Invoke();
    }

    private IEnumerator ShowMuzzleFlash(Vector3 position, float directionX)
    {
        GameObject flash = new GameObject("MuzzleFlash_FX");
        flash.transform.position = position;
        flash.transform.localScale = new Vector3(directionX * 1.0f, 1.0f, 1.0f);

        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 35;
        sr.color = new Color(0.2f, 0.95f, 1f, 1f);

        if (bulletPrefab != null)
        {
            SpriteRenderer bulletSr = bulletPrefab.GetComponent<SpriteRenderer>();
            if (bulletSr != null && bulletSr.sprite != null)
            {
                sr.sprite = bulletSr.sprite;
            }
        }

        yield return new WaitForSeconds(0.06f);
        if (flash != null)
        {
            Destroy(flash);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el punto de disparo en la ventana Scene de Unity
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.12f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, transform.right * 0.5f);
        }
    }
}
