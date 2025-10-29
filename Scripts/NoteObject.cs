using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// the note object
/// has its lane, time, and sprite renderer
/// </summary>
public class NoteObject : MonoBehaviour
{
    public int lane;
    public float hitTime;
    public SpriteRenderer sr;
    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
