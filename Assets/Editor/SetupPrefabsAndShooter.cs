using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SetupPrefabsAndShooter
{
    static SetupPrefabsAndShooter()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("SetupPrefabsExecuted", false))
            {
                SessionState.SetBool("SetupPrefabsExecuted", true);
                ExecuteSetup();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.2 - Configurar Prefabs y Disparo")]
    public static void ExecuteSetup()
    {
        Debug.Log("[Actividad 3.2] Configurando Prefabs y Sistema de Disparo 2D...");

        // 1. Asegurar directorios
        if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

        // 2. Sprite para la bala
        Sprite bulletSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (bulletSprite == null)
        {
            Texture2D tex = new Texture2D(16, 16);
            Color[] colors = new Color[16 * 16];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.yellow;
            tex.SetPixels(colors);
            tex.Apply();
            bulletSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        }

        // 3. Crear Prefab de la Bala
        string prefabPath = "Assets/Prefabs/BulletPrefab.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (bulletPrefab == null)
        {
            GameObject tempBullet = new GameObject("BulletPrefab");

            SpriteRenderer sr = tempBullet.AddComponent<SpriteRenderer>();
            sr.sprite = bulletSprite;
            sr.color = new Color(1.0f, 0.85f, 0.2f); // Amarillo dorado #FFD54F
            tempBullet.transform.localScale = new Vector3(0.45f, 0.25f, 1f);

            CircleCollider2D col = tempBullet.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            Rigidbody2D rb = tempBullet.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BulletProjectile2D proj = tempBullet.AddComponent<BulletProjectile2D>();

            bulletPrefab = PrefabUtility.SaveAsPrefabAsset(tempBullet, prefabPath);
            UnityEngine.Object.DestroyImmediate(tempBullet);
            Debug.Log("[Actividad 3.2] Prefab creado exitosamente en: " + prefabPath);
        }

        // 4. Configurar la escena MainScene2D
        string scenePath = "Assets/Scenes/MainScene2D.unity";
        if (File.Exists(scenePath))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                // Asegurar FirePoint alineado (no al centro)
                Transform firePoint = playerObj.transform.Find("FirePoint");
                if (firePoint == null)
                {
                    GameObject fpObj = new GameObject("FirePoint");
                    fpObj.transform.SetParent(playerObj.transform);
                    fpObj.transform.localPosition = new Vector3(0.85f, 0.1f, 0f); // Alineado a la derecha/frente
                    firePoint = fpObj.transform;
                }

                // Asegurar PlayerWeaponShooter2D
                PlayerWeaponShooter2D shooter = playerObj.GetComponent<PlayerWeaponShooter2D>();
                if (shooter == null)
                {
                    shooter = playerObj.AddComponent<PlayerWeaponShooter2D>();
                }

                // Asignar referencias por SerializedObject
                SerializedObject so = new SerializedObject(shooter);
                so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
                so.FindProperty("firePoint").objectReferenceValue = firePoint;
                so.ApplyModifiedProperties();

                Debug.Log("[Actividad 3.2] PlayerWeaponShooter2D vinculado al Player con FirePoint alineado y BulletPrefab.");
            }

            // Gestor de prefabs
            GameObject managersObj = GameObject.Find("[Managers]");
            if (managersObj != null)
            {
                PrefabManager2D pm = managersObj.GetComponent<PrefabManager2D>();
                if (pm == null)
                {
                    pm = managersObj.AddComponent<PrefabManager2D>();
                }
                SerializedObject soPm = new SerializedObject(pm);
                soPm.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
                soPm.ApplyModifiedProperties();
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Actividad 3.2] Escena MainScene2D actualizada y guardada con sistema de Prefabs.");
        }
    }
}
