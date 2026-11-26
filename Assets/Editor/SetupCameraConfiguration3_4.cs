using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SetupCameraConfiguration3_4
{
    static SetupCameraConfiguration3_4()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("Actividad3_4_Configured", false))
            {
                SessionState.SetBool("Actividad3_4_Configured", true);
                ConfigureCamera();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.4 - Configurar Cámara 2D y Parallax")]
    public static void ConfigureCamera()
    {
        Debug.Log("[Actividad 3.4] Configurando seguimiento fluido de cámara 2D y escala proporcional...");

        string scenePath = "Assets/Scenes/MainScene2D.unity";
        if (File.Exists(scenePath))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);

            GameObject playerObj = GameObject.Find("Player");

            // 1. Configurar Main Camera
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                Camera cam = camObj.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.orthographic = true;
                    cam.orthographicSize = 5.0f; // Proporcional (ni muy cerca ni muy lejos)
                    cam.transform.position = new Vector3(0f, 1.25f, -10f);
                }

                CameraController2D camCtrl = camObj.GetComponent<CameraController2D>();
                if (camCtrl == null) camCtrl = camObj.AddComponent<CameraController2D>();

                // Asignar Player como target
                if (playerObj != null)
                {
                    SerializedObject so = new SerializedObject(camCtrl);
                    so.FindProperty("target").objectReferenceValue = playerObj.transform;
                    so.FindProperty("offset").vector3Value = new Vector3(0f, 1.25f, -10f);
                    so.FindProperty("smoothTime").floatValue = 0.15f;
                    so.FindProperty("targetOrthographicSize").floatValue = 5.0f;
                    so.FindProperty("enableBounds").boolValue = true;
                    so.FindProperty("minBounds").vector2Value = new Vector2(-12.0f, -3.5f);
                    so.FindProperty("maxBounds").vector2Value = new Vector2(12.0f, 5.5f);
                    so.ApplyModifiedProperties();
                }

                Debug.Log("[Actividad 3.4] Main Camera configurada con CameraController2D (SmoothDamp y Bounds).");
            }

            // 2. Configurar Fondo Parallax
            GameObject bgObj = GameObject.Find("Background_Parallax");
            if (bgObj != null)
            {
                CameraParallaxBackground2D parallax = bgObj.GetComponent<CameraParallaxBackground2D>();
                if (parallax == null) parallax = bgObj.AddComponent<CameraParallaxBackground2D>();
                Debug.Log("[Actividad 3.4] Background_Parallax configurado con CameraParallaxBackground2D.");
            }

            // 3. Configurar Auditor en [Managers]
            GameObject managers = GameObject.Find("[Managers]");
            if (managers == null) managers = new GameObject("[Managers]");

            CameraConfigurationAuditor2D auditor = managers.GetComponent<CameraConfigurationAuditor2D>();
            if (auditor == null) auditor = managers.AddComponent<CameraConfigurationAuditor2D>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Actividad 3.4] Escena MainScene2D actualizada y guardada con éxito.");
        }
    }
}
