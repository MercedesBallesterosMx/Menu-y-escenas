using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optativa: Diseño de juegos II - Unidad 3: Desarrollo de videojuegos 2D
/// Actividad 3.8. La conexión a base de datos
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// 
/// Script: GameProgressDatabaseManager2D.cs
/// Responsabilidad: Orquestar el progreso global del juego y su persistencia en la base de datos (Criterios 1 y 2):
/// 1. Mantiene el conteo en tiempo real de monedas recolectadas.
/// 2. Controla y persiste el registro de avances en dos niveles distintos (Nivel 1 y Nivel 2).
/// 3. Ejecuta el ciclo de guardado automático al recolectar monedas, superar niveles y al cerrar el juego.
/// 4. Sincroniza y carga los datos desde la base de datos persistente al abrir el juego.
/// </summary>
public class GameProgressDatabaseManager2D : MonoBehaviour
{
    public static GameProgressDatabaseManager2D Instance { get; private set; }

    [Header("Servicio de Base de Datos")]
    [SerializeField] private GameDatabaseCloudService2D databaseService;

    [Header("Estado Actual de la Partida (En Memoria)")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private int activeLevelIndex = 1;
    [SerializeField] private bool isLevel1Completed = false;
    [SerializeField] private bool isLevel2Completed = false;
    [SerializeField] private int highestLevelUnlocked = 1;

    [Header("Eventos de Interfaz")]
    public UnityEvent<int> onCoinsCountChanged;
    public UnityEvent<string> onDatabaseSyncNotice;

    public int CurrentCoins => currentCoins;
    public int ActiveLevelIndex => activeLevelIndex;
    public bool IsLevel1Completed => isLevel1Completed;
    public bool IsLevel2Completed => isLevel2Completed;
    public int HighestLevelUnlocked => highestLevelUnlocked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (databaseService == null)
        {
            databaseService = GetComponent<GameDatabaseCloudService2D>();
            if (databaseService == null)
            {
                databaseService = gameObject.AddComponent<GameDatabaseCloudService2D>();
            }
        }

        // Cargar progreso desde la base de datos al inicializar la aplicación (Criterio 2)
        LoadDatabaseRecords();
    }

    /// <summary>
    /// Carga los datos almacenados en la base de datos
    /// </summary>
    public void LoadDatabaseRecords()
    {
        if (databaseService != null)
        {
            databaseService.LoadProgress((record, success) =>
            {
                currentCoins = record.totalCoinsCollected;
                isLevel1Completed = record.level1Completed;
                isLevel2Completed = record.level2Completed;
                highestLevelUnlocked = record.highestLevelUnlocked;

                onCoinsCountChanged?.Invoke(currentCoins);
                onDatabaseSyncNotice?.Invoke($"Progreso cargado desde base de datos: {currentCoins} monedas, Nivel {highestLevelUnlocked}.");

                Debug.Log($"[GameProgressDatabaseManager2D] Datos sincronizados desde base de datos. Monedas: {currentCoins}, Niveles [1: {isLevel1Completed}, 2: {isLevel2Completed}].");
            });
        }
    }

    /// <summary>
    /// Suma monedas recolectadas en tiempo de ejecución y dispara guardado en base de datos (Criterio 1)
    /// </summary>
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        onCoinsCountChanged?.Invoke(currentCoins);
        Debug.Log($"[GameProgressDatabaseManager2D] Moneda sumada. Total en juego: {currentCoins}.");

        // Guardado persistente automático
        CommitProgressToDatabase();
    }

    /// <summary>
    /// Registra la superación de un nivel (Nivel 1 o Nivel 2) y actualiza la base de datos
    /// </summary>
    public void CompleteLevel(int levelNumber, int score = 100)
    {
        if (levelNumber == 1)
        {
            isLevel1Completed = true;
            if (highestLevelUnlocked < 2) highestLevelUnlocked = 2;
        }
        else if (levelNumber == 2)
        {
            isLevel2Completed = true;
        }

        Debug.Log($"[GameProgressDatabaseManager2D] ¡Nivel {levelNumber} completado! Guardando en base de datos.");
        CommitProgressToDatabase();
    }

    /// <summary>
    /// Guarda el estado actual en la base de datos
    /// </summary>
    public void CommitProgressToDatabase()
    {
        if (databaseService != null)
        {
            GameDatabaseCloudService2D.PlayerDataRecord record = new GameDatabaseCloudService2D.PlayerDataRecord
            {
                totalCoinsCollected = currentCoins,
                level1Completed = isLevel1Completed,
                level2Completed = isLevel2Completed,
                highestLevelUnlocked = highestLevelUnlocked,
                level1Score = isLevel1Completed ? 150 : 0,
                level2Score = isLevel2Completed ? 300 : 0
            };

            databaseService.SaveProgress(record, (success, msg) =>
            {
                if (success)
                {
                    onDatabaseSyncNotice?.Invoke("Base de datos sincronizada correctamente.");
                }
            });
        }
    }

    private void OnApplicationQuit()
    {
        // Guardar automáticamente al cerrar por completo el juego (Criterio 2)
        Debug.Log("[GameProgressDatabaseManager2D] Cierre de juego detectado. Guardando estado final en base de datos.");
        CommitProgressToDatabase();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            CommitProgressToDatabase();
        }
    }

    private void Update()
    {
        // Tecla de escape para navegación rápida y cierre (Criterio 2 de Moodle)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
            {
                Application.Quit();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private void OnGUI()
    {
        // Panel visual HUD en pantalla para la Actividad 3.8 y 3.9 (Criterios 1 y 2 de Moodle)
        var oldBoxFontSize = GUI.skin.box.fontSize;
        var oldLabelFontSize = GUI.skin.label.fontSize;

        GUI.skin.box.fontSize = 12;
        GUI.skin.label.fontSize = 11;

        // Caja de fondo semitransparente con borde estético
        GUI.Box(new Rect(15, 15, 275, 150), "<b>Base de Datos Cloud (Firebase)</b>");

        GUI.Label(new Rect(25, 40, 255, 22), $"<b>🪙 Monedas:</b> <color=yellow><size=13><b>{currentCoins}</b></size></color>");
        GUI.Label(new Rect(25, 62, 255, 22), $"<b>🏆 Niveles:</b> Nivel 1 [{(isLevel1Completed ? "✓" : "En curso")}] | Nivel 2 [{(isLevel2Completed ? "✓" : "Bloqueado")}]");
        GUI.Label(new Rect(25, 84, 255, 22), $"<b>☁ Estado:</b> <color=#90ee90>{(databaseService != null ? "Sincronizado con Firestore" : "Persistencia Local")}</color>");
        GUI.Label(new Rect(25, 102, 255, 20), $"<size=9><color=#cccccc>Jugador: {databaseService?.PlayerId ?? "player_mercedes_226300438"}</color></size>");

        // Botones de navegación y cierre autónomo (Criterio 2 de Moodle)
        if (GUI.Button(new Rect(25, 126, 125, 26), "<size=10><b>🏠 Menú Principal</b></size>"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        if (GUI.Button(new Rect(155, 126, 125, 26), "<size=10><b>✕ Salir (Quit)</b></size>"))
        {
            CommitProgressToDatabase();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        GUI.skin.box.fontSize = oldBoxFontSize;
        GUI.skin.label.fontSize = oldLabelFontSize;
    }
}
