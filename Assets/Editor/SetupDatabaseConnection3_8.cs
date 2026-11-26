using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Script de Editor para configurar automáticamente la Actividad 3.8 (Conexión a Base de Datos)
/// Estudiante: Mercedes de Jesús Ballesteros Chávez (Código: 226300438)
/// </summary>
public static class SetupDatabaseConnection3_8
{
    [MenuItem("Tools/Actividad 3.8 - Configurar Monedas y Conexión a Base de Datos")]
    [MenuItem("UDG Tools/Configurar Actividad 3.8 - Conexión a Base de Datos")]
    public static void SetupSceneAndDatabase()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[SetupDatabaseConnection3_8] Por favor detén el modo Play antes de configurar la escena.");
            return;
        }

        string scenePath = "Assets/Scenes/MainScene2D.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Configurar Sprite de Moneda (Pixel Art nítido)
        string coinSpritePath = "Assets/Sprites/Coin_Gold.png";
        TextureImporter importer = AssetImporter.GetAtPath(coinSpritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(coinSpritePath);

        // 2. Crear Prefab de Moneda
        string prefabPath = "Assets/Prefabs/CoinPrefab.prefab";
        GameObject coinObj = new GameObject("CoinPrefab");
        SpriteRenderer sr = coinObj.AddComponent<SpriteRenderer>();
        if (coinSprite != null) sr.sprite = coinSprite;
        sr.sortingOrder = 8;
        sr.color = Color.white;

        CircleCollider2D col = coinObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        coinObj.AddComponent<CoinCollectible2D>();
        coinObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

        PrefabUtility.SaveAsPrefabAsset(coinObj, prefabPath);
        GameObject.DestroyImmediate(coinObj);
        AssetDatabase.SaveAssets();

        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // 3. Colocar Monedas en la Escena sobre el suelo y plataformas flotantes
        GameObject coinsGroup = GameObject.Find("Coins_Group");
        if (coinsGroup != null) GameObject.DestroyImmediate(coinsGroup);
        coinsGroup = new GameObject("Coins_Group");

        Vector3[] coinPositions = new Vector3[]
        {
            new Vector3(-2.5f, -2.0f, 0f), // Suelo (accesible caminando)
            new Vector3(-0.5f, -2.0f, 0f), // Suelo (accesible caminando)
            new Vector3(1.5f, -2.0f, 0f),  // Suelo (accesible caminando)
            new Vector3(-4.5f, -0.3f, 0f), // Plataforma flotante izquierda
            new Vector3(4.5f, 1.4f, 0f)    // Plataforma flotante derecha
        };

        for (int i = 0; i < coinPositions.Length; i++)
        {
            GameObject coinInstance = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
            coinInstance.name = $"Coin_{i + 1}";
            coinInstance.transform.position = coinPositions[i];
            coinInstance.transform.SetParent(coinsGroup.transform);
        }

        // 4. Configurar Managers
        GameObject managersObj = GameObject.Find("[Managers]");
        if (managersObj == null) managersObj = new GameObject("[Managers]");

        if (managersObj.GetComponent<GameDatabaseCloudService2D>() == null)
            managersObj.AddComponent<GameDatabaseCloudService2D>();

        if (managersObj.GetComponent<GameProgressDatabaseManager2D>() == null)
            managersObj.AddComponent<GameProgressDatabaseManager2D>();

        if (managersObj.GetComponent<DatabaseConnectionAuditor2D>() == null)
            managersObj.AddComponent<DatabaseConnectionAuditor2D>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupDatabaseConnection3_8] ¡Monedas y base de datos Firebase configuradas con éxito en MainScene2D!");
    }
}
