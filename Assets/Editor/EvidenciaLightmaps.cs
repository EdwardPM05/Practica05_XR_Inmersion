using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Abre la escena y la ventana Lighting, y exporta los lightmaps a PNG en Evidencias/.
// Se dispara desde el menu Practica05 o creando el archivo "evidencia.trigger" en la raiz del proyecto.
[InitializeOnLoad]
public static class EvidenciaLightmaps
{
    const string Trigger = "evidencia.trigger";

    static EvidenciaLightmaps()
    {
        EditorApplication.update += Poll;
    }

    static void Poll()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists("build.trigger"))
        {
            File.Delete("build.trigger");
            BuildPractica05.BuildAndBakeBatch();
            return;
        }
        if (File.Exists("play.trigger"))
        {
            File.Delete("play.trigger");
            MedirFPS();
            return;
        }
        if (!File.Exists(Trigger)) return;
        File.Delete(Trigger);
        Capturar();
    }

    [MenuItem("Practica05/4. Medir FPS (Play 15 s)")]
    public static void MedirFPS()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practica05.unity");
        File.WriteAllText("play.autostop", "1"); // el FPSCounter lo lee y detiene el Play a los 15 s
        if (File.Exists("fps_report.txt")) File.Delete("fps_report.txt");
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Practica05/3. Capturar evidencia de lightmaps")]
    public static void Capturar()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practica05.unity");
        Directory.CreateDirectory("Evidencias");

        var maps = LightmapSettings.lightmaps;
        for (int i = 0; i < maps.Length; i++)
        {
            var tex = maps[i].lightmapColor;
            if (tex == null) continue;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(tex, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var png = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            png.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            png.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes("Evidencias/Lightmap-" + i + ".png", png.EncodeToPNG());
            Object.DestroyImmediate(png);
        }

        foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            var t = w.titleContent.text;
            if (t == "Package Manager" || t == "Project Settings") w.Close();
        }
        EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting");
        var lighting = EditorWindow.focusedWindow;
        if (lighting != null && lighting.titleContent.text.Contains("Lighting"))
            lighting.position = new Rect(300, 120, 900, 760);
        File.WriteAllText("Evidencias/estado.txt", "lightmaps=" + maps.Length + " escena=" + EditorSceneManager.GetActiveScene().name);
        Debug.Log("[Practica05] Evidencia exportada: " + maps.Length + " lightmaps");
    }
}
