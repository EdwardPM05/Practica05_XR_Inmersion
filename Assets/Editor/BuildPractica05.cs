using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Debug = UnityEngine.Debug;
using Random = System.Random;

// Construye la sala de estar de la Practica 05 (luz Baked + audio 3D) y hornea la iluminacion.
public static class BuildPractica05
{
    const string ScenePath = "Assets/Scenes/Practica05.unity";
    const string XrVersionDir = "Assets/Samples/XR Interaction Toolkit/3.2.1";

    static Transform _root;
    static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();

    // ------------------------------------------------------------------ escena

    [MenuItem("Practica05/1. Construir escena")]
    public static void BuildScene()
    {
        SetupUrp();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateMaterials();
        _root = new GameObject("Entorno").transform;

        BuildShell();
        BuildOutdoors();
        BuildFurniture();
        BuildDecor();
        BuildLights();

        foreach (var t in _root.GetComponentsInChildren<Transform>())
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, (StaticEditorFlags)int.MaxValue);

        BuildAudioObject();
        BuildProbesAndReverb();

        var xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrVersionDir + "/Starter Assets/Prefabs/XR Origin (XR Rig).prefab");
        var xr = (GameObject)PrefabUtility.InstantiatePrefab(xrPrefab);
        xr.transform.position = new Vector3(0, 0, -3f);
        var simPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrVersionDir + "/XR Device Simulator/XR Device Simulator.prefab");
        PrefabUtility.InstantiatePrefab(simPrefab);
        new GameObject("FPS_Counter").AddComponent<FPSCounter>();

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("[Practica05] Escena construida: " + ScenePath);
    }

    // Sala 10 x 8 m, altura 3.2 m, con ventana en la pared norte.
    static void BuildShell()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Setup(floor, "Piso_Parquet", _root, new Vector3(0, 0, 0), new Vector3(1f, 1, 0.8f), "Parquet");

        Box("Techo", new Vector3(0, 3.3f, 0), new Vector3(10.2f, 0.2f, 8.2f), "Techo");

        // pared sur y paredes laterales
        Box("Pared_Sur", new Vector3(0, 1.6f, -4f), new Vector3(10.2f, 3.2f, 0.2f), "Pared");
        Box("Pared_Este", new Vector3(5f, 1.6f, 0), new Vector3(0.2f, 3.2f, 7.8f), "Pared");
        Box("Pared_Oeste", new Vector3(-5f, 1.6f, 0), new Vector3(0.2f, 3.2f, 7.8f), "Pared");

        // pared norte con ventana (x -1.5..1.5, y 1.0..2.4)
        Box("Pared_Norte_Izq", new Vector3(-3.3f, 1.6f, 4f), new Vector3(3.6f, 3.2f, 0.2f), "Pared");
        Box("Pared_Norte_Der", new Vector3(3.3f, 1.6f, 4f), new Vector3(3.6f, 3.2f, 0.2f), "Pared");
        Box("Pared_Norte_Baja", new Vector3(0, 0.5f, 4f), new Vector3(3f, 1f, 0.2f), "Pared");
        Box("Pared_Norte_Alta", new Vector3(0, 2.8f, 4f), new Vector3(3f, 0.8f, 0.2f), "Pared");
        Box("Ventana_Repisa", new Vector3(0, 1.03f, 3.85f), new Vector3(3.2f, 0.06f, 0.4f), "Zocalo");
        Box("Ventana_Marco_Izq", new Vector3(-1.53f, 1.7f, 3.9f), new Vector3(0.06f, 1.4f, 0.24f), "Zocalo");
        Box("Ventana_Marco_Der", new Vector3(1.53f, 1.7f, 3.9f), new Vector3(0.06f, 1.4f, 0.24f), "Zocalo");
        Box("Ventana_Marco_Sup", new Vector3(0, 2.43f, 3.9f), new Vector3(3.12f, 0.06f, 0.24f), "Zocalo");
        Box("Ventana_Travesano", new Vector3(0, 1.7f, 3.95f), new Vector3(0.05f, 1.4f, 0.08f), "Zocalo");

        // zocalo inferior y friso (lambrin) en las paredes
        Box("Zocalo_Sur", new Vector3(0, 0.07f, -3.88f), new Vector3(9.8f, 0.14f, 0.05f), "Zocalo");
        Box("Zocalo_Norte", new Vector3(0, 0.07f, 3.88f), new Vector3(9.8f, 0.14f, 0.05f), "Zocalo");
        Box("Zocalo_Este", new Vector3(4.88f, 0.07f, 0), new Vector3(0.05f, 0.14f, 7.8f), "Zocalo");
        Box("Zocalo_Oeste", new Vector3(-4.88f, 0.07f, 0), new Vector3(0.05f, 0.14f, 7.8f), "Zocalo");
        Box("Friso_Sur", new Vector3(0, 0.55f, -3.9f), new Vector3(9.8f, 0.9f, 0.04f), "Friso");
        Box("Friso_Norte", new Vector3(0, 0.5f, 3.9f), new Vector3(9.8f, 0.8f, 0.04f), "Friso");
        Box("Friso_Este", new Vector3(4.9f, 0.55f, 0), new Vector3(0.04f, 0.9f, 7.8f), "Friso");
        Box("Friso_Oeste", new Vector3(-4.9f, 0.55f, 0), new Vector3(0.04f, 0.9f, 7.8f), "Friso");
        Box("Moldura_Sur", new Vector3(0, 1.02f, -3.88f), new Vector3(9.8f, 0.05f, 0.07f), "Zocalo");
        Box("Moldura_Este", new Vector3(4.88f, 1.02f, 0), new Vector3(0.07f, 0.05f, 7.8f), "Zocalo");
        Box("Moldura_Oeste", new Vector3(-4.88f, 1.02f, 0), new Vector3(0.07f, 0.05f, 7.8f), "Zocalo");
    }

    static void BuildOutdoors()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Setup(ground, "Jardin_Suelo", _root, new Vector3(0, -0.03f, 40f), new Vector3(10f, 1, 8f), "Pasto");

        var rnd = new Random(11);
        for (int i = 0; i < 6; i++)
        {
            float x = -12f + i * 5f + (float)rnd.NextDouble() * 2f;
            float z = 10f + (float)rnd.NextDouble() * 6f;
            float h = 3f + (float)rnd.NextDouble() * 2f;
            Cyl("Arbol_Tronco_" + i, new Vector3(x, h * 0.5f, z), new Vector3(0.45f, h * 0.5f, 0.45f), "Madera_Oscura");
            Sphere("Arbol_Copa_" + i, new Vector3(x, h + 1.2f, z), Vector3.one * (2.6f + (float)rnd.NextDouble()), "Hoja");
        }
    }

    static void BuildFurniture()
    {
        // alfombra
        Box("Alfombra", new Vector3(-0.8f, 0.012f, 0.4f), new Vector3(4.6f, 0.025f, 3.4f), "Alfombra");
        Box("Alfombra_Borde", new Vector3(-0.8f, 0.01f, 0.4f), new Vector3(4.8f, 0.02f, 3.6f), "Alfombra_Borde");

        // sofa contra la pared oeste
        Box("Sofa_Base", new Vector3(-4.1f, 0.28f, 0.5f), new Vector3(1.0f, 0.42f, 2.5f), "Tela_Sofa");
        Box("Sofa_Respaldo", new Vector3(-4.65f, 0.72f, 0.5f), new Vector3(0.28f, 0.9f, 2.5f), "Tela_Sofa");
        Box("Sofa_Brazo_A", new Vector3(-4.1f, 0.42f, -0.85f), new Vector3(1.0f, 0.65f, 0.28f), "Tela_Sofa");
        Box("Sofa_Brazo_B", new Vector3(-4.1f, 0.42f, 1.85f), new Vector3(1.0f, 0.65f, 0.28f), "Tela_Sofa");
        Box("Sofa_Cojin_1", new Vector3(-3.95f, 0.55f, 0.0f), new Vector3(0.75f, 0.14f, 0.85f), "Cojin_Mostaza");
        Box("Sofa_Cojin_2", new Vector3(-3.95f, 0.55f, 1.0f), new Vector3(0.75f, 0.14f, 0.85f), "Cojin_Mostaza");
        Box("Cojin_Decor_1", new Vector3(-4.35f, 0.8f, -0.25f), new Vector3(0.14f, 0.38f, 0.38f), "Cojin_Coral");
        Box("Cojin_Decor_2", new Vector3(-4.35f, 0.8f, 1.25f), new Vector3(0.14f, 0.38f, 0.38f), "Cojin_Coral");

        // sillon
        Box("Sillon_Base", new Vector3(1.6f, 0.26f, 0.5f), new Vector3(0.95f, 0.4f, 0.95f), "Tela_Sillon");
        Box("Sillon_Respaldo", new Vector3(2.0f, 0.65f, 0.5f), new Vector3(0.18f, 0.85f, 0.95f), "Tela_Sillon");
        Box("Sillon_Brazo_A", new Vector3(1.6f, 0.4f, 0.0f), new Vector3(0.95f, 0.5f, 0.15f), "Tela_Sillon");
        Box("Sillon_Brazo_B", new Vector3(1.6f, 0.4f, 1.0f), new Vector3(0.95f, 0.5f, 0.15f), "Tela_Sillon");

        // mesa de centro
        Box("Mesa_Tapa", new Vector3(-1.9f, 0.42f, 0.5f), new Vector3(1.4f, 0.06f, 0.8f), "Madera");
        foreach (var dx in new[] { -0.6f, 0.6f })
            foreach (var dz in new[] { -0.32f, 0.32f })
                Cyl("Mesa_Pata", new Vector3(-1.9f + dx, 0.2f, 0.5f + dz), new Vector3(0.06f, 0.2f, 0.06f), "Metal");
        Cyl("Mesa_Jarron", new Vector3(-1.9f, 0.56f, 0.5f), new Vector3(0.14f, 0.12f, 0.14f), "Ceramica");
        Sphere("Mesa_Flor_1", new Vector3(-1.9f, 0.76f, 0.5f), Vector3.one * 0.14f, "Flor");
        Sphere("Mesa_Flor_2", new Vector3(-1.84f, 0.72f, 0.54f), Vector3.one * 0.11f, "Cojin_Coral");
        Box("Mesa_Libro", new Vector3(-2.2f, 0.47f, 0.35f), new Vector3(0.3f, 0.05f, 0.22f), "Libro_Azul");

        // aparador (sobre el que va la radio) contra la pared este
        Box("Aparador", new Vector3(4.5f, 0.42f, 1.0f), new Vector3(0.75f, 0.84f, 2.2f), "Madera");
        Box("Aparador_Tapa", new Vector3(4.5f, 0.86f, 1.0f), new Vector3(0.8f, 0.04f, 2.3f), "Madera_Oscura");
        Box("Aparador_Puerta_A", new Vector3(4.12f, 0.42f, 0.5f), new Vector3(0.02f, 0.7f, 0.9f), "Madera_Oscura");
        Box("Aparador_Puerta_B", new Vector3(4.12f, 0.42f, 1.5f), new Vector3(0.02f, 0.7f, 0.9f), "Madera_Oscura");

        // estanteria con libros contra la pared oeste (norte del sofa lado sur)
        float sx = -4.72f, sz = -2.5f;
        Box("Estante_Fondo", new Vector3(-4.88f, 1.0f, sz), new Vector3(0.03f, 2.0f, 1.64f), "Madera_Oscura");
        Box("Estante_Lado_A", new Vector3(sx, 1.0f, sz - 0.8f), new Vector3(0.36f, 2.0f, 0.04f), "Madera_Oscura");
        Box("Estante_Lado_B", new Vector3(sx, 1.0f, sz + 0.8f), new Vector3(0.36f, 2.0f, 0.04f), "Madera_Oscura");
        float[] levels = { 0.03f, 0.45f, 0.87f, 1.29f, 1.71f, 1.99f };
        foreach (var y in levels)
            Box("Estante_Tabla", new Vector3(sx, y, sz), new Vector3(0.36f, 0.03f, 1.64f), "Madera_Oscura");
        var rnd = new Random(3);
        string[] bookMats = { "Libro_Rojo", "Libro_Azul", "Libro_Verde", "Libro_Crema", "Libro_Mostaza" };
        for (int s = 0; s < 4; s++)
        {
            float z = sz - 0.72f;
            while (z < sz + 0.7f)
            {
                float w = 0.04f + (float)rnd.NextDouble() * 0.05f;
                float h = 0.22f + (float)rnd.NextDouble() * 0.14f;
                Box("Libro", new Vector3(sx, levels[s] + 0.015f + h * 0.5f, z + w * 0.5f), new Vector3(0.24f, h, w), bookMats[rnd.Next(bookMats.Length)]);
                z += w + 0.004f;
            }
        }

        // lampara de pie junto al sofa
        Cyl("Lampara_Base", new Vector3(-4.35f, 0.03f, 2.9f), new Vector3(0.28f, 0.03f, 0.28f), "Metal");
        Cyl("Lampara_Poste", new Vector3(-4.35f, 0.85f, 2.9f), new Vector3(0.04f, 0.85f, 0.04f), "Metal");
        Cyl("Lampara_Pantalla", new Vector3(-4.35f, 1.75f, 2.9f), new Vector3(0.42f, 0.2f, 0.42f), "Luz_Calida");

        // lampara colgante
        Cyl("Colgante_Cable", new Vector3(0, 3.05f, 0), new Vector3(0.015f, 0.2f, 0.015f), "Metal");
        Sphere("Colgante_Bombilla", new Vector3(0, 2.75f, 0), Vector3.one * 0.32f, "Luz_Calida");
        Cyl("Colgante_Rosetón", new Vector3(0, 3.19f, 0), new Vector3(0.22f, 0.02f, 0.22f), "Metal");

        // macetas con plantas
        Plant(new Vector3(4.2f, 0, -3.2f), 1.0f);
        Plant(new Vector3(-4.3f, 0, 3.4f), 0.8f);
        Plant(new Vector3(3.6f, 0, 3.4f), 1.2f);
    }

    static void Plant(Vector3 p, float s)
    {
        Cyl("Maceta", p + new Vector3(0, 0.2f * s, 0), new Vector3(0.4f * s, 0.2f * s, 0.4f * s), "Terracota");
        Cyl("Planta_Tallo", p + new Vector3(0, 0.65f * s, 0), new Vector3(0.04f * s, 0.35f * s, 0.04f * s), "Hoja");
        Sphere("Planta_Hoja_1", p + new Vector3(0, 1.1f * s, 0), Vector3.one * 0.55f * s, "Hoja");
        Sphere("Planta_Hoja_2", p + new Vector3(0.22f * s, 0.9f * s, 0.1f * s), Vector3.one * 0.38f * s, "Hoja");
        Sphere("Planta_Hoja_3", p + new Vector3(-0.2f * s, 0.85f * s, -0.12f * s), Vector3.one * 0.42f * s, "Hoja");
    }

    static void BuildDecor()
    {
        // cuadros sobre el aparador (pared este) y en la pared sur
        Frame(new Vector3(4.85f, 1.8f, 0.2f), Quaternion.Euler(0, 90, 0), new Vector3(0.7f, 0.9f, 1f), "Lienzo_Azul");
        Frame(new Vector3(4.85f, 1.9f, 1.2f), Quaternion.Euler(0, 90, 0), new Vector3(0.9f, 0.6f, 1f), "Lienzo_Naranja");
        Frame(new Vector3(4.85f, 1.7f, 2.1f), Quaternion.Euler(0, 90, 0), new Vector3(0.5f, 0.7f, 1f), "Lienzo_Verde");
        Frame(new Vector3(-2.2f, 1.9f, -3.85f), Quaternion.identity, new Vector3(1.2f, 0.8f, 1f), "Lienzo_Naranja");
        Frame(new Vector3(0.4f, 1.9f, -3.85f), Quaternion.identity, new Vector3(0.7f, 0.9f, 1f), "Lienzo_Azul");
        Frame(new Vector3(2.2f, 1.9f, -3.85f), Quaternion.identity, new Vector3(1.0f, 0.7f, 1f), "Lienzo_Verde");

        // libros apilados y objetos sobre el aparador
        Box("Aparador_Libros_1", new Vector3(4.55f, 0.92f, 1.9f), new Vector3(0.3f, 0.06f, 0.22f), "Libro_Rojo");
        Box("Aparador_Libros_2", new Vector3(4.55f, 0.98f, 1.9f), new Vector3(0.26f, 0.06f, 0.2f), "Libro_Crema");
        Cyl("Aparador_Jarron", new Vector3(4.55f, 1.0f, 0.2f), new Vector3(0.16f, 0.14f, 0.16f), "Ceramica");
    }

    static void Frame(Vector3 pos, Quaternion rot, Vector3 size, string canvasMat)
    {
        var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Setup(frame, "Cuadro_Marco", _root, pos, new Vector3(size.x + 0.1f, size.y + 0.1f, 0.05f), "Madera_Oscura");
        frame.transform.rotation = rot;
        var canvas = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Setup(canvas, "Cuadro_Lienzo", _root, pos, new Vector3(size.x, size.y, 0.06f), canvasMat);
        canvas.transform.rotation = rot;
        canvas.transform.position = pos + rot * new Vector3(0, 0, -0.01f);
    }

    static void BuildLights()
    {
        var sun = new GameObject("Directional Light (Sol)");
        var sl = sun.AddComponent<Light>();
        sl.type = LightType.Directional;
        sl.lightmapBakeType = LightmapBakeType.Baked;
        sl.intensity = 2.5f;
        sl.color = new Color(1f, 0.95f, 0.85f);
        sl.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(35, 190, 0); // entra por la ventana norte hacia el sur

        PointLight("Luz_Colgante", new Vector3(0, 2.7f, 0), new Color(1f, 0.9f, 0.72f), 3f, 9f);
        PointLight("Luz_Lampara_Pie", new Vector3(-4.35f, 1.7f, 2.9f), new Color(1f, 0.72f, 0.42f), 2.5f, 6f);
        PointLight("Luz_Radio", new Vector3(4.3f, 1.35f, 1.0f), new Color(1f, 0.65f, 0.3f), 1.2f, 3.5f);
    }

    static void PointLight(string name, Vector3 pos, Color c, float intensity, float range)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_root);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.lightmapBakeType = LightmapBakeType.Baked;
        l.color = c;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.Soft;
    }

    // Audio_Object: una radio sobre el aparador. NO es estatico.
    static void BuildAudioObject()
    {
        var audio = new GameObject("Audio_Object");
        audio.transform.position = new Vector3(4.55f, 1.0f, 1.0f);

        void Part(GameObject g, string n, Vector3 pos, Vector3 scale, string mat, Quaternion? rot = null)
        {
            g.name = n;
            g.transform.SetParent(audio.transform);
            g.transform.position = pos;
            g.transform.localScale = scale;
            if (rot.HasValue) g.transform.rotation = rot.Value;
            g.GetComponent<Renderer>().sharedMaterial = Mats[mat];
        }
        Part(GameObject.CreatePrimitive(PrimitiveType.Cube), "Radio_Cuerpo", new Vector3(4.55f, 1.01f, 1.0f), new Vector3(0.28f, 0.3f, 0.6f), "Radio_Cuerpo");
        Part(GameObject.CreatePrimitive(PrimitiveType.Cube), "Radio_Altavoz", new Vector3(4.405f, 1.01f, 0.83f), new Vector3(0.02f, 0.22f, 0.3f), "Metal");
        Part(GameObject.CreatePrimitive(PrimitiveType.Cube), "Radio_Dial", new Vector3(4.405f, 1.06f, 1.2f), new Vector3(0.02f, 0.08f, 0.14f), "Luz_Calida");
        Part(GameObject.CreatePrimitive(PrimitiveType.Cylinder), "Radio_Perilla_1", new Vector3(4.4f, 0.95f, 1.12f), new Vector3(0.05f, 0.015f, 0.05f), "Metal", Quaternion.Euler(0, 0, 90));
        Part(GameObject.CreatePrimitive(PrimitiveType.Cylinder), "Radio_Perilla_2", new Vector3(4.4f, 0.95f, 1.26f), new Vector3(0.05f, 0.015f, 0.05f), "Metal", Quaternion.Euler(0, 0, 90));
        Part(GameObject.CreatePrimitive(PrimitiveType.Cylinder), "Radio_Antena", new Vector3(4.6f, 1.3f, 1.2f), new Vector3(0.01f, 0.18f, 0.01f), "Metal", Quaternion.Euler(0, 0, -12));

        var src = audio.AddComponent<AudioSource>();
        src.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/campana.wav");
        src.loop = true;
        src.playOnAwake = true;
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = 1f;
        src.maxDistance = 15f;
        src.spatialize = true;
    }

    static void BuildProbesAndReverb()
    {
        // light probes para que los objetos no estaticos (radio, XR) reciban la luz horneada
        var go = new GameObject("Light_Probes");
        var group = go.AddComponent<LightProbeGroup>();
        var pts = new List<Vector3>();
        for (float x = -4f; x <= 4f; x += 2f)
            for (float z = -3f; z <= 3f; z += 2f)
                foreach (var y in new[] { 0.5f, 1.5f, 2.5f })
                    pts.Add(new Vector3(x, y, z));
        group.probePositions = pts.ToArray();

        var rz = new GameObject("Reverb_Sala").AddComponent<AudioReverbZone>();
        rz.transform.position = new Vector3(0, 1.5f, 0);
        rz.minDistance = 8f;
        rz.maxDistance = 12f;
        rz.reverbPreset = AudioReverbPreset.Room;
    }

    // ------------------------------------------------------------------ iluminacion

    [MenuItem("Practica05/2. Hornear luz (Bake)")]
    public static void Bake()
    {
        EditorSceneManager.OpenScene(ScenePath);
        const string lsPath = "Assets/Settings/Practica05_Lighting.lighting";
        var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(lsPath);
        if (ls == null)
        {
            Directory.CreateDirectory("Assets/Settings");
            ls = new LightingSettings();
            AssetDatabase.CreateAsset(ls, lsPath);
        }
        ls.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
        ls.bakedGI = true;
        ls.realtimeGI = false;
        ls.lightmapResolution = 40;
        ls.lightmapMaxSize = 1024;
        ls.directSampleCount = 64;
        ls.indirectSampleCount = 256;
        ls.environmentSampleCount = 128;
        ls.maxBounces = 3;
        EditorUtility.SetDirty(ls);
        Lightmapping.lightingSettings = ls;

        var sw = Stopwatch.StartNew();
        bool ok = Lightmapping.Bake();
        sw.Stop();
        EditorSceneManager.SaveOpenScenes();
        string msg = "Bake ok=" + ok + " lightmapper=" + ls.lightmapper + " resolucion=" + ls.lightmapResolution
                     + " tiempo=" + sw.Elapsed + " lightmaps=" + LightmapSettings.lightmaps.Length;
        File.WriteAllText("bake_report.txt", msg);
        Debug.Log("[Practica05] " + msg);
    }

    public static void BuildAndBakeBatch()
    {
        BuildScene();
        Bake();
    }

    static void SetupUrp()
    {
        Directory.CreateDirectory("Assets/Settings");
        const string rp = "Assets/Settings/URP_Practica05.asset";
        var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(rp);
        if (asset == null)
        {
            var data = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(data, "Assets/Settings/URP_Practica05_Renderer.asset");
            asset = UniversalRenderPipelineAsset.Create(data);
            AssetDatabase.CreateAsset(asset, rp);
        }
        GraphicsSettings.defaultRenderPipeline = asset;
        QualitySettings.renderPipeline = asset;
        AssetDatabase.SaveAssets();
    }

    // ------------------------------------------------------------------ materiales

    static void CreateMaterials()
    {
        Directory.CreateDirectory("Assets/Materials");
        Mats.Clear();
        var wood = LoadOrMakeWoodTexture();

        M("Parquet", Color.white, 0.35f, 0f, null, wood, new Vector2(5f, 4f));
        M("Pared", new Color(0.93f, 0.89f, 0.80f), 0.1f);
        M("Techo", new Color(0.96f, 0.95f, 0.92f), 0.05f);
        M("Friso", new Color(0.20f, 0.36f, 0.38f), 0.25f);
        M("Zocalo", new Color(0.97f, 0.96f, 0.93f), 0.3f);
        M("Alfombra", new Color(0.62f, 0.16f, 0.14f), 0.05f);
        M("Alfombra_Borde", new Color(0.88f, 0.78f, 0.55f), 0.05f);
        M("Tela_Sofa", new Color(0.18f, 0.43f, 0.46f), 0.1f);
        M("Tela_Sillon", new Color(0.78f, 0.45f, 0.22f), 0.1f);
        M("Cojin_Mostaza", new Color(0.88f, 0.67f, 0.18f), 0.1f);
        M("Cojin_Coral", new Color(0.92f, 0.45f, 0.38f), 0.1f);
        M("Madera", new Color(0.52f, 0.32f, 0.18f), 0.4f);
        M("Madera_Oscura", new Color(0.26f, 0.15f, 0.09f), 0.45f);
        M("Metal", new Color(0.15f, 0.15f, 0.16f), 0.75f, 0.9f);
        M("Ceramica", new Color(0.92f, 0.92f, 0.88f), 0.8f);
        M("Terracota", new Color(0.72f, 0.36f, 0.22f), 0.2f);
        M("Hoja", new Color(0.22f, 0.5f, 0.2f), 0.2f);
        M("Flor", new Color(0.95f, 0.85f, 0.3f), 0.2f);
        M("Pasto", new Color(0.3f, 0.55f, 0.25f), 0.05f);
        M("Libro_Rojo", new Color(0.65f, 0.15f, 0.15f), 0.2f);
        M("Libro_Azul", new Color(0.15f, 0.25f, 0.55f), 0.2f);
        M("Libro_Verde", new Color(0.18f, 0.4f, 0.25f), 0.2f);
        M("Libro_Crema", new Color(0.9f, 0.85f, 0.7f), 0.2f);
        M("Libro_Mostaza", new Color(0.8f, 0.6f, 0.15f), 0.2f);
        M("Lienzo_Azul", new Color(0.2f, 0.4f, 0.75f), 0.1f);
        M("Lienzo_Naranja", new Color(0.95f, 0.55f, 0.15f), 0.1f);
        M("Lienzo_Verde", new Color(0.3f, 0.65f, 0.4f), 0.1f);
        M("Radio_Cuerpo", new Color(0.55f, 0.12f, 0.1f), 0.5f);
        M("Luz_Calida", new Color(1f, 0.85f, 0.6f), 0.3f, 0f, new Color(1f, 0.7f, 0.35f) * 2.5f);
    }

    static void M(string name, Color c, float smooth = 0.3f, float metal = 0f, Color? emission = null, Texture2D tex = null, Vector2? tiling = null)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", metal);
        if (tex != null)
        {
            m.SetTexture("_BaseMap", tex);
            if (tiling.HasValue) m.SetTextureScale("_BaseMap", tiling.Value);
        }
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        }
        AssetDatabase.CreateAsset(m, "Assets/Materials/" + name + ".mat");
        Mats[name] = m;
    }

    // Textura de parquet generada por codigo (tablones con vetas y juntas).
    static Texture2D LoadOrMakeWoodTexture()
    {
        const string path = "Assets/Materials/Parquet.png";
        const int s = 512, planks = 8;
        var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
        var rnd = new Random(5);
        int ph = s / planks;
        var dark = new Color(0.34f, 0.2f, 0.1f);
        var light = new Color(0.66f, 0.43f, 0.24f);
        for (int row = 0; row < planks; row++)
        {
            float b = 0.35f + (float)rnd.NextDouble() * 0.45f;
            int joint = rnd.Next(s);
            for (int y = row * ph; y < (row + 1) * ph; y++)
                for (int x = 0; x < s; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.015f + row * 7f, y * 0.4f) * 0.25f
                                + Mathf.PerlinNoise(x * 0.12f, y * 0.04f + row) * 0.08f;
                    var col = Color.Lerp(dark, light, Mathf.Clamp01(b + grain));
                    int dy = y - row * ph;
                    if (dy < 2 || dy > ph - 2) col *= 0.55f;
                    if (Mathf.Abs(x - joint) < 2) col *= 0.55f;
                    t.SetPixel(x, y, col);
                }
        }
        t.Apply();
        File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ------------------------------------------------------------------ helpers

    static GameObject Setup(GameObject g, string name, Transform parent, Vector3 pos, Vector3 scale, string mat)
    {
        g.name = name;
        g.transform.SetParent(parent);
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mats[mat];
        return g;
    }

    static void Box(string n, Vector3 pos, Vector3 scale, string mat) =>
        Setup(GameObject.CreatePrimitive(PrimitiveType.Cube), n, _root, pos, scale, mat);

    // scale = (diametro x, MITAD de la altura, diametro z)
    static void Cyl(string n, Vector3 pos, Vector3 scale, string mat) =>
        Setup(GameObject.CreatePrimitive(PrimitiveType.Cylinder), n, _root, pos, scale, mat);

    static void Sphere(string n, Vector3 pos, Vector3 scale, string mat) =>
        Setup(GameObject.CreatePrimitive(PrimitiveType.Sphere), n, _root, pos, scale, mat);
}
