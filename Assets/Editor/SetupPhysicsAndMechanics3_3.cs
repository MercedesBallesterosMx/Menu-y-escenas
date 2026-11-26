using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupPhysicsAndMechanics3_3
{
    [MenuItem("Tools/Actividad 3.3 - Configurar Físicas y Mecánicas 2D")]
    public static void ConfigurePhysicsAndMechanics()
    {
        Debug.Log("[Actividad 3.3] Configurando mecánicas de salto, gravedad y colisiones 2D...");

        string scenePath = "Assets/Scenes/MainScene2D.unity";
        if (File.Exists(scenePath))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);

            // 1. Configurar Jugador (Player)
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb == null) rb = player.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 2.8f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                BoxCollider2D boxCol = player.GetComponent<BoxCollider2D>();
                if (boxCol != null)
                {
                    boxCol.isTrigger = false;
                }

                PlatformerPhysicsMechanics2D phys = player.GetComponent<PlatformerPhysicsMechanics2D>();
                if (phys == null) phys = player.AddComponent<PlatformerPhysicsMechanics2D>();

                Debug.Log("[Actividad 3.3] Jugador configurado con gravedad 2.8, salto reactivo y colisionador sólido.");
            }

            // 2. Configurar Plataformas Flotantes con PlatformEffector2D
            string[] floatPlats = new string[] { "Platform_Floating_Left", "Platform_Floating_Right" };
            foreach (string platName in floatPlats)
            {
                GameObject platObj = GameObject.Find(platName);
                if (platObj != null)
                {
                    BoxCollider2D platCol = platObj.GetComponent<BoxCollider2D>();
                    if (platCol == null) platCol = platObj.AddComponent<BoxCollider2D>();
                    platCol.usedByEffector = true;
                    platCol.isTrigger = false;

                    PlatformEffector2D effector = platObj.GetComponent<PlatformEffector2D>();
                    if (effector == null) effector = platObj.AddComponent<PlatformEffector2D>();
                    effector.useOneWay = true;
                    effector.surfaceArc = 170f;

                    PlatformEffectorController2D effectorCtrl = platObj.GetComponent<PlatformEffectorController2D>();
                    if (effectorCtrl == null) effectorCtrl = platObj.AddComponent<PlatformEffectorController2D>();
                }
            }

            // 3. Configurar Manager de Físicas
            GameObject managers = GameObject.Find("[Managers]");
            if (managers == null) managers = new GameObject("[Managers]");

            PhysicsMechanicsManager2D physMgr = managers.GetComponent<PhysicsMechanicsManager2D>();
            if (physMgr == null) physMgr = managers.AddComponent<PhysicsMechanicsManager2D>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Actividad 3.3] Escena MainScene2D actualizada y guardada con éxito.");
        }
    }
}
