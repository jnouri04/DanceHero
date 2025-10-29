using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// game manager that has scoring combo confidence freestyle and ui updates
/// </summary>
public class GameplayManager : MonoBehaviour
{
    [Header("Intro / Countdown")]
    public TMP_Text countdownText;
    public float introDelay = 3f;

    [Header("Refs")]
    public NoteManager noteManager;
    public DancerController dancer;
    public AudioSource sfxSource;
    public AudioClip sfxHit;
    public AudioClip sfxMiss;
    public AudioClip sfxCheer;
    public AudioClip sfxBoo;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public Slider confidenceBar;
    public Slider songProgress;
    public GameObject freestyleBadge;
    public TMP_Text hintText;

    private const string DefaultHint = "Keys: D F J K * Space = Freestyle";

    [Header("FreestyleWarning")]
    public TMP_Text freestyleCountdownText;
    public float freestyleWarnSeconds = 2f;
    public AudioClip sfxTick;
    private float nextTickAt = 0f;

    [Header("Scoring")]
    public int score; // { get; private set; }
    public int combo; //{ get; private set; }
    public int maxCombo; //{ get; private set; }
    public float confidence = 0.7f;
    public int perfectPts = 100;
    public int greatPts = 70;
    public int goodPts = 40;
    public int missPenalty = 0;

    [Header("Freestyle")]
    public int streakForFreestyle = 20;
    public float freestyleDuration = 6f;
    public float freestyleMultiplier = 2f;
    private bool freestyleReady = false;
    private bool freestyleActive = false;
    private float freestyleEndTime = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        if (freestyleCountdownText) freestyleCountdownText.gameObject.SetActive(false);
        noteManager.OnJudgement += OnJudgement;
        UpdateUI();
        if (hintText) hintText.text = DefaultHint;
        freestyleBadge.SetActive(false);

        introDelay = noteManager.introDelay;

        noteManager.allowSpawning = false;
        StartCoroutine(DoCountdownAndStart());

    }

    private System.Collections.IEnumerator DoCountdownAndStart()
    {
        float t = Mathf.Max(0f, introDelay);
        while(t>0.5f)
        {
            if (countdownText) countdownText.text = Mathf.CeilToInt(t).ToString();
            yield return null;
            t -= Time.unscaledDeltaTime;
        }
        if (countdownText) countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        if (countdownText) countdownText.gameObject.SetActive(false);
        noteManager.allowSpawning = true;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleInput();

        if(noteManager.SongLength > 0f)
        {
            var t = Mathf.Clamp01(noteManager.conductor.SongTime / noteManager.SongLength);
            songProgress.value = t;
        }

        if (freestyleActive)
        {
            float remaining = freestyleEndTime - Time.time;

            if (freestyleCountdownText != null)
            {
                if (remaining <= freestyleWarnSeconds)
                {
                    if (!freestyleCountdownText.gameObject.activeSelf)
                        freestyleCountdownText.gameObject.SetActive(true);

                    freestyleCountdownText.text = (remaining > 1f)
                        ? Mathf.CeilToInt(remaining).ToString()
                        : remaining.ToString("0.0");

                    float warnT = Mathf.InverseLerp(freestyleWarnSeconds, 0f, remaining);
                    freestyleCountdownText.color = Color.Lerp(Color.red, Color.white, warnT);
                    float pulse = 1f + 0.15f * Mathf.PingPong(Time.time * 6f, 1f);
                    freestyleCountdownText.transform.localScale = new Vector3(pulse, pulse, 1f);

                    if (sfxSource != null && sfxTick != null && Time.time >= nextTickAt)
                    {
                        sfxSource.PlayOneShot(sfxTick, 0.8f);
                        nextTickAt = Mathf.Floor(Time.time) + 1f;
                    }
                } else
                {
                    if (freestyleCountdownText.gameObject.activeSelf)
                        freestyleCountdownText.gameObject.SetActive(false);
                    nextTickAt = Mathf.Ceil(Time.time);
                }
            }
            if (Time.time >= freestyleEndTime)
            {
                freestyleActive = false;
                if (dancer != null) dancer.SetFreestyle(false);
                if (freestyleCountdownText != null) freestyleCountdownText.gameObject.SetActive(false);

                if (hintText) hintText.text = DefaultHint;
                if (freestyleBadge != null)
                {
                    if (freestyleBadge.TryGetComponent<TMPro.TMP_Text>(out var btxt)) btxt.text = "READY";
                    freestyleBadge.SetActive(freestyleReady);
                }
            }
        } else
        {
            if (freestyleCountdownText != null && freestyleCountdownText.gameObject.activeSelf)
                freestyleCountdownText.gameObject.SetActive(false);
        }

        if (confidence <= 0.01f)
        {
            EndSong(false);
        } else if (noteManager != null && noteManager.conductor != null &&
            noteManager.AllNotesProcessed && !noteManager.conductor.IsPlaying)
        {
            EndSong(true);
        }
    }

    private void HandleInput()
    {
        if (!noteManager.allowSpawning) return;
        if (Input.GetKeyDown(KeyCode.D)) LanePress(0);
        if (Input.GetKeyDown(KeyCode.F)) LanePress(1);
        if (Input.GetKeyDown(KeyCode.J)) LanePress(2);
        if (Input.GetKeyDown(KeyCode.K)) LanePress(3);

        if (freestyleReady && !freestyleActive && Input.GetKeyDown(KeyCode.Space))
        {
            ActivateFreestyle();
        }

        if (freestyleActive)
        {
            if (Input.anyKeyDown)
            {
                int add = Mathf.RoundToInt(10 * freestyleMultiplier);
                score += add;
                dancer.PlayMove(Random.Range(0, 4), confidence);
                UpdateUI();
            }
        }
    }

    private void LanePress(int lane)
    {
        if (freestyleActive)
        {
            bool hit = noteManager != null && noteManager.TryHitLane(lane, out _, out _);

            if (!hit)
            {
                int add = Mathf.RoundToInt(10 * freestyleMultiplier);
                score += add;
                combo++; maxCombo = Mathf.Max(maxCombo, combo);
                confidence = Mathf.Clamp01(confidence + 0.02f);
                if (sfxSource && sfxHit) sfxSource.PlayOneShot(sfxHit);
                dancer.PlayMove(lane, confidence);
                UpdateUI();
            }
            return;
        }

        if (noteManager != null) noteManager.TryHitLane(lane, out _, out _);

        /*
        string grade; float delta;
        bool hit = noteManager.TryHitLane(lane, out grade, out delta);

        if (!hit)
        {
            OnJudgement(lane, "MISS", delta);
            return;
        }
        else
        {
            OnJudgement(lane, grade, delta);

        }
        */
    }

    private void OnJudgement(int lane, string grade, float delta)
    {
        if (freestyleActive && grade == "MISS")
            return;
        int add = 0;
        float confDelta = 0f;
        switch (grade)
        {
            case "PERFECT": add = Mathf.RoundToInt(perfectPts * (1 + combo * 0.01f)); confDelta = +0.05f; break;
            case "GREAT": add = Mathf.RoundToInt(greatPts * (1 + combo * 0.01f)); confDelta = +0.035f; break;
            case "GOOD": add = Mathf.RoundToInt(goodPts * (1 + combo * 0.01f)); confDelta = +0.02f; break;
            case "MISS": add = -missPenalty; confDelta = -0.08f; break;
        }

        if (grade == "MISS")
        {
            combo = 0;
            sfxSource.PlayOneShot(sfxMiss);
            dancer.Stumble();
            confidence = Mathf.Clamp01(confidence + confDelta);
            UpdateUI();
            return;
        }

        if (grade != "MISS" && freestyleActive)
        {
            add = Mathf.RoundToInt(add * freestyleMultiplier);
        }

        score += add;
        combo++;
        maxCombo = Mathf.Max(maxCombo, combo);
        confidence = Mathf.Clamp01(confidence + confDelta);

        if (combo == streakForFreestyle)
        {
            freestyleReady = true;
            freestyleBadge.SetActive(true);
            sfxSource.PlayOneShot(sfxCheer);
        }

        sfxSource.PlayOneShot(sfxHit);
        dancer.PlayMove(lane, confidence);
        UpdateUI();
    }

    private void ActivateFreestyle()
    {
        freestyleReady = false;
        freestyleActive = true;
        freestyleEndTime = Time.time + freestyleDuration;
        

        if (freestyleBadge)
        {
            freestyleBadge.SetActive(true);
            if (freestyleBadge.TryGetComponent<TMP_Text>(out var btxt))
                btxt.text = "FREESTYLE x2";
        }
        if (freestyleCountdownText) freestyleCountdownText.gameObject.SetActive(false);

        dancer.SetFreestyle(true);
        if (sfxSource && sfxCheer) sfxSource.PlayOneShot(sfxCheer);
        if (hintText) hintText.text = "Freestyle!";

        nextTickAt = Mathf.Ceil(Time.time);
    }

    private void UpdateUI()
    {
        scoreText.text = $"Score: {score:n0}";
        comboText.text = $"Combo: {combo}";
        confidenceBar.value = confidence;
    }

    private void EndSong(bool completed)
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetInt("MaxCombo", maxCombo);
        PlayerPrefs.SetFloat("Confidence", confidence);
        PlayerPrefs.SetInt("Completed", completed ? 1 : 0);
        SceneManager.LoadScene("Results");

    }
}
