#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Menu personalizado "Aguila Tools" para la Actividad "El Aguila y las Gemas" (10 pts).
// Automatiza TODO el setup con submenus.
public static class AguilaMenu
{
    private const string ROOT = "Aguila Tools/";
    private const string PASOS = ROOT + "Pasos individuales/";

    // Dimensiones del mapa (grande para que la camara pueda seguir al aguila)
    private const int MAPA_W = 60;
    private const int MAPA_H = 20;
    private const int MAPA_X0 = -30;
    private const int MAPA_Y0 = -10;
    private const int FILAS_TIERRA = 2;     // franja de tierra abajo del pasto
    private const int FILAS_PASTO = 3;      // franja de pasto encima de la tierra
    private const int NUM_DECORACIONES = 22; // cuantos arboles/arbustos pintar
    private const float ESCALA_DECO = 2f;    // escala de las decoraciones (2x mas grandes)
    private const int MARGEN_X = 15;         // bordes del aguila hacia dentro del mapa en X
    private const int MARGEN_Y_TOP = 3;      // techo del aguila mas abajo que el techo del mapa

    // =======================================================================
    // REHACER TODO - limpia la escena y rehace el setup (lo que normalmente usas)
    // =======================================================================
    [MenuItem(ROOT + "REHACER TODO (limpiar escena y setup)", priority = 1)]
    public static void RehacerTodo()
    {
        if (!EditorUtility.DisplayDialog("Rehacer Todo",
            "Esto va a LIMPIAR la escena actual (borra todo menos Main Camera y luces) " +
            "y despues rehace el setup completo.\n\nContinuar?", "Si, rehacer", "Cancelar"))
            return;

        var escena = EditorSceneManager.GetActiveScene();
        foreach (var go in escena.GetRootGameObjects())
        {
            if (go.name == "Main Camera") continue;
            if (go.name.Contains("Light")) continue;
            Object.DestroyImmediate(go);
        }

        EjecutarSetupCompleto();

        EditorUtility.DisplayDialog("Listo",
            "Escena rehecha. Guarda con Ctrl+S.\n\n" +
            "Si el aguila no se mueve, cambia Active Input Handling a 'Both' en " +
            "Edit > Project Settings > Player > Other Settings.", "OK");
    }

    // =======================================================================
    // SETUP COMPLETO - solo ejecuta los pasos sin limpiar antes
    // =======================================================================
    [MenuItem(ROOT + "SETUP COMPLETO (sin limpiar)", priority = 2)]
    public static void SetupCompleto() => EjecutarSetupCompleto();

    private static void EjecutarSetupCompleto()
    {
        ConfigurarImports();
        CrearTilesDesdeStaticTiles();
        CrearGridYPintarEscenario();
        CrearBordes();
        CrearAguila();
        CrearAnimatorVuelo();
        CrearPrefabsGemaYCereza();
        ColocarGemasYCerezas();
        ConfigurarCamara();
        GuardarComoNivel1();
    }

    // =======================================================================
    // PASO 1 - Configurar imports y Slice
    // =======================================================================
    [MenuItem(PASOS + "1. Configurar imports de sprites", priority = 100)]
    public static void ConfigurarImports()
    {
        SlicePorCeldas("Assets/Images/eagle-attack.png", 16, 16);
        SlicePorCeldas("Assets/Images/gem.png", 16, 16);
        SlicePorCeldas("Assets/Images/cherry.png", 16, 16);
        SlicePorCeldas("Assets/Images/StaticTiles.png", 16, 16);

        AssetDatabase.Refresh();
        Debug.Log("[AguilaTools] 4 spritesheets sliceados con PPU 16.");
    }

    static void SlicePorCeldas(string path, int cw, int ch)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning($"[AguilaTools] No se encontro {path}"); return; }
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = 16;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.isReadable = true;
        imp.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;
        int cols = tex.width / cw;
        int rows = tex.height / ch;
        var slices = new List<SpriteMetaData>();
        int idx = 0;
        string baseName = Path.GetFileNameWithoutExtension(path);
        for (int y = rows - 1; y >= 0; y--)
            for (int x = 0; x < cols; x++)
                slices.Add(new SpriteMetaData
                {
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    rect = new Rect(x * cw, y * ch, cw, ch),
                    name = $"{baseName}_{idx++}"
                });
#pragma warning disable CS0618
        imp.spritesheet = slices.ToArray();
#pragma warning restore CS0618
        imp.SaveAndReimport();
    }

    // =======================================================================
    // PASO 2 - Crear Tiles desde StaticTiles
    // =======================================================================
    [MenuItem(PASOS + "2. Crear Tiles desde StaticTiles", priority = 110)]
    public static void CrearTilesDesdeStaticTiles()
    {
        const string path = "Assets/Images/StaticTiles.png";
        const string carpeta = "Assets/Tiles";
        if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder("Assets", "Tiles");

        var sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>().OrderBy(s => IndiceDe(s.name)).ToArray();
        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "StaticTiles.png no esta sliceado. Corre '1. Configurar imports' primero.", "OK");
            return;
        }

        int creados = 0;
        foreach (var spr in sprites)
        {
            string tp = $"{carpeta}/{spr.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<Tile>(tp) != null) continue;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = spr;
            AssetDatabase.CreateAsset(tile, tp);
            creados++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[AguilaTools] {creados} tiles nuevos (total {sprites.Length}).");
    }

    static int IndiceDe(string name)
    {
        var parts = name.Split('_');
        return int.TryParse(parts[parts.Length - 1], out int n) ? n : 999999;
    }

    // =======================================================================
    // PASO 3 - Crear Grid + Tilemap + pintar pasto + decoraciones automaticas
    // =======================================================================
    [MenuItem(PASOS + "3. Crear Grid + Pintar Escenario (con decoracion)", priority = 120)]
    public static void CrearGridYPintarEscenario()
    {
        var existente = GameObject.Find("Grid");
        if (existente != null) Object.DestroyImmediate(existente);

        var grid = new GameObject("Grid");
        grid.AddComponent<Grid>();
        var tmGO = new GameObject("Suelo");
        tmGO.transform.SetParent(grid.transform, false);
        var tm = tmGO.AddComponent<Tilemap>();
        var tr = tmGO.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 0;

        var tiles = AssetDatabase.FindAssets("t:Tile", new[] { "Assets/Tiles" })
            .Select(g => AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(t => t != null && t.sprite != null)
            .OrderBy(t => IndiceDe(t.name)).ToArray();

        if (tiles.Length == 0)
        {
            Debug.LogWarning("[AguilaTools] No hay tiles. Corre '2. Crear Tiles' primero.");
            return;
        }

        // Clasificar tiles: patron de pasto (verde), patron de tierra (marron), decorativos
        Tile tilePasto = tiles[0]; // fallback
        Tile tileTierra = null;
        var decorativos = new List<Tile>();

        foreach (var t in tiles)
        {
            if (EsPropDecorativo(t)) { decorativos.Add(t); continue; }
            var c = ColorPromedio(t);
            if (tileTierra == null && EsMarron(c)) tileTierra = t;
            if (EsVerdePasto(c) && IndiceDe(t.name) <= IndiceDe(tilePasto.name)) tilePasto = t;
        }
        if (tileTierra == null) tileTierra = tilePasto; // fallback

        // Pintar franja de TIERRA (abajo)
        for (int x = MAPA_X0; x < MAPA_X0 + MAPA_W; x++)
            for (int y = MAPA_Y0; y < MAPA_Y0 + FILAS_TIERRA; y++)
                tm.SetTile(new Vector3Int(x, y, 0), tileTierra);

        // Pintar franja de PASTO (encima de la tierra)
        int yPastoIni = MAPA_Y0 + FILAS_TIERRA;
        for (int x = MAPA_X0; x < MAPA_X0 + MAPA_W; x++)
            for (int y = yPastoIni; y < yPastoIni + FILAS_PASTO; y++)
                tm.SetTile(new Vector3Int(x, y, 0), tilePasto);

        Debug.Log($"[AguilaTools] {decorativos.Count} tiles decorativos detectados.");

        // Colocar decoraciones como GameObjects con SpriteRenderer (escalados 2x)
        CrearDecoracionesComoGameObjects(decorativos, yPastoIni + FILAS_PASTO);

        Debug.Log($"[AguilaTools] Escenario pintado: {FILAS_TIERRA} filas tierra + {FILAS_PASTO} filas pasto + {NUM_DECORACIONES} decoraciones escaladas {ESCALA_DECO}x.");
    }

    // Crea las decoraciones como GameObjects escalados (en vez de tiles 1x1)
    // Asi se ven mas grandes y se distinguen bien.
    static void CrearDecoracionesComoGameObjects(List<Tile> decorativos, int ySueloCielo)
    {
        if (decorativos == null || decorativos.Count == 0) return;

        var existente = GameObject.Find("Decoraciones");
        if (existente != null) Object.DestroyImmediate(existente);
        var padre = new GameObject("Decoraciones");

        System.Random rng = new System.Random(123);
        var xOcupadas = new List<int>();

        for (int i = 0; i < NUM_DECORACIONES; i++)
        {
            int intentos = 0;
            while (intentos < 30)
            {
                int xPos = MAPA_X0 + 2 + rng.Next(MAPA_W - 4);

                // Evitar que se amontonen (distancia minima 2 unidades entre decoraciones)
                bool lejos = true;
                foreach (int ox in xOcupadas)
                    if (Mathf.Abs(ox - xPos) < 2) { lejos = false; break; }

                if (lejos)
                {
                    var tile = decorativos[rng.Next(decorativos.Count)];
                    var g = new GameObject("Deco_" + i);
                    g.transform.SetParent(padre.transform);
                    // Posicion: encima del pasto, con un pequeno offset para que "nazca" del pasto
                    g.transform.position = new Vector3(xPos + 0.5f, ySueloCielo + 0.5f * ESCALA_DECO, 0);
                    g.transform.localScale = new Vector3(ESCALA_DECO, ESCALA_DECO, 1f);
                    var sr = g.AddComponent<SpriteRenderer>();
                    sr.sprite = tile.sprite;
                    sr.sortingOrder = 1;
                    xOcupadas.Add(xPos);
                    break;
                }
                intentos++;
            }
        }
    }

    // Color promedio de los pixeles opacos del tile
    static Color ColorPromedio(Tile tile)
    {
        if (tile == null || tile.sprite == null) return Color.black;
        var tex = tile.sprite.texture;
        if (tex == null) return Color.black;
        try
        {
            var r = tile.sprite.rect;
            float tR = 0, tG = 0, tB = 0;
            int n = 0;
            for (int x = (int)r.x; x < r.x + r.width; x++)
                for (int y = (int)r.y; y < r.y + r.height; y++)
                {
                    var c = tex.GetPixel(x, y);
                    if (c.a > 0.5f) { tR += c.r; tG += c.g; tB += c.b; n++; }
                }
            if (n == 0) return Color.black;
            return new Color(tR / n, tG / n, tB / n);
        }
        catch { return Color.black; }
    }

    static bool EsVerdePasto(Color c) => c.g > 0.3f && c.g > c.r && c.g >= c.b * 0.7f;
    static bool EsMarron(Color c)     => c.r > 0.25f && c.r > c.g && c.g > c.b && c.b < 0.4f;

    // Detecta si un tile es "prop" (tiene transparencia alrededor) vs patron uniforme
    static bool EsPropDecorativo(Tile tile)
    {
        if (tile == null || tile.sprite == null) return false;
        var tex = tile.sprite.texture;
        if (tex == null) return false;

        try
        {
            var rect = tile.sprite.rect;
            int x0 = (int)rect.x;
            int y0 = (int)rect.y;
            int w = (int)rect.width;
            int h = (int)rect.height;

            int transparentes = 0, total = 0;
            // Chequear los 4 bordes
            for (int i = 0; i < w; i++)
            {
                if (tex.GetPixel(x0 + i, y0).a < 0.2f) transparentes++;
                if (tex.GetPixel(x0 + i, y0 + h - 1).a < 0.2f) transparentes++;
                total += 2;
            }
            for (int i = 0; i < h; i++)
            {
                if (tex.GetPixel(x0, y0 + i).a < 0.2f) transparentes++;
                if (tex.GetPixel(x0 + w - 1, y0 + i).a < 0.2f) transparentes++;
                total += 2;
            }
            // Mas del 40% de los bordes transparentes = prop decorativo
            return (float)transparentes / total > 0.4f;
        }
        catch { return false; }
    }

    // =======================================================================
    // PASO 4 - 4 Bordes del mapa (BoxCollider2D, sin Trigger)
    // =======================================================================
    [MenuItem(PASOS + "4. Crear 4 Bordes del Mapa", priority = 130)]
    public static void CrearBordes()
    {
        var existente = GameObject.Find("Bordes");
        if (existente != null) Object.DestroyImmediate(existente);

        var padre = new GameObject("Bordes");

        // Bordes del aguila HACIA DENTRO del mapa, para que nunca llegue al borde del pasto
        // Asi la camara nunca muestra el "corte" del pasto en las orillas
        float xIzq = MAPA_X0 + MARGEN_X;                        // -15
        float xDer = MAPA_X0 + MAPA_W - MARGEN_X;               // 15
        float ySueloPasto = MAPA_Y0 + FILAS_TIERRA + FILAS_PASTO; // -5
        float yTecho = MAPA_Y0 + MAPA_H - MARGEN_Y_TOP;         // 7
        float cxArea = (xIzq + xDer) / 2f;                      // 0
        float cyArea = (ySueloPasto + yTecho) / 2f;             // 1
        float anchoArea = xDer - xIzq;                          // 30
        float altoArea = yTecho - ySueloPasto;                  // 12

        CrearBorde(padre.transform, "Borde_Arriba",     new Vector2(cxArea, yTecho),       new Vector2(anchoArea, 0.5f));
        // Borde SueloCielo mas grueso (1.5 unidades) para que el aguila NUNCA baje al pasto
        CrearBorde(padre.transform, "Borde_SueloCielo", new Vector2(cxArea, ySueloPasto),  new Vector2(anchoArea, 1.5f));
        CrearBorde(padre.transform, "Borde_Izquierda",  new Vector2(xIzq, cyArea),         new Vector2(0.5f, altoArea));
        CrearBorde(padre.transform, "Borde_Derecha",    new Vector2(xDer, cyArea),         new Vector2(0.5f, altoArea));

        Debug.Log($"[AguilaTools] 4 bordes creados. Area del aguila: x=[{xIzq},{xDer}], y=[{ySueloPasto},{yTecho}].");
    }

    static void CrearBorde(Transform padre, string nombre, Vector2 pos, Vector2 size)
    {
        var g = new GameObject(nombre);
        g.transform.SetParent(padre, false);
        g.transform.position = pos;
        var c = g.AddComponent<BoxCollider2D>();
        c.size = size;
    }

    // =======================================================================
    // PASO 5 - Crear Aguila
    // =======================================================================
    [MenuItem(PASOS + "5. Crear Aguila (Jugador)", priority = 140)]
    public static void CrearAguila()
    {
        var existente = GameObject.Find("Aguila");
        if (existente != null) Object.DestroyImmediate(existente);

        var g = new GameObject("Aguila");
        g.transform.position = new Vector3(0, 2, 0);

        var sr = g.AddComponent<SpriteRenderer>();
        var prim = CargarSpriteDeSheet("Assets/Images/eagle-attack.png", 0);
        if (prim != null) sr.sprite = prim;
        sr.sortingOrder = 5;

        var rb = g.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // evita que atraviese bordes al ir rapido

        var col = g.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        g.AddComponent<AguilaController>();

        AsegurarTag("Jugador");
        g.tag = "Jugador";

        Selection.activeGameObject = g;
        Debug.Log("[AguilaTools] Aguila creada en (0,2,0).");
    }

    static Sprite CargarSpriteDeSheet(string path, int indice)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => IndiceDe(s.name)).ToArray();
        if (indice < sprites.Length) return sprites[indice];
        return sprites.Length > 0 ? sprites[0] : null;
    }

    // =======================================================================
    // PASO 6 - Animator Vuelo (loop de los 4 frames)
    // =======================================================================
    [MenuItem(PASOS + "6. Crear Animator Vuelo", priority = 150)]
    public static void CrearAnimatorVuelo()
    {
        var aguila = GameObject.Find("Aguila");
        if (aguila == null) { EditorUtility.DisplayDialog("Falta Aguila", "Primero crea el Aguila.", "OK"); return; }

        const string carpeta = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder("Assets", "Animations");

        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Images/eagle-attack.png")
            .OfType<Sprite>().OrderBy(s => IndiceDe(s.name)).ToArray();
        if (sprites.Length == 0) { EditorUtility.DisplayDialog("Error", "eagle-attack.png no esta sliceado.", "OK"); return; }

        var clip = new AnimationClip { frameRate = 10 };
        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        var binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * 0.1f, value = sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        string cp = $"{carpeta}/aguila_vuela.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(cp) != null) AssetDatabase.DeleteAsset(cp);
        AssetDatabase.CreateAsset(clip, cp);

        string ctp = $"{carpeta}/Aguila.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctp) != null) AssetDatabase.DeleteAsset(ctp);
        var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(ctp, clip);

        var anim = aguila.GetComponent<Animator>();
        if (anim == null) anim = aguila.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        AssetDatabase.SaveAssets();
        Debug.Log("[AguilaTools] Animator Vuelo listo.");
    }

    // =======================================================================
    // PASO 7 - Prefabs Gema y Cereza
    // =======================================================================
    [MenuItem(PASOS + "7. Crear Prefabs Gema y Cereza", priority = 160)]
    public static void CrearPrefabsGemaYCereza()
    {
        const string carpeta = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder("Assets", "Prefabs");

        AsegurarTag("Gema");
        AsegurarTag("Cereza");

        CrearPrefabColectable("Gema", "Assets/Images/gem.png", carpeta + "/Gema.prefab");
        CrearPrefabColectable("Cereza", "Assets/Images/cherry.png", carpeta + "/Cereza.prefab");

        Debug.Log("[AguilaTools] Prefabs 'Gema' y 'Cereza' creados con tags.");
    }

    static void CrearPrefabColectable(string nombre, string sheetPath, string prefabPath)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            AssetDatabase.DeleteAsset(prefabPath);

        var g = new GameObject(nombre);
        g.tag = nombre;
        var sr = g.AddComponent<SpriteRenderer>();
        var spr = CargarSpriteDeSheet(sheetPath, 0);
        if (spr != null) sr.sprite = spr;
        sr.sortingOrder = 3;
        var col = g.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        PrefabUtility.SaveAsPrefabAsset(g, prefabPath);
        Object.DestroyImmediate(g);
    }

    // =======================================================================
    // PASO 8 - Colocar 8 Gemas + 5 Cerezas distribuidas a lo largo del mapa
    // =======================================================================
    [MenuItem(PASOS + "8. Colocar 8 Gemas + 5 Cerezas", priority = 170)]
    public static void ColocarGemasYCerezas()
    {
        var prefabGema = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gema.prefab");
        var prefabCereza = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cereza.prefab");
        if (prefabGema == null || prefabCereza == null)
        {
            EditorUtility.DisplayDialog("Faltan prefabs", "Primero ejecuta '7. Crear Prefabs'.", "OK");
            return;
        }

        foreach (var g in GameObject.FindGameObjectsWithTag("Gema")) Object.DestroyImmediate(g);
        foreach (var g in GameObject.FindGameObjectsWithTag("Cereza")) Object.DestroyImmediate(g);

        var padreGemas = GameObject.Find("Gemas");
        if (padreGemas != null) Object.DestroyImmediate(padreGemas);
        padreGemas = new GameObject("Gemas");

        var padreCerezas = GameObject.Find("Cerezas");
        if (padreCerezas != null) Object.DestroyImmediate(padreCerezas);
        padreCerezas = new GameObject("Cerezas");

        // 8 gemas distribuidas DENTRO del area del aguila: x=[-13,13], y=[-3, 6]
        Vector2[] gemas = {
            new Vector2(-13, 3), new Vector2(-10, 6), new Vector2(-7, 2), new Vector2(-3, 5),
            new Vector2(2, 3),   new Vector2(6, 6),   new Vector2(10, 1), new Vector2(13, 4)
        };
        foreach (var pos in gemas)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabGema);
            inst.transform.position = pos;
            inst.transform.SetParent(padreGemas.transform);
        }

        // 5 cerezas distribuidas DENTRO del area del aguila
        Vector2[] cerezas = {
            new Vector2(-11, 5), new Vector2(-5, 2), new Vector2(0, 6),
            new Vector2(5, 4),   new Vector2(11, 1)
        };
        foreach (var pos in cerezas)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabCereza);
            inst.transform.position = pos;
            inst.transform.SetParent(padreCerezas.transform);
        }

        Debug.Log("[AguilaTools] 8 gemas + 5 cerezas distribuidas a lo largo del mapa.");
    }

    // =======================================================================
    // PASO 9 - Configurar camara (fondo cielo + sigue al Aguila)
    // =======================================================================
    [MenuItem(PASOS + "9. Camara sigue al Aguila (fondo cielo)", priority = 180)]
    public static void ConfigurarCamara()
    {
        var aguila = GameObject.Find("Aguila");
        if (aguila == null) { EditorUtility.DisplayDialog("Falta Aguila", "Primero crea el Aguila.", "OK"); return; }
        var cam = Camera.main;
        if (cam == null) { Debug.LogError("[AguilaTools] No existe Main Camera."); return; }

        // Quitar CinemachineBrain si quedo de versiones anteriores
        var brain = cam.GetComponent("Unity.Cinemachine.CinemachineBrain")
                 ?? cam.GetComponent("Cinemachine.CinemachineBrain");
        if (brain != null) Object.DestroyImmediate(brain);

        cam.transform.position = new Vector3(0, 2, -10);
        cam.orthographic = true;
        cam.orthographicSize = 6;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f); // azul cielo

        var sigue = cam.GetComponent<CamaraSigue>();
        if (sigue == null) sigue = cam.gameObject.AddComponent<CamaraSigue>();
        sigue.objetivo = aguila.transform;

        Debug.Log("[AguilaTools] Camara: fondo cielo + sigue al Aguila.");
    }

    // =======================================================================
    // PASO 10 - Guardar como Nivel1
    // =======================================================================
    [MenuItem(PASOS + "10. Guardar escena como Nivel1", priority = 190)]
    public static void GuardarComoNivel1()
    {
        const string carpeta = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder("Assets", "Scenes");

        var escena = EditorSceneManager.GetActiveScene();
        string nuevoPath = $"{carpeta}/Nivel1.unity";

        EditorSceneManager.SaveScene(escena, nuevoPath);

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>($"{carpeta}/SampleScene.unity") != null
            && escena.path != $"{carpeta}/SampleScene.unity")
            AssetDatabase.DeleteAsset($"{carpeta}/SampleScene.unity");

        Debug.Log($"[AguilaTools] Escena guardada como {nuevoPath}.");
    }

    // =======================================================================
    // Helpers
    // =======================================================================
    static void AsegurarTag(string tag)
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.arraySize++;
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tm.ApplyModifiedProperties();
    }
}
#endif
