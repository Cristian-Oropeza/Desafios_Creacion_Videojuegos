#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menu personalizado "Menu Tools" para construir el menu con FreeButtonSet.
// Automatiza TODO el setup segun la actividad "Creacion de Menu".
public static class MenuBuilder
{
    private const string ROOT = "Menu Tools/";
    private const string PASOS = ROOT + "Pasos individuales/";

    // Paths de assets
    private const string P_BTN = "Assets/FreeButtonSet/Textures/buttons/";
    private const string P_ICO = "Assets/FreeButtonSet/Textures/icons/64x64/";
    private const string P_CTRL = "Assets/FreeButtonSet/Textures/controls/";

    // ===== Paleta azul navy + verdes teal =====
    private static Color ColPanel     => Hex("1E3A5F"); // azul navy oscuro (fondo panel)
    private static Color ColTexto     => Hex("E8FFF5"); // casi blanco con tinte verde
    private static Color ColTitulo    => Hex("A0F0E0"); // verde turquesa claro (titulos)
    private static Color ColBtnHigh   => Hex("88E8DD"); // turquesa claro al pasar mouse
    private static Color ColBtnPress  => Hex("2A8278"); // teal oscuro al hacer click
    private static Color ColToggleLbl => Hex("7CC9B0"); // teal medio (labels toggles)
    private static Color ColFill      => Hex("4ECDC4"); // turquesa (fill slider)

    // Nombre de la escena de juego que carga el boton Jugar
    private const string NOMBRE_ESCENA_JUEGO = "Nivel1";

    // =======================================================================
    // REHACER TODO (limpia escena + setup)
    // =======================================================================
    [MenuItem(ROOT + "REHACER TODO (limpiar escena y setup)", priority = 1)]
    public static void RehacerTodo()
    {
        if (!EditorUtility.DisplayDialog("Rehacer Menu",
            "Esto va a LIMPIAR la escena actual y rehacer el menu completo.\n\nContinuar?",
            "Si, rehacer", "Cancelar"))
            return;

        LimpiarEscena();
        EjecutarSetup();

        EditorUtility.DisplayDialog("Listo",
            "Menu creado con paleta azul+verde.\n\n" +
            "Guarda con Ctrl+S (si es escena nueva, elige Assets/Scenes/Menu.unity).\n\n" +
            "Las escenas Menu y Nivel1 ya quedaron en Build Settings.", "OK");
    }

    [MenuItem(ROOT + "SETUP COMPLETO (sin limpiar)", priority = 2)]
    public static void SetupCompleto() => EjecutarSetup();

    private static void EjecutarSetup()
    {
        ConfigurarImports();
        var canvas = CrearCanvas();
        var (mainPanel, optionsPanel, creditsPanel) = CrearPaneles(canvas);

        CrearBotonesMain(mainPanel);
        CrearElementosOptions(optionsPanel);
        CrearElementosCredits(creditsPanel);

        var musicPlayer = CrearAudioSource();
        var menuMgr = CrearMenuManager();
        ConectarReferencias(menuMgr, mainPanel, optionsPanel, creditsPanel, musicPlayer);

        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        GuardarComoMenu();
        AgregarEscenasABuildSettings("Assets/Scenes/Menu.unity", "Assets/Scenes/Nivel1.unity");
    }

    static void LimpiarEscena()
    {
        var escena = EditorSceneManager.GetActiveScene();
        foreach (var go in escena.GetRootGameObjects())
        {
            if (go.name == "Main Camera") continue;
            if (go.name.Contains("Light")) continue;
            Object.DestroyImmediate(go);
        }
    }

    // =======================================================================
    // PASO 1 - Configurar imports con 9-slice
    // =======================================================================
    [MenuItem(PASOS + "1. Configurar imports de sprites (9-slice)", priority = 100)]
    public static void ConfigurarImports()
    {
        Slice9(P_BTN + "button_100.png", new Vector4(30, 30, 30, 30));
        Slice9(P_BTN + "button_150.png", new Vector4(50, 30, 50, 30));
        Slice9(P_BTN + "button_200.png", new Vector4(60, 40, 60, 40));
        Slice9(P_BTN + "button_round_130.png", new Vector4(50, 50, 50, 50));
        Slice9(P_BTN + "button_round_220.png", new Vector4(100, 100, 100, 100));
        Slice9(P_BTN + "button_round_320.png", new Vector4(150, 150, 150, 150));
        Slice9(P_CTRL + "universal_panel_10.png", new Vector4(8, 8, 8, 8));
        Slice9(P_CTRL + "universal_panel_20.png", new Vector4(15, 15, 15, 15));
        Slice9(P_CTRL + "universal_panel_40.png", new Vector4(30, 30, 30, 30));
        Slice9(P_CTRL + "toggle_bg_100.png", new Vector4(20, 20, 20, 20));
        Slice9(P_CTRL + "radio_100.png", new Vector4(45, 45, 45, 45));

        AssetDatabase.Refresh();
        Debug.Log("[MenuBuilder] Imports configurados con 9-slice.");
    }

    static void Slice9(string path, Vector4 border)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning($"[MenuBuilder] No encontre {path}"); return; }
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.filterMode = FilterMode.Bilinear;
        imp.textureCompression = TextureImporterCompression.Uncompressed;

        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.spriteBorder = border;
        imp.SetTextureSettings(s);
        imp.SaveAndReimport();
    }

    // =======================================================================
    // PASO 2 - Canvas
    // =======================================================================
    [MenuItem(PASOS + "2. Crear Canvas", priority = 110)]
    public static GameObject CrearCanvas()
    {
        var exist = GameObject.Find("Canvas");
        if (exist != null) Object.DestroyImmediate(exist);

        var go = new GameObject("Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;

        AsegurarEventSystem();
        Debug.Log("[MenuBuilder] Canvas creado.");
        return go;
    }

    static void AsegurarEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
        var tipo = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (tipo != null) es.AddComponent(tipo);
        else es.AddComponent<StandaloneInputModule>();
    }

    // =======================================================================
    // PASO 3 - Paneles
    // =======================================================================
    [MenuItem(PASOS + "3. Crear 3 Paneles (Main, Options, Credits)", priority = 120)]
    public static void CrearPanelesMenu()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) canvas = CrearCanvas();
        CrearPaneles(canvas);
    }

    public static (GameObject main, GameObject options, GameObject credits) CrearPaneles(GameObject canvas)
    {
        var main = CrearPanel(canvas.transform, "MainPanel");
        var opt = CrearPanel(canvas.transform, "OptionsPanel");
        var cred = CrearPanel(canvas.transform, "CreditsPanel");

        // MainPanel: Vertical Layout con padding arriba/abajo para que el titulo no se pegue
        var vlgMain = main.AddComponent<VerticalLayoutGroup>();
        vlgMain.spacing = 25;
        vlgMain.childAlignment = TextAnchor.MiddleCenter;
        vlgMain.childControlWidth = false;
        vlgMain.childControlHeight = false;
        vlgMain.childForceExpandWidth = false;
        vlgMain.childForceExpandHeight = false;
        vlgMain.padding = new RectOffset(0, 0, 120, 120);

        // Titulo "Game Scholar"
        var titulo = CrearTexto(main.transform, "Titulo", "Game Scholar", 72, ColTitulo, TextAlignmentOptions.Center);
        var tmpTit = titulo.GetComponent<TextMeshProUGUI>();
        tmpTit.textWrappingMode = TextWrappingModes.NoWrap;
        tmpTit.fontStyle = FontStyles.Bold;
        var leTit = titulo.AddComponent<LayoutElement>();
        leTit.preferredHeight = 120;
        leTit.preferredWidth = 1200;

        // Espaciador entre titulo y botones
        var sep = new GameObject("Separador", typeof(RectTransform));
        sep.transform.SetParent(main.transform, false);
        var leSep = sep.AddComponent<LayoutElement>();
        leSep.preferredHeight = 40;
        leSep.preferredWidth = 10;

        Debug.Log("[MenuBuilder] 3 paneles creados.");
        return (main, opt, cred);
    }

    static GameObject CrearPanel(Transform padre, string nombre)
    {
        var exist = GameObject.Find(nombre);
        if (exist != null) Object.DestroyImmediate(exist);

        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = null;
        img.color = ColPanel;
        return go;
    }

    // =======================================================================
    // PASO 4 - Botones del MainPanel
    // =======================================================================
    [MenuItem(PASOS + "4. Crear botones del MainPanel", priority = 130)]
    public static void CrearBotonesMainMenu()
    {
        var main = GameObject.Find("MainPanel");
        if (main == null) { EditorUtility.DisplayDialog("Falta MainPanel", "Primero crea los paneles.", "OK"); return; }
        CrearBotonesMain(main);
    }

    public static void CrearBotonesMain(GameObject main)
    {
        CrearBotonLargo(main.transform, "BtnJugar",    "Jugar",    P_ICO + "play.png");
        CrearBotonLargo(main.transform, "BtnOpciones", "Opciones", P_ICO + "settings.png");
        CrearBotonLargo(main.transform, "BtnCreditos", "Creditos", P_ICO + "user.png");
        CrearBotonLargo(main.transform, "BtnSalir",    "Salir",    P_ICO + "x.png");

        Debug.Log("[MenuBuilder] 4 botones del MainPanel creados.");
    }

    static GameObject CrearBotonLargo(Transform padre, string nombre, string texto, string iconoPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(P_BTN + "button_200.png");

        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 100);

        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = ColBtnHigh;
        colors.pressedColor = ColBtnPress;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(30, 30, 10, 10);

        // Icono
        var icGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        icGO.transform.SetParent(go.transform, false);
        var icImg = icGO.GetComponent<Image>();
        icImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconoPath);
        icImg.color = Color.white;
        icImg.preserveAspect = true;
        var icLe = icGO.GetComponent<LayoutElement>();
        icLe.preferredWidth = 48;
        icLe.preferredHeight = 48;

        // Texto TMP
        var txGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        txGO.transform.SetParent(go.transform, false);
        var tmp = txGO.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = 60;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.fontStyle = FontStyles.Bold;
        var txLe = txGO.AddComponent<LayoutElement>();
        txLe.preferredWidth = 300;
        txLe.preferredHeight = 80;

        return go;
    }

    // =======================================================================
    // PASO 5 - OptionsPanel
    // =======================================================================
    [MenuItem(PASOS + "5. Crear elementos del OptionsPanel", priority = 140)]
    public static void CrearElementosOptionsMenu()
    {
        var opt = GameObject.Find("OptionsPanel");
        if (opt == null) { EditorUtility.DisplayDialog("Falta OptionsPanel", "Primero crea los paneles.", "OK"); return; }
        CrearElementosOptions(opt);
    }

    public static void CrearElementosOptions(GameObject optPanel)
    {
        var vlg = optPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(120, 120, 80, 80);
        vlg.spacing = 40;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        var titulo = CrearTexto(optPanel.transform, "TituloOpciones", "Opciones", 64, ColTitulo, TextAlignmentOptions.Center);
        titulo.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;
        titulo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        titulo.AddComponent<LayoutElement>().preferredHeight = 90;

        var lblVol = CrearTexto(optPanel.transform, "LabelVolumen", "Volumen", 40, ColToggleLbl, TextAlignmentOptions.Left);
        lblVol.AddComponent<LayoutElement>().preferredHeight = 60;

        CrearSlider(optPanel.transform, "Slider");
        CrearToggle(optPanel.transform, "ToggleMusica", "Musica");
        CrearToggle(optPanel.transform, "ToggleEfectos", "Efectos");
        CrearBotonVolver(optPanel.transform, "BtnVolverOptions");

        Debug.Log("[MenuBuilder] OptionsPanel con Slider + 2 Toggles + Volver.");
    }

    // =======================================================================
    // PASO 6 - CreditsPanel
    // =======================================================================
    [MenuItem(PASOS + "6. Crear elementos del CreditsPanel", priority = 150)]
    public static void CrearElementosCreditsMenu()
    {
        var cred = GameObject.Find("CreditsPanel");
        if (cred == null) { EditorUtility.DisplayDialog("Falta CreditsPanel", "Primero crea los paneles.", "OK"); return; }
        CrearElementosCredits(cred);
    }

    public static void CrearElementosCredits(GameObject credPanel)
    {
        var vlg = credPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(120, 120, 120, 80);
        vlg.spacing = 30;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        var titulo = CrearTexto(credPanel.transform, "TituloCreditos", "Creditos", 64, ColTitulo, TextAlignmentOptions.Center);
        titulo.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;
        titulo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        titulo.AddComponent<LayoutElement>().preferredHeight = 90;

        var texto = CrearTexto(credPanel.transform,
            "TextoCreditos",
            "Desarrollado por:\n\nCristian Oropeza\n\nAsignatura: Creacion de Videojuegos\nCuatrimestre 10\n\nGracias por jugar!",
            40, ColTexto, TextAlignmentOptions.Center);
        texto.AddComponent<LayoutElement>().preferredHeight = 500;

        CrearBotonVolver(credPanel.transform, "BtnVolverCredits");

        Debug.Log("[MenuBuilder] CreditsPanel con texto + Volver.");
    }

    // =======================================================================
    // Helpers: Slider, Toggle, Boton Volver, Texto
    // =======================================================================
    static void CrearSlider(Transform padre, string nombre)
    {
        var sliderGO = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Slider), typeof(LayoutElement));
        sliderGO.transform.SetParent(padre, false);
        sliderGO.GetComponent<LayoutElement>().preferredWidth = 600;
        sliderGO.GetComponent<LayoutElement>().preferredHeight = 50;

        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.wholeNumbers = false;
        slider.value = 0.7f;

        var bg = CrearHijoImagen(sliderGO.transform, "Background",
            AssetDatabase.LoadAssetAtPath<Sprite>(P_CTRL + "universal_panel_10.png"),
            Color.white, Image.Type.Sliced);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.5f);
        bgRT.anchorMax = new Vector2(1, 0.5f);
        bgRT.sizeDelta = new Vector2(0, 18);
        bgRT.anchoredPosition = Vector2.zero;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.5f);
        faRT.anchorMax = new Vector2(1, 0.5f);
        faRT.offsetMin = new Vector2(0, -9);
        faRT.offsetMax = new Vector2(-22, 9);

        var fill = CrearHijoImagen(fillArea.transform, "Fill",
            AssetDatabase.LoadAssetAtPath<Sprite>(P_CTRL + "radio_100.png"),
            ColFill, Image.Type.Sliced);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        slider.fillRect = fillRT;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGO.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(22, 0);
        haRT.offsetMax = new Vector2(-22, 0);

        var handle = CrearHijoImagen(handleArea.transform, "Handle",
            AssetDatabase.LoadAssetAtPath<Sprite>(P_CTRL + "slider_handle.png"),
            Color.white, Image.Type.Simple);
        var handImg = handle.GetComponent<Image>();
        handImg.preserveAspect = true;
        var handRT = handle.GetComponent<RectTransform>();
        handRT.sizeDelta = new Vector2(44, 0);

        slider.handleRect = handRT;
        slider.targetGraphic = handImg;
    }

    static void CrearToggle(Transform padre, string nombre, string labelTexto)
    {
        var toggleGO = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Toggle), typeof(LayoutElement));
        toggleGO.transform.SetParent(padre, false);
        var le = toggleGO.GetComponent<LayoutElement>();
        le.preferredWidth = 600;
        le.preferredHeight = 70;

        var toggle = toggleGO.GetComponent<Toggle>();
        toggle.isOn = true;
        toggle.transition = Selectable.Transition.ColorTint;

        var bg = CrearHijoImagen(toggleGO.transform, "Background",
            AssetDatabase.LoadAssetAtPath<Sprite>(P_CTRL + "toggle_bg_100.png"),
            Color.white, Image.Type.Sliced);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(1, 0.5f);
        bgRT.anchorMax = new Vector2(1, 0.5f);
        bgRT.pivot = new Vector2(1, 0.5f);
        bgRT.sizeDelta = new Vector2(64, 64);
        bgRT.anchoredPosition = Vector2.zero;
        toggle.targetGraphic = bg.GetComponent<Image>();

        var check = CrearHijoImagen(bg.transform, "Checkmark",
            AssetDatabase.LoadAssetAtPath<Sprite>(P_CTRL + "radio_60.png"),
            ColFill, Image.Type.Simple);
        check.GetComponent<Image>().preserveAspect = true;
        var checkRT = check.GetComponent<RectTransform>();
        checkRT.anchorMin = new Vector2(0.5f, 0.5f);
        checkRT.anchorMax = new Vector2(0.5f, 0.5f);
        checkRT.sizeDelta = new Vector2(36, 36);
        checkRT.anchoredPosition = Vector2.zero;
        toggle.graphic = check.GetComponent<Image>();

        var lbl = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        lbl.transform.SetParent(toggleGO.transform, false);
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = labelTexto;
        tmp.fontSize = 40;
        tmp.color = ColToggleLbl;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        var lblRT = lbl.GetComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0, 0);
        lblRT.anchorMax = new Vector2(1, 1);
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = new Vector2(-80, 0);
    }

    static void CrearBotonVolver(Transform padre, string nombre)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(padre, false);

        var le = go.GetComponent<LayoutElement>();
        le.ignoreLayout = true;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(70, -70);
        rt.sizeDelta = new Vector2(90, 90);

        var img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(P_BTN + "button_round_130.png");
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = ColBtnHigh;
        colors.pressedColor = ColBtnPress;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        var icon = CrearHijoImagen(go.transform, "Icon",
            AssetDatabase.LoadAssetAtPath<Sprite>(P_ICO + "arrow_left.png"),
            Color.white, Image.Type.Simple);
        icon.GetComponent<Image>().preserveAspect = true;
        var icRT = icon.GetComponent<RectTransform>();
        icRT.anchorMin = new Vector2(0.5f, 0.5f);
        icRT.anchorMax = new Vector2(0.5f, 0.5f);
        icRT.sizeDelta = new Vector2(48, 48);
        icRT.anchoredPosition = Vector2.zero;
    }

    static GameObject CrearTexto(Transform padre, string nombre, string texto, int size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(padre, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        return go;
    }

    static GameObject CrearHijoImagen(Transform padre, string nombre, Sprite sprite, Color color, Image.Type tipo)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.type = tipo;
        return go;
    }

    // =======================================================================
    // PASO 7 - AudioSource + MenuManager
    // =======================================================================
    [MenuItem(PASOS + "7. Crear MenuManager + MusicPlayer", priority = 160)]
    public static void CrearMenuManagerMenu()
    {
        var mp = CrearAudioSource();
        var mm = CrearMenuManager();
        ConectarReferencias(mm,
            GameObject.Find("MainPanel"),
            GameObject.Find("OptionsPanel"),
            GameObject.Find("CreditsPanel"),
            mp);
    }

    public static GameObject CrearAudioSource()
    {
        var exist = GameObject.Find("MusicPlayer");
        if (exist != null) return exist;

        var go = new GameObject("MusicPlayer", typeof(AudioSource));
        var au = go.GetComponent<AudioSource>();
        au.playOnAwake = true;
        au.loop = true;
        au.volume = 0.7f;

        Debug.Log("[MenuBuilder] MusicPlayer creado. Arrastra un AudioClip al campo AudioClip del Inspector.");
        return go;
    }

    public static GameObject CrearMenuManager()
    {
        var exist = GameObject.Find("MenuManager");
        if (exist != null) Object.DestroyImmediate(exist);

        var go = new GameObject("MenuManager");
        var tipo = System.Type.GetType("MenuManager, Assembly-CSharp");
        if (tipo == null)
        {
            Debug.LogError("[MenuBuilder] No existe la clase MenuManager en el proyecto. Revisa Assets/Scripts/MenuManager.cs");
            return go;
        }
        go.AddComponent(tipo);
        return go;
    }

    // =======================================================================
    // PASO 8 - Conectar referencias
    // =======================================================================
    [MenuItem(PASOS + "8. Conectar referencias del MenuManager", priority = 170)]
    public static void ConectarReferenciasMenu()
    {
        var mm = GameObject.Find("MenuManager");
        if (mm == null) { EditorUtility.DisplayDialog("Falta MenuManager", "Primero crea el MenuManager.", "OK"); return; }
        ConectarReferencias(mm,
            GameObject.Find("MainPanel"),
            GameObject.Find("OptionsPanel"),
            GameObject.Find("CreditsPanel"),
            GameObject.Find("MusicPlayer"));
    }

    public static void ConectarReferencias(GameObject mmGO, GameObject main, GameObject opt, GameObject cred, GameObject music)
    {
        var mm = mmGO.GetComponent<MonoBehaviour>();
        if (mm == null) { Debug.LogError("[MenuBuilder] MenuManager sin script."); return; }

        var tipo = mm.GetType();
        var bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

        tipo.GetField("mainPanel", bf)?.SetValue(mm, main);
        tipo.GetField("optionsPanel", bf)?.SetValue(mm, opt);
        tipo.GetField("creditsPanel", bf)?.SetValue(mm, cred);

        var play    = GameObject.Find("BtnJugar")?.GetComponent<Button>();
        var options = GameObject.Find("BtnOpciones")?.GetComponent<Button>();
        var credits = GameObject.Find("BtnCreditos")?.GetComponent<Button>();
        var quit    = GameObject.Find("BtnSalir")?.GetComponent<Button>();
        tipo.GetField("playButton", bf)?.SetValue(mm, play);
        tipo.GetField("optionsButton", bf)?.SetValue(mm, options);
        tipo.GetField("creditsButton", bf)?.SetValue(mm, credits);
        tipo.GetField("quitButton", bf)?.SetValue(mm, quit);

        var backs = new List<Button>();
        var bo = GameObject.Find("BtnVolverOptions")?.GetComponent<Button>();
        var bc = GameObject.Find("BtnVolverCredits")?.GetComponent<Button>();
        if (bo != null) backs.Add(bo);
        if (bc != null) backs.Add(bc);
        tipo.GetField("backButtons", bf)?.SetValue(mm, backs.ToArray());

        var slider = GameObject.Find("Slider")?.GetComponent<Slider>();
        tipo.GetField("volumeSlider", bf)?.SetValue(mm, slider);

        var toggle = GameObject.Find("ToggleMusica")?.GetComponent<Toggle>();
        tipo.GetField("muteToggle", bf)?.SetValue(mm, toggle);

        var audio = music?.GetComponent<AudioSource>();
        tipo.GetField("uiAudio", bf)?.SetValue(mm, audio);

        tipo.GetField("gameSceneName", bf)?.SetValue(mm, NOMBRE_ESCENA_JUEGO);

        EditorUtility.SetDirty(mm);
        Debug.Log($"[MenuBuilder] MenuManager conectado. gameSceneName='{NOMBRE_ESCENA_JUEGO}'.");
    }

    // =======================================================================
    // PASO 9 - Guardar escena
    // =======================================================================
    [MenuItem(PASOS + "9. Guardar escena como Menu", priority = 180)]
    public static void GuardarComoMenu()
    {
        const string carpeta = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(carpeta))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var escena = EditorSceneManager.GetActiveScene();
        string nuevoPath = $"{carpeta}/Menu.unity";
        EditorSceneManager.SaveScene(escena, nuevoPath);
        Debug.Log($"[MenuBuilder] Escena guardada como {nuevoPath}.");
    }

    // =======================================================================
    // PASO 10 - Agregar escenas a Build Settings
    // =======================================================================
    [MenuItem(PASOS + "10. Agregar Menu y Nivel1 a Build Settings", priority = 190)]
    public static void AgregarEscenasABuildSettingsMenu()
    {
        AgregarEscenasABuildSettings("Assets/Scenes/Menu.unity", "Assets/Scenes/Nivel1.unity");
    }

    static void AgregarEscenasABuildSettings(params string[] rutas)
    {
        var actuales = EditorBuildSettings.scenes.ToList();
        int agregadas = 0, faltantes = 0;

        foreach (var ruta in rutas)
        {
            if (string.IsNullOrEmpty(ruta)) continue;
            if (!System.IO.File.Exists(ruta))
            {
                Debug.LogWarning($"[MenuBuilder] Escena no existe en disco: {ruta}");
                faltantes++;
                continue;
            }
            if (actuales.Any(s => s.path == ruta)) continue;
            actuales.Add(new EditorBuildSettingsScene(ruta, true));
            agregadas++;
        }

        // Asegurar Menu.unity primero
        actuales = actuales.OrderBy(s => s.path.EndsWith("/Menu.unity") ? 0 : 1).ToList();

        EditorBuildSettings.scenes = actuales.ToArray();
        Debug.Log($"[MenuBuilder] Build Settings: {agregadas} escena(s) agregada(s), {faltantes} no existen en disco.");
    }

    // =======================================================================
    // Helpers
    // =======================================================================
    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}
#endif
