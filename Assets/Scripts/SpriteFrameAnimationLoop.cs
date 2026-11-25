using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.4. El bucle de juego y la animación
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: SpriteFrameAnimationLoop.cs
/// Responsabilidad: Reproducir bucles de animación cíclicos basados en una secuencia
/// de al menos 8 sprites (frames) con control de velocidad (FPS) y soporte para
/// SpriteRenderer en escena o Image en Canvas UI.
/// </summary>
public class SpriteFrameAnimationLoop : MonoBehaviour
{
    [Header("Secuencia de Sprites (Frames de Animación)")]
    [Tooltip("Lista de al menos 8 sprites que componen el bucle de animación.")]
    [SerializeField] private Sprite[] animationFrames;

    [Header("Configuración del Bucle")]
    [Tooltip("Velocidad de reproducción en cuadros por segundo (FPS).")]
    [SerializeField] private float framesPerSecond = 12f;

    [Tooltip("Si es verdadero, la animación se repite indefinidamente (Loop).")]
    [SerializeField] private bool loop = true;

    [Tooltip("Si es verdadero, inicia con un cuadro aleatorio para desincronizar múltiples bucles.")]
    [SerializeField] private bool randomizeStartFrame = true;

    [Tooltip("Indica si el bucle inicia reproduciéndose automáticamente al cargar.")]
    [SerializeField] private bool playOnAwake = true;

    // Componentes de renderizado compatibles
    private SpriteRenderer spriteRenderer;
    private Image uiImage;

    // Variables internas de control
    private int currentFrameIndex = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();

        if (spriteRenderer == null && uiImage == null)
        {
            Debug.LogError($"[SpriteFrameAnimationLoop] El GameObject '{gameObject.name}' requiere un componente SpriteRenderer o UI Image.");
        }
    }

    private void Start()
    {
        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning($"[SpriteFrameAnimationLoop] No se han asignado frames en '{gameObject.name}'.");
            return;
        }

        if (randomizeStartFrame && animationFrames.Length > 1)
        {
            currentFrameIndex = Random.Range(0, animationFrames.Length);
        }

        if (playOnAwake)
        {
            Play();
        }

        DisplayCurrentFrame();
    }

    private void Update()
    {
        if (!isPlaying || animationFrames == null || animationFrames.Length <= 1) return;

        timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(framesPerSecond, 0.01f);

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            AdvanceFrame();
        }
    }

    /// <summary>
    /// Avanza al siguiente frame en la secuencia cíclica.
    /// </summary>
    private void AdvanceFrame()
    {
        currentFrameIndex++;

        if (currentFrameIndex >= animationFrames.Length)
        {
            if (loop)
            {
                currentFrameIndex = 0;
            }
            else
            {
                currentFrameIndex = animationFrames.Length - 1;
                isPlaying = false;
            }
        }

        DisplayCurrentFrame();
    }

    /// <summary>
    /// Asigna el sprite del cuadro actual al renderizador correspondiente.
    /// </summary>
    private void DisplayCurrentFrame()
    {
        if (animationFrames == null || currentFrameIndex < 0 || currentFrameIndex >= animationFrames.Length) return;

        Sprite activeSprite = animationFrames[currentFrameIndex];

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = activeSprite;
        }
        else if (uiImage != null)
        {
            uiImage.sprite = activeSprite;
        }
    }

    /// <summary>
    /// Inicia o reanuda la reproducción del bucle animado.
    /// </summary>
    public void Play()
    {
        isPlaying = true;
    }

    /// <summary>
    /// Pausa la animación en el frame actual.
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Detiene y reinicia la animación al primer cuadro.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        currentFrameIndex = 0;
        timer = 0f;
        DisplayCurrentFrame();
    }

    /// <summary>
    /// Permite ajustar dinámicamente la velocidad de fotogramas por segundo (FPS).
    /// </summary>
    public void SetFPS(float newFPS)
    {
        framesPerSecond = Mathf.Max(newFPS, 0.1f);
    }
}
