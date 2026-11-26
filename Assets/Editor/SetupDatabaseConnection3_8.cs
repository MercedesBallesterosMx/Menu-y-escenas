using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Script de Editor para configurar automáticamente la Actividad 3.8 (Conexión a Base de Datos)
/// </summary>
public static class SetupDatabaseConnection3_8
{
    [MenuItem("UDG Tools/Configurar Actividad 3.8 - Conexión a Base de Datos")]
    public static void SetupSceneAndDatabase()
    {
        string scenePath = "Assets/Scenes/MainScene2D.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Cargar Sprite de Moneda
        string coinSpritePath = "Assets/Sprites/Coin_Gold.png";
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(coinSpritePath);

        // 2. Crear Prefab de Moneda
        string prefabPath = "Assets/Prefabs/CoinPrefab.prefab";
        GameObject coinObj = new GameObject("CoinPrefab");
        SpriteRenderer sr = coinObj.AddComponent<SpriteRenderer>();
        if (coinSprite != null) sr.sprite = coinSprite;
        sr.sortingOrder = 5;

        CircleCollider2D col = coinObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        coinObj.AddComponent<CoinCollectible2D>();
        coinObj.transform.localScale = Vector3.one * 0.75f;

        PrefabUtility.SaveAsPrefabAsset(coinObj, prefabPath);
        GameObject.DestroyImmediate(coinObj);
        AssetDatabase.SaveAssets();

        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // 3. Colocar Monedas en la Escena sobre las plataformas
        GameObject coinsGroup = GameObject.Find("Coins_Group");
        if (coinsGroup != null) GameObject.DestroyImmediate(coinsGroup);
        coinsGroup = new GameObject("Coins_Group");

        Vector3[] coinPositions = new Vector3[]
        {
            new Vector3(-2.5f, -1.2f, 0f),
            new Vector3(-0.5f, -1.2f, 0f),
            new Vector3(1.5f, -1.2f, 0f),
            new Vector3(-2.0f, 0.8f, 0f),  // Plataforma flotante izquierda
            new Vector3(3.0f, 1.3f, 0f)    // Plataforma flotante derecha
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
        Debug.Log("[SetupDatabaseConnection3_8] Escena y Prefabs configurados con éxito para la Actividad 3.8.");
    }
}
