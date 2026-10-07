using System.IO;
using UnityEngine;

// Muestra FPS actuales y promedio en pantalla para el reporte de la práctica.
// Solo en el Editor: si existe el archivo "play.autostop", mide 15 s (tras calentamiento),
// guarda fps_report.txt y detiene el Play. En uso normal nunca detiene la ejecución.
public class FPSCounter : MonoBehaviour
{
    const float AutoStopSeconds = 15f;
    const float WarmupSeconds = 3f;

    float _sum, _min = float.MaxValue, _max;
    int _frames;
    float _current;
    float _t;
    bool _autoStop;

    void Start()
    {
#if UNITY_EDITOR
        _autoStop = File.Exists("play.autostop");
#endif
    }

    void Update()
    {
        _t += Time.unscaledDeltaTime;
        _current = 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-5f);
        if (_t > WarmupSeconds)
        {
            _sum += _current;
            _frames++;
            if (_current < _min) _min = _current;
            if (_current > _max) _max = _current;
        }
        if (_autoStop && _t >= AutoStopSeconds + WarmupSeconds)
            Finish();
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        GUI.Label(new Rect(10, 10, 420, 30), $"FPS: {_current:0}  |  Promedio: {_sum / Mathf.Max(_frames, 1):0.0}", style);
    }

    void Finish()
    {
        _autoStop = false;
        if (_frames == 0) return;
        string msg = $"FPS promedio={_sum / _frames:0.0} min={_min:0.0} max={_max:0.0} frames={_frames} duracion={_t - WarmupSeconds:0.0}s";
        Debug.Log("[FPSCounter] " + msg);
#if UNITY_EDITOR
        File.WriteAllText("fps_report.txt", msg);
        File.Delete("play.autostop");
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
