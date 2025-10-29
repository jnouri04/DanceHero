using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// menu controller with play and quit
/// </summary>

public class MenuController : MonoBehaviour
{
    [Header("Optional default selection")]
    public Selectable firstSelected;

    private void OnEnable()
    {
        if (firstSelected != null)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);

    }

    public void Play()
    {
        SceneManager.LoadScene("Game"); 
    }
    public void Quit() => Application.Quit();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
