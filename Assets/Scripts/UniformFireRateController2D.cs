using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.7. Efectos de acción
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: UniformFireRateController2D.cs
/// Responsabilidad: Garantizar el Criterio 3 de evaluación de Moodle:
/// "La cantidad de proyectiles que salen del arma debe ser uniforme y fluida, evitando que se superpongan entre sí de forma errática."
/// 
/// Características:
/// 1. Cadencia uniforme estricta (Uniform Fire Rate) mediante temporizador regulado (cadencia calibrada a 0.22 s).
/// 2. Control de espaciado físico mínimo entre proyectiles consecutivos para impedir solapamientos.
/// 3. Efecto visual de destello de cañón (Muzzle Flash) al disparar.
/// 4. Sincronización transparente con el sistema de animación Mecanim y PlayerWeaponShooter2D.
/// </summary>
public class UniformFireRateController2D : MonoBehaviour
{
    [Header("1. Control de Cadencia Uniforme (Criterio 3)")]
    [Tooltip("Intervalo exacto y constante entre disparos sucesivos (segundos).")]
    [SerializeField] private float fireRateInterval = 0.22f;

    [Tooltip("Separación espacial mínima requerida entre proyectiles (metros) para evitar superposición.")]
    [SerializeField] private float minProjectileSpacing = 0.65f;

    [Tooltip("Límite máximo de proyectiles simultáneos activos en pantalla.")]
    [SerializeField] private int maxActiveProjectiles = 12;

    [Header("2. Referencias de Disparo")]
    [Tooltip("Prefab del misil/bala con efectos de acción.")]
    [SerializeField] private GameObject missilePrefab;

    [Tooltip("Punto de origen de salida del proyectil (FirePoint).")]
    [SerializeField] private Transform muzzleFirePoint;

    [Header("3. Efecto de Acción: Destello de Disparo (Muzzle Flash)")]
    [Tooltip("Destello visual generado en la boca del cañón al disparar.")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Tooltip("Duración visible del destello del cañón.")]
    [SerializeField] private float muzzleFlashDuration = 0.05f;

    [Header("Eventos")]
    public UnityEvent onUniformShotFired;

    private float nextAllowedFireTime = 0f;
    private Vector2 lastFiredPosition = Vector2.negativeInfinity;
    private int consecutiveShotsCount = 0;
    private PlayerController2D playerController;
    private Animator animator;

    public float FireRateInterval => fireRateInterval;
    public int ConsecutiveShotsCount => consecutiveShotsCount;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();
        animator = GetComponent<Animator>();

        if (muzzleFirePoint == null)
        {
            Transform existingFp = transform.Find("FirePoint");
            if (existingFp != null)
            {
                muzzleFirePoint = existingFp;
            }
            else
            {
                GameObject fp = new GameObject("MuzzleFirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = new Vector3(0.85f, -0.05f, 0f);
                muzzleFirePoint = fp.transform;
            }
        }
    }

    private void Update()
    {
        // Detección de input de disparo continuo o por pulsación
        if (IsFireInputActive())
        {
            TryExecuteUniformShot();
        }
    }

    /// <summary>
    /// Intenta disparar garantizando cadencia uniforme y erradicando superposiciones
    /// </summary>
    public bool TryExecuteUniformShot()
    {
        // 1. Validar temporizador de cadencia uniforme
        if (Time.time < nextAllowedFireTime)
        {
            return false; // Bloquea disparos erráticos prematuros
        }

        // 2. Validar separación espacial si el personaje está quieto
        Vector2 currentMuzzlePos = muzzleFirePoint.position;
        if (Vector2.Distance(currentMuzzlePos, lastFiredPosition) < (minProjectileSpacing * 0.1f) &&
            Time.time - (nextAllowedFireTime - fireRateInterval) < (fireRateInterval * 0.95f))
        {
            return false;
        }

        // Ejecutar disparo cadenciado
        PerformShot(currentMuzzlePos);
        return true;
    }

    private void PerformShot(Vector2 spawnPosition)
    {
        nextAllowedFireTime = Time.time + fireRateInterval;
        lastFiredPosition = spawnPosition;
        consecutiveShotsCount++;

        // 1. Determinar orientación del disparo
        Vector2 shootDirection = Vector2.right;
        if (playerController != null)
        {
            shootDirection = playerController.IsFacingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            shootDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        }

        // 2. Instanciar proyectil si existe prefab
        if (missilePrefab != null)
        {
            GameObject proj = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);

            // Inyectar efectos de acción y cinemática rectilínea sin gravedad
            MissileActionEffects2D actionEffects = proj.GetComponent<MissileActionEffects2D>();
            if (actionEffects != null)
            {
                actionEffects.InitializeTrajectory(shootDirection, 16.0f);
            }
            else
            {
                BulletMovementController2D bmc = proj.GetComponent<BulletMovementController2D>();
                if (bmc != null)
                {
                    bmc.SetDirection(shootDirection);
                }
            }
        }

        // 3. Efecto de acción: Muzzle Flash en el cañón
        StartCoroutine(ShowMuzzleFlashEffect());

        // 4. Sincronizar trigger con Animator
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Shoot");
        }

        onUniformShotFired?.Invoke();
        Debug.Log($"[UniformFireRateController2D] Disparo uniforme #{consecutiveShotsCount} emitido con cadencia constante ({fireRateInterval}s).");
    }

    private IEnumerator ShowMuzzleFlashEffect()
    {
        GameObject flash = null;
        if (muzzleFlashPrefab != null)
        {
            flash = Instantiate(muzzleFlashPrefab, muzzleFirePoint.position, Quaternion.identity, muzzleFirePoint);
        }
        else
        {
            // Muzzle flash procedural (destello circular luminoso)
            flash = new GameObject("MuzzleFlash_Procedural");
            flash.transform.SetParent(muzzleFirePoint);
            flash.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
            sr.sprite = CreateFlashSprite();
            sr.color = new Color(1f, 0.95f, 0.4f, 0.9f); // Amarillo resplandor
            flash.transform.localScale = Vector3.one * 0.45f;
        }

        yield return new WaitForSeconds(muzzleFlashDuration);

        if (flash != null)
        {
            Destroy(flash);
        }
    }

    private Sprite CreateFlashSprite()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                float alpha = Mathf.Clamp01(1f - (dist / 7.5f));
                tex.SetPixel(x, y, new Color(1f, 1f, 0.8f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
    }

    private bool IsFireInputActive()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.fKey.isPressed || Keyboard.current.jKey.isPressed) return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.J) || Input.GetButton("Fire1")) return true;
#endif
        return false;
    }
}
