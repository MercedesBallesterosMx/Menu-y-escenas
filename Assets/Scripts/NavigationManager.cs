using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 2
/// Actividad 2.3. Programación de botones
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: NavigationManager.cs
/// Responsabilidad: Administrar el flujo de navegación entre escenas y páginas
/// del libro interactivo/aplicación de escritorio en Unity.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    [Header("Configuración de Navegación por Escenas")]
    [Tooltip("Nombre de la escena del menú principal.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Tooltip("Nombre de la siguiente escena/página interactiva.")]
    [SerializeField] private string nextPageSceneName = "Pagina2";

    [Tooltip("Nombre de la escena/página anterior.")]
    [SerializeField] private string previousPageSceneName = "Pagina1";

    [Header("Configuración de Paneles Locales (Páginas en Canvas)")]
    [Tooltip("Lista de paneles que representan las páginas o vistas dentro de la misma escena.")]
    [SerializeField] private List<GameObject> bookPages = new List<GameObject>();

    [Tooltip("Índice de la página actualmente visible.")]
    [SerializeField] private int currentPageIndex = 0;

    [Header("Botones de Control de Interfaz")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button returnMenuButton;
    [SerializeField] private Button exitAppButton;

    private void Start()
    {
        // Inicializar los listeners de botones si fueron asignados en el Inspector
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextBookPage);
        }

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousBookPage);
        }

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.AddListener(LoadMainMenu);
        }

        if (exitAppButton != null)
        {
            exitAppButton.onClick.AddListener(QuitInteractiveBook);
        }

        // Si se trabaja con páginas basadas en paneles Canvas, actualizar visibilidad inicial
        if (bookPages.Count > 0)
        {
            UpdatePageVisibility();
        }
    }

    /// <summary>
    /// Avanza a la siguiente página dentro del Canvas o carga la siguiente escena.
    /// </summary>
    public void NextBookPage()
    {
        if (bookPages.Count > 0)
        {
            if (currentPageIndex < bookPages.Count - 1)
            {
                currentPageIndex++;
                UpdatePageVisibility();
            }
            else if (!string.IsNullOrEmpty(nextPageSceneName))
            {
                LoadSceneByName(nextPageSceneName);
            }
        }
        else if (!string.IsNullOrEmpty(nextPageSceneName))
        {
            LoadSceneByName(nextPageSceneName);
        }
    }

    /// <summary>
    /// Retrocede a la página anterior dentro del Canvas o carga la escena previa.
    /// </summary>
    public void PreviousBookPage()
    {
        if (bookPages.Count > 0)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdatePageVisibility();
            }
            else if (!string.IsNullOrEmpty(previousPageSceneName))
            {
                LoadSceneByName(previousPageSceneName);
            }
        }
        else if (!string.IsNullOrEmpty(previousPageSceneName))
        {
            LoadSceneByName(previousPageSceneName);
        }
    }

    /// <summary>
    /// Actualiza el estado activo de los GameObjects de cada página.
    /// </summary>
    private void UpdatePageVisibility()
    {
        for (int i = 0; i < bookPages.Count; i++)
        {
            if (bookPages[i] != null)
            {
                bookPages[i].SetActive(i == currentPageIndex);
            }
        }

        // Desactivar botones en los extremos para prevenir desbordes de navegación
        if (previousButton != null)
        {
            previousButton.interactable = (currentPageIndex > 0 || !string.IsNullOrEmpty(previousPageSceneName));
        }

        if (nextButton != null)
        {
            nextButton.interactable = (currentPageIndex < bookPages.Count - 1 || !string.IsNullOrEmpty(nextPageSceneName));
        }
    }

    /// <summary>
    /// Carga la escena del Menú Principal.
    /// </summary>
    public void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[NavigationManager] El nombre de la escena del menú principal no ha sido especificado.");
        }
    }

    /// <summary>
    /// Carga cualquier escena por su identificador de nombre.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Reinicia la escena activa actual.
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    /// <summary>
    /// Cierra la aplicación interactiva de escritorio.
    /// </summary>
    public void QuitInteractiveBook()
    {
        Debug.Log("[NavigationManager] Saliendo de la aplicación interactiva...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
