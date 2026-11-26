using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SetupBulletMovement3_6
{
    static SetupBulletMovement3_6()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("Actividad3_6_Configured", false))
            {
                SessionState.SetBool("Actividad3_6_Configured", true);
                ConfigureBulletMovement();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.6 - Configurar Movimiento y Trayectoria de Bala")]
    public static void ConfigureBulletMovement()
    {
        Debug.Log("[Actividad 3.6] Configurando cinemática balística de proyectil y puente de animación...");

        // 1. Configurar Prefab de la Bala
        string prefabPath = "Assets/Prefabs/BulletPrefab.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (bulletPrefab != null)
        {
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editScope.prefabContentsRoot;

                Rigidbody2D rb = root.GetComponent<Rigidbody2D>();
                if (rb == null) rb = root.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                CircleCollider2D col = root.GetComponent<CircleCollider2D>();
                if (col == null) col = root.AddComponent<CircleCollider2D>();
                col.isTrigger = true;

                BulletMovementController2D bmc = root.GetComponent<BulletMovementController2D>();
                if (bmc == null) bmc = root.AddComponent<BulletMovementController2D>();
            }
            Debug.Log("[Actividad 3.6] BulletPrefab actualizado con BulletMovementController2D (16 m/s, Gravedad 0).");
        }

        // 2. Configurar Escena MainScene2D
        string scenePath = "Assets/Scenes/MainScene2D.unity";
        if (File.Exists(scenePath))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);

            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                PlayerCombatAnimationBridge2D bridge = playerObj.GetComponent<PlayerCombatAnimationBridge2D>();
                if (bridge == null) bridge = playerObj.AddComponent<PlayerCombatAnimationBridge2D>();
                Debug.Log("[Actividad 3.6] Player configurado con PlayerCombatAnimationBridge2D.");
            }

            GameObject managers = GameObject.Find("[Managers]");
            if (managers == null) managers = new GameObject("[Managers]");

            BulletMechanicsAuditor2D auditor = managers.GetComponent<BulletMechanicsAuditor2D>();
            if (auditor == null) auditor = managers.AddComponent<BulletMechanicsAuditor2D>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Actividad 3.6] Escena MainScene2D guardada exitosamente.");
        }
    }
}
