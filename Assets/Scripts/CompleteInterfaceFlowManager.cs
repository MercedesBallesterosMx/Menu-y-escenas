using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.6. El flujo de la interfaz
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: CompleteInterfaceFlowManager.cs
/// Responsabilidad: Orquestar el flujo global entre todas las escenas del proyecto
/// (Menú Principal, Opciones, Nivel 1 y Escenario interactivo), incluyendo transiciones
/// con fundido suave (Fade In/Out) y persistencia de configuración.
/// </summary>
public class CompleteInterfaceFlowManager : MonoBehaviour
{
    public static CompleteInterfaceFlowManager Instance { get; private set; }

    [Header("Identificadores de Escenas del Proyecto")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string level1Scene = "Level1";
    [SerializeField] private string sampleScene = "SampleScene";

    [Header("Transición Visual (Fade Canvas)")]
    [Tooltip("Panel UI Image negro que cubre la pantalla para transiciones.")]
    [SerializeField] private Image fadeOverlay;

    [Tooltip("Velocidad de transición de desvanecimiento.")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Configuración de Interfaz")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider brightnessSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cargar preferencias guardadas
        LoadSavedSettings();
    }

    private void Start()
    {
        if (fadeOverlay != null)
        {
            StartCoroutine(FadeRoutine(1f, 0f)); // Fundido de entrada al iniciar
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetGlobalVolume);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetGlobalBrightness);
        }
    }

    /// <summary>
    /// Carga la escena del Nivel 1 con transición suave.
    /// </summary>
    public void GoToPlayLevel()
    {
        TransitionToScene(level1Scene);
    }

    /// <summary>
    /// Retorna a la escena del Menú Principal.
    /// </summary>
    public void GoToMainMenu()
    {
        TransitionToScene(mainMenuScene);
    }

    /// <summary>
    /// Carga una escena específica por su nombre.
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithFadeRoutine(sceneName));
    }

    private IEnumerator LoadSceneWithFadeRoutine(string sceneName)
    {
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeRoutine(0f, 1f));
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeRoutine(1f, 0f));
        }
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha)
    {
        float timer = 0f;
        Color c = fadeOverlay.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        fadeOverlay.color = c;
    }

    public void SetGlobalVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("GlobalVolume", volume);
    }

    public void SetGlobalBrightness(float brightness)
    {
        PlayerPrefs.SetFloat("GlobalBrightness", brightness);
    }

    private void LoadSavedSettings()
    {
        if (PlayerPrefs.HasKey("GlobalVolume"))
        {
            float vol = PlayerPrefs.GetFloat("GlobalVolume");
            AudioListener.volume = vol;
            if (volumeSlider != null) volumeSlider.value = vol;
        }

        if (PlayerPrefs.HasKey("GlobalBrightness") && brightnessSlider != null)
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("GlobalBrightness");
        }
    }

    public void QuitApplication()
    {
        Debug.Log("[CompleteInterfaceFlowManager] Saliendo de la aplicación interactiva.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
