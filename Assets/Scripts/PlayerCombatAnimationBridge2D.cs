using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.6. El movimiento de una bala
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: PlayerCombatAnimationBridge2D.cs
/// Responsabilidad: Coordinar las mecánicas de disparo con la máquina de estados Mecanim
/// erradicando retrasos o desfases (Criterios 1, 2 y 3 de Moodle):
/// 1. Conecta eventos de movimiento horizontal (Idle <-> Walk/Run hacia izquierda y derecha).
/// 2. Dispara la animación y mecánica de disparo (Shoot) de forma instantánea al presionar el hardware.
/// 3. Gestiona los 3 estados de animación requeridos: 'Idle' (reposo), 'Walk' (carrera) y 'Shoot' (disparo).
/// </summary>
public class PlayerCombatAnimationBridge2D : MonoBehaviour
{
    [Header("Referencias de Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerWeaponShooter2D weaponShooter;

    [Header("Parámetros de Mecanim")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string shootTriggerParam = "Shoot";
    [SerializeField] private string isGroundedParam = "isGrounded";

    [Header("Cadencia y Rendimiento")]
    [Tooltip("Tiempo mínimo entre disparos para erradicar retrasos manteniendo cadencia limpia.")]
    [SerializeField] private float fireCooldown = 0.20f;

    private float nextFireTime = 0f;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (weaponShooter == null) weaponShooter = GetComponent<PlayerWeaponShooter2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 1. Sincronizar eventos de movimiento horizontal con el Animator (Criterio 1)
        if (animator != null && animator.runtimeAnimatorController != null && rb != null)
        {
            float currentHorizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
            animator.SetFloat(speedParam, currentHorizontalSpeed);
        }

        // 2. Captura instantánea de evento de disparo sin desfases (Criterio 2)
        if (IsShootInputPressed() && Time.time >= nextFireTime)
        {
            ExecuteCombatInteraction();
        }
    }

    /// <summary>
    /// Ejecuta de inmediato el disparo físico y activa la animación en Mecanim.
    /// </summary>
    private void ExecuteCombatInteraction()
    {
        nextFireTime = Time.time + fireCooldown;

        // Disparar animación en Mecanim (Criterio 3: Estado funcional 'Shoot')
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger(shootTriggerParam);
        }

        // Disparar proyectil físico mediante el sistema balístico
        if (weaponShooter != null)
        {
            weaponShooter.Shoot();
        }

        Debug.Log("[PlayerCombatAnimationBridge2D] Disparo e interacciones sincronizadas sin desfases.");
    }

    /// <summary>
    /// Detecta la pulsación de disparo en teclado (F, J, Clic izquierdo o barra).
    /// </summary>
    private bool IsShootInputPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.jKey.wasPressedThisFrame)
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
}
