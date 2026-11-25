using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.4. El bucle de juego y la animación
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: EnvironmentGameLoopManager.cs
/// Responsabilidad: Orquestar el Game Loop principal del escenario interactivo,
/// coordinando la sincronización de los 3 bucles de animación requeridos
/// (Fogata/Fuego, Destellos/Partículas y Viento/Ambiente) y transiciones de estado.
/// </summary>
public class EnvironmentGameLoopManager : MonoBehaviour
{
    [Header("Tres Bucles de Animación Principales (Rúbrica: ≥8 Frames c/u)")]
    [Tooltip("Bucle de animación 1: Elemento de fuego / fogata de fondo.")]
    [SerializeField] private SpriteFrameAnimationLoop fireAnimationLoop;

    [Tooltip("Bucle de animación 2: Destellos o partículas mágicas flotantes.")]
    [SerializeField] private SpriteFrameAnimationLoop sparklesAnimationLoop;

    [Tooltip("Bucle de animación 3: Ondulación ambiental / vegetación en movimiento.")]
    [SerializeField] private SpriteFrameAnimationLoop ambientMotionLoop;

    [Header("Control del Ciclo de Ambiente (Game Loop)")]
    [Tooltip("Tiempo en segundos que dura una fase completa del ciclo ambiental.")]
    [SerializeField] private float cycleDuration = 10f;

    [Tooltip("Intensidad lumínica o de color ambiental global.")]
    [SerializeField] private Light mainSceneLight;

    [SerializeField] private Color dayColor = new Color(1f, 0.98f, 0.9f);
    [SerializeField] private Color duskColor = new Color(0.6f, 0.45f, 0.75f); // Tonalidad violeta/crepúsculo

    // Variables internas del Game Loop
    private float cycleTimer = 0f;
    private bool isGameLoopRunning = true;

    private void Start()
    {
        // Asegurar que los 3 bucles inicien activamente y de forma integrada
        InitializeAnimationLoops();
    }

    private void Update()
    {
        if (!isGameLoopRunning) return;

        ExecuteGameLoopCycle();
    }

    /// <summary>
    /// Configura y arranca los 3 bucles de animación asegurando que se reproduzcan armónicamente.
    /// </summary>
    private void InitializeAnimationLoops()
    {
        if (fireAnimationLoop != null)
        {
            fireAnimationLoop.SetFPS(10f); // Tasa visual para fuego constante
            fireAnimationLoop.Play();
        }

        if (sparklesAnimationLoop != null)
        {
            sparklesAnimationLoop.SetFPS(8f); // Tasa sutil para partículas
            sparklesAnimationLoop.Play();
        }

        if (ambientMotionLoop != null)
        {
            ambientMotionLoop.SetFPS(12f); // Movimiento fluido de vegetación/agua
            ambientMotionLoop.Play();
        }
    }

    /// <summary>
    /// Ejecuta el ciclo continuo del bucle de juego (Game Loop) modificando sutilmente
    /// el entorno para enriquecer la atmósfera interactiva.
    /// </summary>
    private void ExecuteGameLoopCycle()
    {
        cycleTimer += Time.deltaTime;
        float progress = Mathf.PingPong(cycleTimer / cycleDuration, 1f);

        if (mainSceneLight != null)
        {
            mainSceneLight.color = Color.Lerp(dayColor, duskColor, progress);
        }
    }

    /// <summary>
    /// Pausa o reanuda globalmente todos los bucles de animación y el Game Loop.
    /// </summary>
    public void ToggleGameLoopPause(bool pause)
    {
        isGameLoopRunning = !pause;

        if (pause)
        {
            if (fireAnimationLoop != null) fireAnimationLoop.Pause();
            if (sparklesAnimationLoop != null) sparklesAnimationLoop.Pause();
            if (ambientMotionLoop != null) ambientMotionLoop.Pause();
        }
        else
        {
            if (fireAnimationLoop != null) fireAnimationLoop.Play();
            if (sparklesAnimationLoop != null) sparklesAnimationLoop.Play();
            if (ambientMotionLoop != null) ambientMotionLoop.Play();
        }
    }
}
