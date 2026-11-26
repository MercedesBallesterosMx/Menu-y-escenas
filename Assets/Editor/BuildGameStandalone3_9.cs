using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>
/// Script de Editor para automatizar la compilación del ejecutable autónomo (.exe)
/// Actividad 3.9. Las pruebas de resultados: el paso final
/// </summary>
[InitializeOnLoad]
public static class BuildGameStandalone3_9
{
    static BuildGameStandalone3_9()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string exePath = "Builds/Windows/Videojuego2D_MercedesBallesteros.exe";
            if (!File.Exists(exePath) && !SessionState.GetBool("AutoBuild3_9_Executed", false))
            {
                SessionState.SetBool("AutoBuild3_9_Executed", true);
                BuildStandaloneWindowsPlayer();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.9 - Compilar Ejecutable Autónomo Windows (.exe)")]
    [MenuItem("UDG Tools/Compilar Ejecutable Autónomo Windows (.exe)")]
    public static void BuildStandaloneWindowsPlayer()
    {
        ConfigureBuildSettings();

        string buildDir = "Builds/Windows";
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        string exePath = Path.Combine(buildDir, "Videojuego2D_MercedesBallesteros.exe");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/MainScene2D.unity",
            "Assets/Scenes/Level1.unity"
        };
        buildPlayerOptions.locationPathName = exePath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log($"[BuildGameStandalone3_9] Iniciando compilación de ejecutable autónomo en: {exePath}...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildGameStandalone3_9] ¡COMPILACIÓN EXITOSA! Tamaño: {summary.totalSize / (1024 * 1024)} MB en {summary.totalTime.TotalSeconds:F1} s.");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[BuildGameStandalone3_9] Fallo en la compilación con {summary.totalErrors} errores.");
        }
    }

    [MenuItem("Tools/Actividad 3.9 - Configurar Build Settings y Escenas")]
    [MenuItem("UDG Tools/Configurar Build Settings y Escenas (3.9)")]
    public static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainScene2D.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true)
        };

        EditorBuildSettings.scenes = scenes;
        Debug.Log("[BuildGameStandalone3_9] Build Settings configuradas con 3 escenas: MainMenu (0), MainScene2D (1), Level1 (2).");
    }
}
