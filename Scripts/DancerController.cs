using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// controls the dancer, changes its pose smushes it or wobbles the dancer
/// based on the confidence
/// 
/// </summary>
public class DancerController : MonoBehaviour
{

    public SpriteRenderer sr;
    [Header("Smooth Poses (0..3)")]
    public Sprite[] smoothPoses = new Sprite[4];
    [Header("Clumsy Poses (0..3")]
    public Sprite[] clumsyPoses = new Sprite[4];
    public Sprite neutral;


    [Header("Motion Tuning")]
    [Range(0f, 0.2f)] public float wobbleSmoothMax = 0.04f;
    [Range(0f, 0.2f)] public float wobbleClumsyMax = 0.08f;
    public float relaxSpeed = 10f;
    public bool enableScaleWobble = true;
    public bool enableStumbleSquash = true;

    private Vector3 baseScale;
    private Quaternion baseRot;
    private Vector3 targetScale;
    private Quaternion targetRot;

    
    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (neutral) sr.sprite = neutral;

        baseScale = transform.localScale;
        baseRot = transform.rotation;

        targetScale = baseScale;
        targetRot = baseRot;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * relaxSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * relaxSpeed);
    }

    public void PlayMove(int lane, float confidence)
    {
        bool smooth = confidence >= 0.5f;
        var set = smooth ? smoothPoses : clumsyPoses;
        if (lane >= 0 && lane < set.Length && set[lane] != null)
        {
            sr.sprite = set[lane];
        }

        float wobbleMax = smooth ? wobbleSmoothMax : wobbleClumsyMax;

        if (enableScaleWobble)
        {
            float s = 1f + Random.Range(-wobbleMax, wobbleMax);
            targetScale = baseScale * s;
        } else
        {
            targetScale = baseScale;
        }
        targetRot = baseRot * Quaternion.Euler(0f, 0f, Random.Range(-wobbleMax * 80f, wobbleMax * 80f));

    }

    public void Stumble()
    {
        if (!enableStumbleSquash) { targetScale = baseScale; return; }
        targetScale = new Vector3(baseScale.x * 0.9f, baseScale.y * 1.1f, baseScale.z);
    }

    public void SetFreestyle(bool on)
    {
        
    }

    public void ResetPose()
    {
        targetScale = baseScale;
        targetRot = baseRot;
        if (neutral) sr.sprite = neutral;
    }
}
