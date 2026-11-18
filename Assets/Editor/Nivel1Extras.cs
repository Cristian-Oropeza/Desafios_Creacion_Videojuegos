#if UNITY_EDITOR
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menu "Menu Tools/Extras del Nivel" para agregar al Nivel1:
// - Virtual Joystick (conectado al Aguila)
// - Menu de Pausa con ESC (Reanudar, Opciones, Salir al Menu)
public static class Nivel1Extras
{
    private const string ROOT = "Menu Tools/Extras del Nivel/";

    private const string P_BTN = "Assets/FreeButtonSet/Textures/buttons/";
    private const string P_CTRL = "Assets/FreeButtonSet/Textures/controls/";
    private const string P_ICO = "Assets/FreeButtonSet/Textures/icons/64x64/";

    // Paleta (misma del menu principal)
    private static Color ColPanel     => Hex("1E3A5F");
    private static Color ColTexto     => Hex("E8FFF5");
    private static Color ColTitulo    => Hex("A0F0E0");
    private static Color ColBtnHigh   => Hex("88E8DD");
    private static Color ColBtnPress  => Hex("2A8278");
    private static Color ColToggleLbl => Hex("7CC9B0");
    private static Color ColFill      => Hex("4ECDC4");

    // =======================================================================
    // AGREGAR TODO (Joystick + Menu Pausa)
    // =======================================================================
    [MenuItem(ROOT + "AGREGAR TODO (Joystick + Menu Pausa)", priority = 1)]
    public static void AgregarTodo()
    {
        if (!EditorUtility.DisplayDialog("Extras del Nivel",
            "Esto va a agregar el Joystick y el Menu de Pausa al Nivel actual.\n\n" +
            "ASEGURATE de tener abierta la escena Nivel1 (no la del Menu).\n\n" +
            "Continuar?", "Si", "Cancelar"))
            return;

        AgregarJoystick();
        AgregarMenuPausa();

        EditorUtility.DisplayDialog("Listo",
            "Joystick y Menu de Pausa agregados. Guarda con Ctrl+S.", "OK");
    }

    // =======================================================================
    // JOYSTICK
    // =======================================================================
    [MenuItem(ROOT + "Solo Joystick", priority = 10)]
    public static void AgregarJoystick()
    {
        // Canvas de UI si no existe
        var canvas = GameObject.Find("CanvasUI");
        if (canvas == null) canvas = CrearCanvasUI("CanvasUI", 10);
        AsegurarEventSystem();

        // Si ya habia un joystick, borrarlo
        var existBG = GameObject.Find("JoystickBackground");
        if (existBG != null) Object.DestroyImmediate(existBG);

        // JoystickBackground (circulo grande, abajo izquierda)
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(P_BTN + "button_round_320.png");

        var bgGO = new GameObject("JoystickBackground",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VirtualJoystick));
        bgGO.transform.SetParent(canvas.transform, false);

        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0);
        bgRT.anchorMax = new Vector2(0, 0);
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = new Vector2(250, 250);
        bgRT.anchoredPosition = new Vector2(200, 200);

        var bgImg = bgGO.GetComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = new Color(1, 1, 1, 0.5f);

        // JoystickHandle (circulo chico, hijo del background)
        var handleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(P_BTN + "button_round_130.png");
        var handleGO = new GameObject("JoystickHandle",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handleGO.transform.SetParent(bgGO.transform, false);

        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0.5f, 0.5f);
        handleRT.anchorMax = new Vector2(0.5f, 0.5f);
        handleRT.pivot = new Vector2(0.5f, 0.5f);
        handleRT.sizeDelta = new Vector2(100, 100);
        handleRT.anchoredPosition = Vector2.zero;

        var handleImg = handleGO.GetComponent<Image>();
        handleImg.sprite = handleSprite;
        handleImg.color = new Color(1, 1, 1, 0.9f);

        // Asignar refs al script del joystick
        var joy = bgGO.GetComponent<VirtualJoystick>();
        joy.background = bgRT;
        joy.handle = handleRT;
        joy.handleRange = 1f;

        // Conectar al AguilaController si existe
        var aguila = GameObject.Find("Aguila");
        if (aguila != null)
        {
            var ctrl = aguila.GetComponent<AguilaController>();
            if (ctrl != null)
            {
                ctrl.joystick = joy;
                EditorUtility.SetDirty(ctrl);
                Debug.Log("[Nivel1Extras] Joystick asignado al AguilaController.");
            }
            else
            {
                Debug.LogWarning("[Nivel1Extras] Aguila no tiene AguilaController.");
            }
        }
        else
        {
            Debug.LogWarning("[Nivel1Extras] No encontre 'Aguila' en la escena. Asigna el JoystickBackground al slot Joystick manualmente.");
        }

        Selection.activeGameObject = bgGO;
        Debug.Log("[Nivel1Extras] Joystick virtual agregado.");
    }

    // =======================================================================
    // MENU PAUSA
    // =======================================================================
    [MenuItem(ROOT + "Solo Menu Pausa", priority = 20)]
    public static void AgregarMenuPausa()
    {
        AsegurarEventSystem();

        // Si ya habia, borrarlo
        var existCanvas = GameObject.Find("CanvasPausa");
        if (existCanvas != null) Object.DestroyImmediate(existCanvas);
        var existCtrl = GameObject.Find("PausaController");
        if (existCtrl != null) Object.DestroyImmediate(existCtrl);

        // Canvas Pausa con sortingOrder alto para quedar encima de todo
        var canvas = CrearCanvasUI("CanvasPausa", 100);

        // ===== Panel de Pausa =====
        var pausaPanel = CrearPanelFondoOscuro(canvas.transform, "PausaPanel");

        // Vertical Layout en el panel
        var vlg = pausaPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 25;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 200, 200);

        // Titulo
        var titulo = CrearTexto(pausaPanel.transform, "TituloPausa", "PAUSA", 96, ColTitulo, TextAlignmentOptions.Center);
        titulo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        titulo.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;
        titulo.AddComponent<LayoutElement>().preferredHeight = 150;

        // Botones
        var btnReanudar = CrearBotonLargo(pausaPanel.transform, "BtnReanudar", "Reanudar", P_ICO + "play.png");
        var btnOpciones = CrearBotonLargo(pausaPanel.transform, "BtnOpcionesPausa", "Opciones", P_ICO + "settings.png");
        var btnSalir = CrearBotonLargo(pausaPanel.transform, "BtnSalirMenu", "Salir al Menu", P_ICO + "arrow_left.png");

        // ===== Panel de Opciones de Pausa =====
        var opcionesPanel = CrearPanelFondoOscuro(canvas.transform, "OpcionesPausaPanel");
        var vlgOpt = opcionesPanel.AddComponent<VerticalLayoutGroup>();
        vlgOpt.spacing = 40;
        vlgOpt.childAlignment = TextAnchor.UpperCenter;
        vlgOpt.childControlWidth = true;
        vlgOpt.childControlHeight = true;
        vlgOpt.childForceExpandWidth = false;
        vlgOpt.childForceExpandHeight = false;
        vlgOpt.padding = new RectOffset(120, 120, 120, 80);

        var tituloOpt = CrearTexto(opcionesPanel.transform, "TituloOpcionesPausa", "Opciones", 64, ColTitulo, TextAlignmentOptions.Center);
        tituloOpt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        tituloOpt.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;
        tituloOpt.AddComponent<LayoutElement>().preferredHeight = 90;

        var lblVol = CrearTexto(opcionesPanel.transform, "LabelVolumen", "Volumen", 40, ColToggleLbl, TextAlignmentOptions.Left);
        lblVol.AddComponent<LayoutElement>().preferredHeight = 60;

        var slider = CrearSlider(opcionesPanel.transform, "SliderVolumen");
        var btnVolver = CrearBotonVolver(opcionesPanel.transform, "BtnVolverOpcionesPausa");

        // ===== PausaController =====
        var pausaGO = new GameObject("PausaController");
        var pm = pausaGO.AddComponent<PausaMenu>();

        var tipo = typeof(PausaMenu);
        var bf = BindingFlags.Instance | BindingFlags.NonPublic;

        tipo.GetField("pausaPanel", bf)?.SetValue(pm, pausaPanel);
        tipo.GetField("opcionesPanel", bf)?.SetValue(pm, opcionesPanel);
        tipo.GetField("btnReanudar", bf)?.SetValue(pm, btnReanudar.GetComponent<Button>());
        tipo.GetField("btnOpciones", bf)?.SetValue(pm, btnOpciones.GetComponent<Button>());
        tipo.GetField("btnSalirMenu", bf)?.SetValue(pm, btnSalir.GetComponent<Button>());
        tipo.GetField("btnVolverDeOpciones", bf)?.SetValue(pm, btnVolver.GetComponent<Button>());
        tipo.GetField("volumeSlider", bf)?.SetValue(pm, slider);
        tipo.GetField("escenaMenu", bf)?.SetValue(pm, "Menu");

        pausaPanel.SetActive(false);
        opcionesPanel.SetActive(false);

        EditorUtility.SetDirty(pm);
        Debug.Log("[Nivel1Extras] Menu de Pausa agregado. Prueba con ESC durante Play.");
    }

    // =======================================================================
    // Helpers: Canvas, Panel, Boton, Slider, Texto
    // =======================================================================
    static GameObject CrearCanvasUI(string nombre, int sortingOrder)
    {
        var go = new GameObject(nombre, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortingOrder;
        var cs = go.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
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

    static GameObject CrearPanelFondoOscuro(Transform padre, string nombre)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        var c = ColPanel;
        c.a = 0.92f; // ligeramente translucido para que veas el juego de fondo
        img.color = c;
        return go;
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

        var icGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        icGO.transform.SetParent(go.transform, false);
        var icImg = icGO.GetComponent<Image>();
        icImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconoPath);
        icImg.preserveAspect = true;
        var icLe = icGO.GetComponent<LayoutElement>();
        icLe.preferredWidth = 48;
        icLe.preferredHeight = 48;

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
        txLe.preferredWidth = 400;
        txLe.preferredHeight = 80;

        return go;
    }

    static GameObject CrearBotonVolver(Transform padre, string nombre)
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

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = ColBtnHigh;
        colors.pressedColor = ColBtnPress;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(go.transform, false);
        var icImg = icon.GetComponent<Image>();
        icImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(P_ICO + "arrow_left.png");
        icImg.preserveAspect = true;
        var icRT = icon.GetComponent<RectTransform>();
        icRT.anchorMin = new Vector2(0.5f, 0.5f);
        icRT.anchorMax = new Vector2(0.5f, 0.5f);
        icRT.sizeDelta = new Vector2(48, 48);
        icRT.anchoredPosition = Vector2.zero;

        return go;
    }

    static Slider CrearSlider(Transform padre, string nombre)
    {
        var sliderGO = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Slider), typeof(LayoutElement));
        sliderGO.transform.SetParent(padre, false);
        var le = sliderGO.GetComponent<LayoutElement>();
        le.preferredWidth = 600;
        le.preferredHeight = 50;

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

        return slider;
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

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}
#endif
