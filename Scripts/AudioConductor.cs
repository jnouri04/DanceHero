using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// song timer, uses audio settings to line notes up with audio
/// </summary>
public class AudioConductor : MonoBehaviour
{
    public static AudioConductor Instance
    {
        get;
        private set;
    }

    public AudioSource musicSource;

    [Tooltip("Extra scheduling lead time for robust start")]

    public double startLead = 0.20;

    private double dspSongStart = -1;
    private float offsetSec = 0f;

    private void Awake()
    {
        Instance = this;
        if (!musicSource) musicSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// make sure song has a bit of delay so it does not start
    /// right away
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="introDelay"></param>

    public void Play(float offset, float introDelay = 0f)
    {
        offsetSec = offset;
        double dspStart = AudioSettings.dspTime + startLead + introDelay;
        dspSongStart = dspStart + offsetSec;
        musicSource.PlayScheduled(dspStart);
    }

    public double SongDspTime => AudioSettings.dspTime - dspSongStart;
    public float SongTime => (float)SongDspTime;

    public bool IsPlaying => musicSource && musicSource.isPlaying;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
