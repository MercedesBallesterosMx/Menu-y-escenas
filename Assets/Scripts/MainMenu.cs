using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.9. Las pruebas de resultados: el paso final
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: MainMenu.cs
/// Responsabilidad: Gestionar el flujo de navegación del menú principal y el cierre del videojuego (Criterio 2):
/// 1. Botón "Jugar" / "Iniciar": Carga la escena principal del juego (MainScene2D o Level1).
/// 2. Botón "Salir" / "Cerrar": Ejecuta el cierre autónomo del programa mediante Application.Quit().
/// 3. Submenús de opciones y controles interactivos con retroalimentación inmediata.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    [Tooltip("Panel contenedor del menú principal.")]
    public GameObject mainMenu;

    [Tooltip("Panel de opciones de configuración / controles.")]
    public GameObject optionsMenu;

    [Header("Configuración de Carga de Escena")]
    [Tooltip("Nombre de la escena principal a cargar al presionar Jugar.")]
    [SerializeField] private string targetPlayScene = "MainScene2D";

    private void Awake()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (optionsMenu != null) optionsMenu.SetActive(false);
    }

    public void OpenOptionsPanel()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (optionsMenu != null) optionsMenu.SetActive(true);
        Debug.Log("[MainMenu] Panel de opciones abierto.");
    }

    public void OpenMainMenuPanel()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (optionsMenu != null) optionsMenu.SetActive(false);
        Debug.Log("[MainMenu] Retorno al menú principal.");
    }

    /// <summary>
    /// Inicia la partida cargando la escena de juego 2D (Criterio 2)
    /// </summary>
    public void PlayGame()
    {
        Debug.Log($"[MainMenu] Iniciando partida... Cargando escena '{targetPlayScene}'.");
        if (Application.CanStreamedLevelBeLoaded(targetPlayScene))
        {
            SceneManager.LoadScene(targetPlayScene);
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    /// <summary>
    /// Cierra el programa de forma autónoma fuera del editor (Criterio 2 oficial de Moodle)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MainMenu] Solicitud de cierre de videojuego (Application.Quit()). Cerrando programa...");

        // Guardar progreso antes de salir
        if (GameProgressDatabaseManager2D.Instance != null)
        {
            GameProgressDatabaseManager2D.Instance.CommitProgressToDatabase();
        }

        // Cierre en Build ejecutable autónomo
        Application.Quit();

#if UNITY_EDITOR
        // Detener modo Play si se ejecuta dentro del Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
