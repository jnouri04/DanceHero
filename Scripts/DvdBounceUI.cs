using UnityEngine;
using TMPro;
using UnityEngine.UI;


/// <summary>
/// make title bounce like DVD logo
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DvdBounceUI : MonoBehaviour {
    public RectTransform container;     
    public float speed = 300f;           
    public Vector2 initialDirection = new Vector2(1f, 1f);
    public bool randomizeColorOnBounce = true;
    public AudioSource sfxSource;        
    public AudioClip bounceSfx;          

    private RectTransform rt;
    private Vector2 velocity;
    private TMP_Text tmp;
    private Image img;

    void Awake() {
        rt = GetComponent<RectTransform>();
        tmp = GetComponent<TMP_Text>();
        img = GetComponent<Image>();
        if (container == null) container = rt.parent as RectTransform;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var dir = initialDirection.sqrMagnitude < 0.001f ? new Vector2(1f, 1f) : initialDirection.normalized;
        velocity = dir * speed;
    }

    void Update() {
    if (container == null) return;

    float tLeft = Time.unscaledDeltaTime;

    Vector2 pos = rt.anchoredPosition;
    Vector2 elemSize = new Vector2(rt.rect.width * rt.lossyScale.x, rt.rect.height * rt.lossyScale.y) ;
    Vector2 halfElem = elemSize * 0.5f;
    Vector2 halfCont = container.rect.size * 0.5f;

    float pad = 0f;

    float minX = -halfCont.x + halfElem.x + pad;
    float maxX =  halfCont.x - halfElem.x - pad;
    float minY = -halfCont.y + halfElem.y + pad;
    float maxY =  halfCont.y - halfElem.y - pad;

    for (int guard = 0; guard < 4 && tLeft > 0f; guard++) {
        float tx = float.PositiveInfinity;
        float ty = float.PositiveInfinity;

        if (Mathf.Abs(velocity.x) > 1e-5f) {
            float targetX = velocity.x > 0f ? maxX : minX;
            tx = (targetX - pos.x) / velocity.x; // seconds to reach wall on X
        }
        if (Mathf.Abs(velocity.y) > 1e-5f) {
            float targetY = velocity.y > 0f ? maxY : minY;
            ty = (targetY - pos.y) / velocity.y; // seconds to reach wall on Y
        }

        if (tx < 0f) tx = float.PositiveInfinity;
        if (ty < 0f) ty = float.PositiveInfinity;

        float tHit = Mathf.Min(tx, ty);

        if (tHit <= tLeft) {
            pos += velocity * tHit;

            bool hitX = Mathf.Approximately(tHit, tx);
            bool hitY = Mathf.Approximately(tHit, ty);

            if (hitX) { 
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                velocity.x = -velocity.x; 
            }
            if (hitY) { 
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                velocity.y = -velocity.y; 
            }

            if ((hitX || hitY) && randomizeColorOnBounce) {
                var c = new Color(Random.value, Random.value, Random.value, 1f);
                if (tmp) tmp.color = c;
                if (img) img.color = c;
            }
            if ((hitX || hitY) && sfxSource && bounceSfx) sfxSource.PlayOneShot(bounceSfx, 0.6f);

            tLeft -= tHit; 
        } else {
            pos += velocity * tLeft;
            tLeft = 0f;
        }
    }

    rt.anchoredPosition = pos;
}
}