using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Script de Editor para configurar automáticamente la Actividad 3.7 (Efectos de acción)
/// </summary>
public static class SetupActionEffects3_7
{
    [MenuItem("UDG Tools/Configurar Actividad 3.7 - Efectos de Acción")]
    public static void SetupSceneAndPrefabs()
    {
        string scenePath = "Assets/Scenes/MainScene2D.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Configurar Prefab de Bala / Misil
        string prefabPath = "Assets/Prefabs/BulletPrefab.prefab";
        GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabObj != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(prefabObj);
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                GameObject root = editScope.prefabContentsRoot;

                // Asegurar Rigidbody2D con gravityScale = 0
                Rigidbody2D rb = root.GetComponent<Rigidbody2D>();
                if (rb == null) rb = root.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearDamping = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // Asegurar Collider2D como trigger
                Collider2D col = root.GetComponent<Collider2D>();
                if (col == null) col = root.AddComponent<CircleCollider2D>();
                col.isTrigger = true;

                // Agregar MissileActionEffects2D
                MissileActionEffects2D mae = root.GetComponent<MissileActionEffects2D>();
                if (mae == null) mae = root.AddComponent<MissileActionEffects2D>();

                // Agregar TrailRenderer
                TrailRenderer tr = root.GetComponent<TrailRenderer>();
                if (tr == null) tr = root.AddComponent<TrailRenderer>();
                tr.time = 0.25f;
                tr.startWidth = 0.22f;
                tr.endWidth = 0.02f;

                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(new Color(0.2f, 0.85f, 1f), 0.0f),
                        new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.7f),
                        new GradientColorKey(new Color(0.8f, 0.2f, 0.1f), 1.0f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1.0f, 0.0f),
                        new GradientAlphaKey(0.6f, 0.6f),
                        new GradientAlphaKey(0.0f, 1.0f)
                    }
                );
                tr.colorGradient = gradient;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[SetupActionEffects3_7] BulletPrefab configurado exitosamente con MissileActionEffects2D y TrailRenderer.");
        }

        // 2. Configurar Player en la escena
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            UniformFireRateController2D ufrc = playerObj.GetComponent<UniformFireRateController2D>();
            if (ufrc == null) ufrc = playerObj.AddComponent<UniformFireRateController2D>();

            // Asignar prefab
            var so = new SerializedObject(ufrc);
            var propMissile = so.FindProperty("missilePrefab");
            if (propMissile != null && prefabObj != null)
            {
                propMissile.objectReferenceValue = prefabObj;
            }
            so.ApplyModifiedProperties();
        }

        // 3. Configurar Auditor en [Managers]
        GameObject managersObj = GameObject.Find("[Managers]");
        if (managersObj == null)
        {
            managersObj = new GameObject("[Managers]");
        }

        ActionEffectsAuditor2D auditor = managersObj.GetComponent<ActionEffectsAuditor2D>();
        if (auditor == null) auditor = managersObj.AddComponent<ActionEffectsAuditor2D>();

        if (auditor != null && prefabObj != null)
        {
            var soAud = new SerializedObject(auditor);
            var propBullet = soAud.FindProperty("bulletPrefab");
            if (propBullet != null) propBullet.objectReferenceValue = prefabObj;
            soAud.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupActionEffects3_7] Escena MainScene2D configurada y guardada con efectos de acción.");
    }
}
