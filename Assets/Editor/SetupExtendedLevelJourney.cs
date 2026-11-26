using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Script de Editor para expandir el recorrido del nivel 2D a una aventura de múltiples pantallas
/// Actividad 3.9 - Pruebas de Resultados y Jugabilidad Ampliada
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// </summary>
public static class SetupExtendedLevelJourney
{
    [MenuItem("Tools/Actividad 3.9 - Expandir Recorrido del Nivel (Múltiples Pantallas)")]
    [MenuItem("UDG Tools/Expandir Recorrido del Nivel (Aventura Completa)")]
    public static void ExpandLevel()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[SetupExtendedLevelJourney] Detén el modo Play antes de modificar la escena.");
            return;
        }

        string scenePath = "Assets/Scenes/MainScene2D.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Cargar Assets necesarios
        Sprite sGround = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ground_Grass_Tile.png");
        Sprite sPlatform = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Floating_Platform_Tile.png");
        Sprite sBackground = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background_Platformer_2D.png");
        Sprite sHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player_Hero.png");
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CoinPrefab.prefab");

        // Si CoinPrefab aún no está listo, ejecutar SetupDatabaseConnection3_8
        if (coinPrefab == null)
        {
            SetupDatabaseConnection3_8.SetupSceneAndDatabase();
            coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CoinPrefab.prefab");
        }

        // 2. Contenedor del Escenario
        GameObject envObj = GameObject.Find("Environment_Level");
        if (envObj != null) GameObject.DestroyImmediate(envObj);
        envObj = new GameObject("Environment_Level");

        // Helper para crear plataformas/suelos estilizados
        Func<string, Vector3, Vector2, Sprite, int, GameObject> createTileObj = (name, pos, size, sprite, sorting) =>
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = pos;
            obj.transform.SetParent(envObj.transform);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = sorting;

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = size;
            col.offset = Vector2.zero;

            return obj;
        };

        // Eliminar suelo y plataformas anteriores para evitar duplicados
        string[] oldObjs = { "Ground_Platform", "Platform_Floating_Left", "Platform_Floating_Right" };
        foreach (var oldName in oldObjs)
        {
            GameObject oldG = GameObject.Find(oldName);
            if (oldG != null) GameObject.DestroyImmediate(oldG);
        }

        // A) Secciones de Suelo (con abismos entre ellas para saltar)
        // Zona 1: x = -10 a x = 10
        createTileObj("Ground_Zone_1", new Vector3(0f, -3.8f, 0f), new Vector2(20f, 2.0f), sGround, 0);
        // Zona 2: x = 14 a x = 28
        createTileObj("Ground_Zone_2", new Vector3(21f, -3.8f, 0f), new Vector2(14f, 2.0f), sGround, 0);
        // Zona 3: x = 33 a x = 49
        createTileObj("Ground_Zone_3", new Vector3(41f, -3.8f, 0f), new Vector2(16f, 2.0f), sGround, 0);
        // Zona 4: x = 52 a x = 66
        createTileObj("Ground_Zone_4", new Vector3(59f, -3.8f, 0f), new Vector2(14f, 2.0f), sGround, 0);

        // B) Plataformas Flotantes y Puentes sobre abismos
        createTileObj("Platform_Floating_1", new Vector3(-4.5f, -1.2f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        createTileObj("Platform_Floating_2", new Vector3(4.5f, 0.5f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        // Puente flotante en Abismo 1
        createTileObj("Platform_Bridge_Abyss_1", new Vector3(12.0f, -1.5f, 0f), new Vector2(3.5f, 0.9f), sPlatform, 5);
        // Plataformas en Zona 2
        createTileObj("Platform_Floating_3", new Vector3(17.5f, 0.3f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        createTileObj("Platform_Floating_4", new Vector3(23.5f, 1.5f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        // Puente flotante en Abismo 2
        createTileObj("Platform_Bridge_Abyss_2", new Vector3(30.5f, -1.0f, 0f), new Vector2(3.5f, 0.9f), sPlatform, 5);
        // Escalera de ascenso en Zona 3
        createTileObj("Platform_Floating_5", new Vector3(36.0f, 0.8f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        createTileObj("Platform_Floating_6", new Vector3(42.0f, 2.2f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        createTileObj("Platform_Floating_7", new Vector3(47.5f, 3.4f, 0f), new Vector2(4.5f, 0.9f), sPlatform, 5);
        // Cima Final (Meta del recorrido)
        createTileObj("Platform_Summit_Final", new Vector3(56.0f, 4.5f, 0f), new Vector2(6.0f, 0.9f), sPlatform, 5);

        // C) Paredes Invisibles de Límite
        Action<string, Vector3> createWall = (name, pos) =>
        {
            GameObject wall = new GameObject(name);
            wall.transform.position = pos;
            wall.transform.SetParent(envObj.transform);
            BoxCollider2D wCol = wall.AddComponent<BoxCollider2D>();
            wCol.size = new Vector2(1f, 18f);
        };
        createWall("Boundary_Wall_Left", new Vector3(-10.5f, 2f, 0f));
        createWall("Boundary_Wall_Right", new Vector3(66.5f, 2f, 0f));

        // D) Fondo Parallax Continuo a lo largo de todo el recorrido
        GameObject oldBg = GameObject.Find("Background_Parallax");
        if (oldBg != null) GameObject.DestroyImmediate(oldBg);

        GameObject bgGroup = GameObject.Find("Background_Parallax_Group");
        if (bgGroup != null) GameObject.DestroyImmediate(bgGroup);
        bgGroup = new GameObject("Background_Parallax_Group");
        bgGroup.transform.position = Vector3.zero;

        float[] bgXPositions = new float[] { -2f, 20f, 42f, 64f };
        for (int b = 0; b < bgXPositions.Length; b++)
        {
            GameObject bgSection = new GameObject($"Background_Section_{b}");
            bgSection.transform.position = new Vector3(bgXPositions[b], 1.5f, 5f);
            bgSection.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            bgSection.transform.SetParent(bgGroup.transform);

            SpriteRenderer srBg = bgSection.AddComponent<SpriteRenderer>();
            srBg.sprite = sBackground;
            srBg.color = Color.white;
            srBg.sortingOrder = -50;
        }

        // E) Detector de Caída al Abismo (Respawn seguro)
        GameObject abyssObj = GameObject.Find("Abyss_Kill_Trigger");
        if (abyssObj != null) GameObject.DestroyImmediate(abyssObj);
        abyssObj = new GameObject("Abyss_Kill_Trigger");
        abyssObj.transform.position = new Vector3(28f, -6.5f, 0f);
        BoxCollider2D abyssCol = abyssObj.AddComponent<BoxCollider2D>();
        abyssCol.isTrigger = true;
        abyssCol.size = new Vector2(90f, 2.5f);
        AbyssFallTrigger2D fallScript = abyssObj.AddComponent<AbyssFallTrigger2D>();
        fallScript.SetRespawnPoint(new Vector3(0f, -2.0f, 0f));

        // F) 20 Monedas Doradas Distribuidas por todo el camino
        GameObject coinsGroup = GameObject.Find("Coins_Group");
        if (coinsGroup != null) GameObject.DestroyImmediate(coinsGroup);
        coinsGroup = new GameObject("Coins_Group");

        Vector3[] coinPositions = new Vector3[]
        {
            // Zona 1
            new Vector3(-2.5f, -2.0f, 0f),
            new Vector3(-0.5f, -2.0f, 0f),
            new Vector3(1.5f, -2.0f, 0f),
            new Vector3(-4.5f, -0.3f, 0f),
            new Vector3(4.5f, 1.4f, 0f),
            // Abismo 1
            new Vector3(11.0f, -0.6f, 0f),
            new Vector3(13.0f, -0.6f, 0f),
            // Zona 2
            new Vector3(15.5f, -2.0f, 0f),
            new Vector3(17.5f, 1.2f, 0f),
            new Vector3(23.5f, 2.4f, 0f),
            new Vector3(26.0f, -2.0f, 0f),
            // Abismo 2
            new Vector3(29.5f, -0.1f, 0f),
            new Vector3(31.5f, -0.1f, 0f),
            // Zona 3 (Ascenso)
            new Vector3(36.0f, 1.7f, 0f),
            new Vector3(42.0f, 3.1f, 0f),
            new Vector3(47.5f, 4.3f, 0f),
            // Zona 4 (Cima y Meta)
            new Vector3(53.5f, 5.5f, 0f),
            new Vector3(55.0f, 5.8f, 0f),
            new Vector3(56.5f, 5.8f, 0f),
            new Vector3(58.0f, 5.5f, 0f)
        };

        if (coinPrefab != null)
        {
            for (int c = 0; c < coinPositions.Length; c++)
            {
                GameObject coinInstance = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
                coinInstance.name = $"Coin_{c + 1}";
                coinInstance.transform.position = coinPositions[c];
                coinInstance.transform.SetParent(coinsGroup.transform);
            }
        }

        // G) Configurar la Cámara para seguir al héroe en el recorrido ampliado
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraController2D camCtrl = cam.GetComponent<CameraController2D>();
            if (camCtrl != null)
            {
                // Usar SerializedObject para asignar los límites ampliados
                SerializedObject so = new SerializedObject(camCtrl);
                SerializedProperty minProp = so.FindProperty("minBounds");
                SerializedProperty maxProp = so.FindProperty("maxBounds");
                if (minProp != null) minProp.vector2Value = new Vector2(-8.0f, -3.5f);
                if (maxProp != null) maxProp.vector2Value = new Vector2(60.0f, 6.5f);
                so.ApplyModifiedProperties();
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupExtendedLevelJourney] ¡Nivel 2D ampliado con éxito a más de 75 metros de recorrido, 4 zonas de plataformas, 20 monedas y seguimiento de cámara fluido!");
    }
}
