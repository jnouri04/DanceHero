using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


/// <summary>
/// use this to make charts,
/// r to record
/// d f j k at to create notes
/// return to stop and save
/// </summary>
public class ChartRecorder : MonoBehaviour
{
    public AudioConductor conductor;
    public string chartTitle = "My Track";
    public float bpm = 120f;
    [Tooltip("Visual/timing compensation")]
    public float offset = 0.12f;
    public KeyCode[] laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

    private readonly List<NoteEvent> events = new List<NoteEvent>();
    private bool recording;

    void Awake() { if (!conductor) conductor = GetComponent<AudioConductor>(); }
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !recording) Begin();

        if (!recording) return;

        for (int i = 0; i < laneKeys.Length; i++)
        {
            if (Input.GetKeyDown(laneKeys[i]))
            {
                events.Add(new NoteEvent { time = conductor.SongTime, lane = i });
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && events.Count > 0)
            events.RemoveAt(events.Count - 1);

        if (Input.GetKeyDown(KeyCode.Return))
            SaveAndStop();

    }

    public void Begin()
    {
        events.Clear();
        recording = true;
        conductor.Play(offset);
        Debug.Log("Recorder: started (R to start again, Enter to save, Backspace to undo");
    }

    void SaveAndStop()
    {
        recording = false;
        var chart = new Chart { title = chartTitle, bpm = bpm, offset = offset, notes = new List<NoteEvent>(events) };
        chart.notes.Sort((a, b) => a.time.CompareTo(b.time));
        var json = JsonUtility.ToJson(chart, true);

        #if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanelInProject("Save Chart JSON", "new_chart", "json", "Save in Charts");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"Saved chart with {chart.notes.Count} notes -> {path}");

        } else
        {
            Debug.Log("Save cancelled");
        }
        #else
        Debug.Log(json);
        #endif
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 600, 100));
        GUILayout.Label(recording
            ? $"Recording... taps: {events.Count} | Enter=save  Backspace=undo"
            : "Press R to start. Tap D/F/J/K in time with the music. Enter=Save, Backspace=Undo.");
        GUILayout.EndArea();

       
    }

}