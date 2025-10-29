using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// the note entries along with the certain times they appear at,
/// and which lane
/// keys D F J K for lane 0 1 2 3
/// </summary>
[System.Serializable]
public class NoteEvent
{
    public float time;
    public int lane;
}

[System.Serializable]
public class Chart
{
    public string title = "Untitled";
    public float bpm = 120f;
    public float offset = 0f;
    public List<NoteEvent> notes = new List<NoteEvent>();
}


/// <summary>
/// use JSON for the chart
/// parse it, give it to note manager
/// </summary>
[CreateAssetMenu(menuName = "DanceHero/ChartData", fileName = "ChartData")]
public class ChartData : ScriptableObject
{
    public TextAsset jsonChart;
    public Chart ParsedChart
    {
        get;
        private set;
    }
    public void Parse()
    {
        if (jsonChart == null)
        {
            Debug.LogError("ChartData: No JSON assigned");
            ParsedChart = new Chart();
            return;
        }
        ParsedChart = JsonUtility.FromJson<Chart>(jsonChart.text);
        if (ParsedChart == null) ParsedChart = new Chart();

    }
}