using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SetupPlayerMovement3_5
{
    static SetupPlayerMovement3_5()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("Actividad3_5_Configured", false))
            {
                SessionState.SetBool("Actividad3_5_Configured", true);
                ConfigurePlayerMovement();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.5 - Configurar Movimiento del Jugador")]
    public static void ConfigurePlayerMovement()
    {
        Debug.Log("[Actividad 3.5] Configurando desplazamiento cinemático por flechas y respeto a físicas...");

        string scenePath = "Assets/Scenes/MainScene2D.unity";
        if (File.Exists(scenePath))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);

            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                AdvancedPlayerMovement2D mov = playerObj.GetComponent<AdvancedPlayerMovement2D>();
                if (mov == null) mov = playerObj.AddComponent<AdvancedPlayerMovement2D>();

                Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.freezeRotation = true;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                }

                BoxCollider2D col = playerObj.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    col.isTrigger = false;
                }

                Debug.Log("[Actividad 3.5] Componente AdvancedPlayerMovement2D configurado en el jugador.");
            }

            GameObject managers = GameObject.Find("[Managers]");
            if (managers == null) managers = new GameObject("[Managers]");

            PlayerMovementAuditor2D auditor = managers.GetComponent<PlayerMovementAuditor2D>();
            if (auditor == null) auditor = managers.AddComponent<PlayerMovementAuditor2D>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Actividad 3.5] Escena MainScene2D actualizada y guardada con éxito.");
        }
    }
}
