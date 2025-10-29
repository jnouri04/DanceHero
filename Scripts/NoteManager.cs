using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// note manager that spawns the notes based on the chart
/// supposed to cross the hit line at the correct time
/// judges the hit timing
/// </summary>
public class NoteManager : MonoBehaviour
{

    [Header("Intro")]
    public float introDelay = 3f;
    [HideInInspector] public bool allowSpawning = false;


    [Header("Refs")]
    public AudioConductor conductor;
    public ChartData chartData;
    public Transform lanesRoot;
    public Transform hitLine;
    public Transform spawnLine;
    public GameObject notePrefab;

    [Header("Timing / Movement")]
    [Tooltip("How many seconds early a note appears above the hit line.")]
    public float appearLeadTime = 1.5f;
    [Tooltip("Pixels per unit (for y-movement math). Usually world units are fine; we use distance instead.")]
    public float laneTravelDistance = 6f;

    [Header("Judgement Windows (seconds)")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.10f;
    public float goodWindow = 0.15f;
    public float missWindow = 0.20f;

    private List<NoteEvent> notes;
    private int nextIndex = 0;

    private readonly List<NoteObject>[] laneActives = new List<NoteObject>[4];

    public System.Action<int, string, float> OnJudgement;

    private float SpawnY => spawnLine.position.y;
    private float HitY => hitLine.position.y;

    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < laneActives.Length; i++) laneActives[i] = new List<NoteObject>();
        chartData.Parse();
        notes = chartData.ParsedChart.notes;
        conductor.Play(chartData.ParsedChart.offset, introDelay);

    }

    // Update is called once per frame
    void Update()
    {
        if (!allowSpawning) return;

        float songTime = conductor.SongTime;
        while(nextIndex < notes.Count && notes[nextIndex].time <= songTime + appearLeadTime)
        {
            SpawnNote(notes[nextIndex]);
            nextIndex++;

        }
        for (int lane = 0;lane < 4; lane++)
        {
            for (int i = laneActives[lane].Count - 1; i >= 0; i--)
            {
                var n = laneActives[lane][i];
                float tToHit = n.hitTime - songTime;
                float y = HitY + (tToHit / appearLeadTime) * (SpawnY - HitY);
                var p = n.transform.position;
                n.transform.position = new Vector3(p.x, y, p.z);

                if (tToHit < -missWindow)
                {
                    Judge(n, "MISS", tToHit);
                    laneActives[lane].RemoveAt(i);
                    Destroy(n.gameObject);

                }
            }
        }
    }
    private void SpawnNote(NoteEvent e)
    {
        var laneT = lanesRoot.GetChild(e.lane);
        var go = Instantiate(notePrefab, laneT.position + Vector3.up * (SpawnY - HitY), Quaternion.identity, lanesRoot);
        var no = go.GetComponent<NoteObject>();
        no.lane = e.lane;
        no.hitTime = e.time;
        laneActives[e.lane].Add(no);

    }

    public bool TryHitLane(int lane, out string grade, out float delta)
    {
        grade = null; delta = 0f;
        if (laneActives[lane].Count == 0)
        {
            grade = "MISS";
            delta = 999f;
            OnJudgement?.Invoke(lane, grade, delta);
            return false;

        }
        var n = laneActives[lane][0];
        float d = n.hitTime - conductor.SongTime;
        float ad = Mathf.Abs(d);

        if (ad <= perfectWindow) grade = "PERFECT";
        else if (ad <= greatWindow) grade = "GREAT";
        else if (ad <= goodWindow) grade = "GOOD";
        else
        {
            grade = "MISS";
        }

        OnJudgement?.Invoke(lane, grade, d);

        if (grade != "MISS")
        {
            laneActives[lane].RemoveAt(0);
            Destroy(n.gameObject);
            delta = d;
            return true;
        }
        else
        {
            delta = d;
            return false;

        }
    }

    private void Judge(NoteObject n, string grade, float delta)
    {
        OnJudgement?.Invoke(n.lane, grade, delta);

    }
    public float SongLength => conductor.musicSource.clip ? conductor.musicSource.clip.length : 0f;
    public bool AllNotesProcessed => nextIndex >= notes.Count;
}
