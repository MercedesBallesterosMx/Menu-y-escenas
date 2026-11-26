using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class ApplyVisualDesignTheme
{
    static ApplyVisualDesignTheme()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!SessionState.GetBool("VisualDesignThemeApplied", false))
            {
                SessionState.SetBool("VisualDesignThemeApplied", true);
                ApplyTheme();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.2 - Aplicar Diseño Visual del Videojuego")]
    public static void ApplyTheme()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Diseño Visual 2D] Detén el modo Play antes de aplicar y guardar el diseño.");
            return;
        }

        Debug.Log("[Diseño Visual 2D] Aplicando texturas estilizadas, personaje tipo héroe y escenario...");

        // 1. Configurar importadores de texturas para Pixel Art nítido
        ConfigureTextureAsPixelSprite("Assets/Sprites/Player_Hero.png", 64);
        ConfigureTextureAsPixelSprite("Assets/Sprites/Ground_Grass_Tile.png", 64);
        ConfigureTextureAsPixelSprite("Assets/Sprites/Floating_Platform_Tile.png", 64);
        ConfigureTextureAsPixelSprite("Assets/Sprites/Bullet_Energy_Bolt.png", 64);
        ConfigureTextureAsPixelSprite("Assets/Sprites/Background_Platformer_2D.png", 100);

        AssetDatabase.Refresh();

        Sprite sHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player_Hero.png");
        Sprite sGround = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ground_Grass_Tile.png");
        Sprite sPlatform = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Floating_Platform_Tile.png");
        Sprite sBullet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bullet_Energy_Bolt.png");
        Sprite sBackground = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background_Platformer_2D.png");

        // 2. Actualizar Prefab de la Bala
        string prefabPath = "Assets/Prefabs/BulletPrefab.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (bulletPrefab != null && sBullet != null)
        {
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editScope.prefabContentsRoot;
                SpriteRenderer srB = root.GetComponent<SpriteRenderer>();
                if (srB != null)
                {
                    srB.sprite = sBullet;
                    srB.color = Color.white;
                    srB.sortingOrder = 15;
                }

                CircleCollider2D colB = root.GetComponent<CircleCollider2D>();
                if (colB != null)
                {
                    colB.radius = 0.22f;
                    colB.isTrigger = true;
                }

                root.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);
            }
            Debug.Log("[Diseño Visual 2D] BulletPrefab actualizado con sprite de plasma luminiscente.");
        }

        // 3. Actualizar Escena MainScene2D
        string scenePath = "Assets/Scenes/MainScene2D.unity";
        if (File.Exists(scenePath))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);

            // A) Fondo Parallax
            GameObject bgObj = GameObject.Find("Background_Parallax");
            if (bgObj == null)
            {
                bgObj = new GameObject("Background_Parallax");
            }
            bgObj.transform.position = new Vector3(0f, 0f, 5f);
            bgObj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            SpriteRenderer srBg = bgObj.GetComponent<SpriteRenderer>();
            if (srBg == null) srBg = bgObj.AddComponent<SpriteRenderer>();
            srBg.sprite = sBackground;
            srBg.color = Color.white;
            srBg.sortingOrder = -50;

            // B) Suelo Principal
            GameObject groundObj = GameObject.Find("Ground_Platform");
            if (groundObj != null && sGround != null)
            {
                groundObj.transform.position = new Vector3(0f, -3.8f, 0f);
                groundObj.transform.localScale = new Vector3(1f, 1f, 1f);

                SpriteRenderer srG = groundObj.GetComponent<SpriteRenderer>();
                if (srG != null)
                {
                    srG.sprite = sGround;
                    srG.color = Color.white;
                    srG.drawMode = SpriteDrawMode.Tiled;
                    srG.size = new Vector2(18f, 2.0f);
                    srG.sortingOrder = 0;
                }

                BoxCollider2D colG = groundObj.GetComponent<BoxCollider2D>();
                if (colG != null)
                {
                    colG.size = new Vector2(18f, 2.0f);
                    colG.offset = new Vector2(0f, 0f);
                }
            }

            // C) Plataformas Flotantes
            Action<string, Vector3, Vector2> stylePlatform = (objName, pos, size) =>
            {
                GameObject pObj = GameObject.Find(objName);
                if (pObj != null && sPlatform != null)
                {
                    pObj.transform.position = pos;
                    pObj.transform.localScale = new Vector3(1f, 1f, 1f);

                    SpriteRenderer srP = pObj.GetComponent<SpriteRenderer>();
                    if (srP != null)
                    {
                        srP.sprite = sPlatform;
                        srP.color = Color.white;
                        srP.drawMode = SpriteDrawMode.Tiled;
                        srP.size = size;
                        srP.sortingOrder = 5;
                    }

                    BoxCollider2D colP = pObj.GetComponent<BoxCollider2D>();
                    if (colP != null)
                    {
                        colP.size = size;
                        colP.offset = new Vector2(0f, 0f);
                    }
                }
            };

            stylePlatform("Platform_Floating_Left", new Vector3(-4.5f, -1.2f, 0f), new Vector2(4.5f, 0.9f));
            stylePlatform("Platform_Floating_Right", new Vector3(4.5f, 0.5f, 0f), new Vector2(4.5f, 0.9f));

            // D) Personaje Principal (Player Hero)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && sHero != null)
            {
                playerObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

                SpriteRenderer srPlayer = playerObj.GetComponent<SpriteRenderer>();
                if (srPlayer != null)
                {
                    srPlayer.sprite = sHero;
                    srPlayer.color = Color.white;
                    srPlayer.sortingOrder = 10;
                }

                BoxCollider2D playerCol = playerObj.GetComponent<BoxCollider2D>();
                if (playerCol != null)
                {
                    playerCol.size = new Vector2(0.7f, 1.35f);
                    playerCol.offset = new Vector2(0.0f, 0.0f);
                }

                // FirePoint en la punta del blaster
                Transform firePoint = playerObj.transform.Find("FirePoint");
                if (firePoint != null)
                {
                    firePoint.localPosition = new Vector3(0.65f, 0.05f, 0f);
                }
            }

            // E) Monedas y Base de Datos (Actividad 3.8)
            SetupDatabaseConnection3_8.SetupSceneAndDatabase();

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Diseño Visual 2D] Escena MainScene2D estilizada con éxito (Héroe, Fondo, Plataformas de Pasto/Madera y Proyectil de Plasma).");
        }
    }

    private static void ConfigureTextureAsPixelSprite(string assetPath, float ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point; // Pixel art sin borrosidad
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
