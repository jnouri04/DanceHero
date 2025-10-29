using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultsController : MonoBehaviour
{

    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public TMP_Text detailText;

    // Start is called before the first frame update
    void Start()
    {
        int score = PlayerPrefs.GetInt("LastScore", 0);
        int maxCombo = PlayerPrefs.GetInt("MaxCombo", 0);
        float conf = PlayerPrefs.GetFloat("Confidence", 0f);
        bool completed = PlayerPrefs.GetInt("Completed", 0) == 1;

        if (titleText) titleText.text = completed ? "Song Complete!" : "Confidence Broke!";
        if (scoreText) scoreText.text = $"Score: {score:n0}";
        if (detailText) detailText.text = $"MaxCombo: {maxCombo} \n* Final Confidence: {(int)(conf * 100)}%";

        
    }

    public void Retry() => SceneManager.LoadScene("Game");
    public void ToMenu() => SceneManager.LoadScene("Menu");


    // Update is called once per frame
    void Update()
    {
        
    }
}
