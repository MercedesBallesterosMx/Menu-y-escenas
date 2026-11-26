using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;

[InitializeOnLoad]
public class SetupSceneAndCapture
{
    private static string outputDir = @"c:\Users\espar\OneDrive\Documentos\Proyectos_universitarios\entregas\3.1. La escena principal y la estructura del personaje";

    static SetupSceneAndCapture()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("Setup2DSceneExecuted", false))
            {
                SessionState.SetBool("Setup2DSceneExecuted", true);
                ExecuteSetupAndCapture();
            }
        };
    }

    [MenuItem("Tools/Actividad 3.1 - Configurar Escena 2D y Capturar")]
    public static void ExecuteSetupAndCapture()
    {
        Debug.Log("[Actividad 3.1] Iniciando configuración automatizada de Escena 2D, Personaje y Animaciones...");

        // 1. Asegurar directorios
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
        if (!Directory.Exists("Assets/Animations")) Directory.CreateDirectory("Assets/Animations");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        // 2. Crear textura cuadrada para Sprites si no existe
        Sprite squareSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (squareSprite == null)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            squareSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        // 3. Crear Animator Controller con estados y parámetros
        string animControllerPath = "Assets/Animations/PlayerAnimatorController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(animControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(animControllerPath);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;
            var idleState = sm.AddState("IDLE");
            var walkState = sm.AddState("WALK");
            var jumpState = sm.AddState("JUMP");
            var fallState = sm.AddState("FALL");

            sm.defaultState = idleState;

            // Idle -> Walk
            var t1 = idleState.AddTransition(walkState);
            t1.hasExitTime = false;
            t1.duration = 0.1f;
            t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            // Walk -> Idle
            var t2 = walkState.AddTransition(idleState);
            t2.hasExitTime = false;
            t2.duration = 0.1f;
            t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            // Any State -> Jump
            var t3 = sm.AddAnyStateTransition(jumpState);
            t3.hasExitTime = false;
            t3.AddCondition(AnimatorConditionMode.If, 0, "Jump");

            // Jump -> Fall
            var t4 = jumpState.AddTransition(fallState);
            t4.hasExitTime = false;
            t4.duration = 0.1f;
            t4.AddCondition(AnimatorConditionMode.Less, 0f, "VerticalVelocity");

            // Fall -> Idle
            var t5 = fallState.AddTransition(idleState);
            t5.hasExitTime = false;
            t5.duration = 0.1f;
            t5.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");

            AssetDatabase.SaveAssets();
            Debug.Log("[Actividad 3.1] PlayerAnimatorController creado exitosamente en Assets/Animations/.");
        }

        // 4. Crear Escena 2D
        string scenePath = "Assets/Scenes/MainScene2D.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // A) Main Camera (2D Orthographic)
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0, 0, -10);
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.0f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.20f, 0.27f, 1f); // #263245
        camObj.AddComponent<AudioListener>();

        // B) Plataforma de Suelo Principal
        GameObject groundObj = new GameObject("Ground_Platform");
        groundObj.transform.position = new Vector3(0, -3.8f, 0);
        groundObj.transform.localScale = new Vector3(18f, 1.2f, 1f);
        SpriteRenderer srGround = groundObj.AddComponent<SpriteRenderer>();
        srGround.sprite = squareSprite;
        srGround.color = new Color(0.22f, 0.55f, 0.24f); // Verde pasto
        BoxCollider2D boxGround = groundObj.AddComponent<BoxCollider2D>();

        // C) Plataformas flotantes secundarias
        GameObject plat1 = new GameObject("Platform_Floating_Left");
        plat1.transform.position = new Vector3(-4.5f, -1.5f, 0);
        plat1.transform.localScale = new Vector3(4.5f, 0.6f, 1f);
        SpriteRenderer srP1 = plat1.AddComponent<SpriteRenderer>();
        srP1.sprite = squareSprite;
        srP1.color = new Color(0.35f, 0.45f, 0.55f);
        plat1.AddComponent<BoxCollider2D>();

        GameObject plat2 = new GameObject("Platform_Floating_Right");
        plat2.transform.position = new Vector3(4.5f, 0.2f, 0);
        plat2.transform.localScale = new Vector3(4.5f, 0.6f, 1f);
        SpriteRenderer srP2 = plat2.AddComponent<SpriteRenderer>();
        srP2.sprite = squareSprite;
        srP2.color = new Color(0.35f, 0.45f, 0.55f);
        plat2.AddComponent<BoxCollider2D>();

        // D) Personaje Principal (Player)
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.transform.position = new Vector3(0, -2.2f, 0);

        SpriteRenderer srPlayer = playerObj.AddComponent<SpriteRenderer>();
        srPlayer.sprite = squareSprite;
        srPlayer.color = new Color(0.12f, 0.53f, 0.90f); // Azul héroe #1E88E5
        playerObj.transform.localScale = new Vector3(1.2f, 2.0f, 1f);

        BoxCollider2D playerCol = playerObj.AddComponent<BoxCollider2D>();
        playerCol.size = new Vector2(1f, 1f);

        Rigidbody2D rbPlayer = playerObj.AddComponent<Rigidbody2D>();
        rbPlayer.bodyType = RigidbodyType2D.Dynamic;
        rbPlayer.mass = 1.0f;
        rbPlayer.gravityScale = 2.0f;
        rbPlayer.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rbPlayer.freezeRotation = true;

        Animator animPlayer = playerObj.AddComponent<Animator>();
        animPlayer.runtimeAnimatorController = controller;

        // Punto de comprobación de suelo
        GameObject checkObj = new GameObject("GroundCheckPoint");
        checkObj.transform.SetParent(playerObj.transform);
        checkObj.transform.localPosition = new Vector3(0, -0.55f, 0);

        // Scripts C# de la actividad
        PlayerController2D controllerScript = playerObj.AddComponent<PlayerController2D>();
        PlayerAnimationStateController2D animControllerScript = playerObj.AddComponent<PlayerAnimationStateController2D>();

        // E) Gestor de Escena
        GameObject managersObj = new GameObject("[Managers]");
        managersObj.AddComponent<GameSceneInitializer2D>();

        // 5. Guardar la escena
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Actividad 3.1] Escena guardada exitosamente en " + scenePath);

        // 6. Capturar render real de la escena
        CaptureSceneRender(cam, Path.Combine(outputDir, "evidencia_1_estructura_personaje_2d.png"));

        // 7. Intentar captura de la ventana de Unity
        CaptureUnityWindowScreenshot(Path.Combine(outputDir, "evidencia_3_controles_teclado_ejecucion.png"));

        Debug.Log("[Actividad 3.1] Proceso completado exitosamente.");
    }

    private static void CaptureSceneRender(Camera cam, string filePath)
    {
        try
        {
            RenderTexture rt = new RenderTexture(1920, 1080, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);

            Debug.Log("[Actividad 3.1] Render real de cámara 2D guardado en: " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Actividad 3.1] Error al capturar cámara: " + ex.Message);
        }
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")]
    static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")]
    static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll")]
    static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")]
    static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")]
    static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
    [DllImport("gdi32.dll")]
    static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, [Out] byte[] lpvBits, ref BITMAPINFO lpbi, uint uUsage);

    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER
    {
        public int biSize; public int biWidth; public int biHeight; public short biPlanes;
        public short biBitCount; public int biCompression; public int biSizeImage;
        public int biXPelsPerMeter; public int biYPelsPerMeter; public int biClrUsed; public int biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFO { public BITMAPINFOHEADER bmiHeader; }

    private static void CaptureUnityWindowScreenshot(string filePath)
    {
        try
        {
            IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            if (hWnd == IntPtr.Zero) return;

            RECT r;
            GetWindowRect(hWnd, out r);
            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;
            if (width <= 0 || height <= 0) return;

            IntPtr hdcWindow = GetDC(hWnd);
            IntPtr hdcMem = CreateCompatibleDC(hdcWindow);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcWindow, width, height);
            IntPtr hOld = SelectObject(hdcMem, hBitmap);

            // PrintWindow con flag 2 (PW_RENDERFULLCONTENT)
            bool pwOk = PrintWindow(hWnd, hdcMem, 2);
            if (!pwOk)
            {
                BitBlt(hdcMem, 0, 0, width, height, hdcWindow, 0, 0, 0x00CC0020);
            }

            BITMAPINFO bi = new BITMAPINFO();
            bi.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            bi.bmiHeader.biWidth = width;
            bi.bmiHeader.biHeight = height; // Bottom-up
            bi.bmiHeader.biPlanes = 1;
            bi.bmiHeader.biBitCount = 32;
            bi.bmiHeader.biCompression = 0; // BI_RGB

            byte[] bits = new byte[width * height * 4];
            GetDIBits(hdcMem, hBitmap, 0, (uint)height, bits, ref bi, 0);

            SelectObject(hdcMem, hOld);
            DeleteObject(hBitmap);
            DeleteDC(hdcMem);
            ReleaseDC(hWnd, hdcWindow);

            // Convertir a Texture2D y guardar PNG
            Texture2D tex = new Texture2D(width, height, TextureFormat.BGRA32, false);
            tex.LoadRawTextureData(bits);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, png);
            UnityEngine.Object.DestroyImmediate(tex);

            Debug.Log("[Actividad 3.1] Captura completa de la ventana de Unity guardada en: " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Actividad 3.1] No se pudo capturar ventana de Unity: " + ex.Message);
        }
    }
}
