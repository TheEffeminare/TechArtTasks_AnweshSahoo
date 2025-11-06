using UnityEngine;
using UnityEngine.UI;

public class ToggleKnob : MonoBehaviour
{
    [SerializeField] Toggle toggle;            // assign self
    [SerializeField] RectTransform knob;       // the Knob art
    [SerializeField] float xOff = 18f;         // knob X when OFF
    [SerializeField] float xOn  = 112f;        // knob X when ON
    [SerializeField] float moveTime = 0.12f;   // slide duration
    [SerializeField] AnimationCurve ease = null;

    Coroutine co;

    void Awake()
    {
        if (!toggle) toggle = GetComponent<Toggle>();
        if (ease == null) ease = AnimationCurve.EaseInOut(0,0,1,1);
        // place at correct start
        SetInstant(toggle.isOn);
        toggle.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(bool on)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Slide(on ? xOn : xOff));
    }

    void SetInstant(bool on)
    {
        var p = knob.anchoredPosition;
        p.x = on ? xOn : xOff;
        knob.anchoredPosition = p;
    }

    System.Collections.IEnumerator Slide(float targetX)
    {
        float t = 0f;
        Vector2 start = knob.anchoredPosition, end = start;
        end.x = targetX;
        while (t < moveTime)
        {
            t += Time.unscaledDeltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / moveTime));
            knob.anchoredPosition = Vector2.Lerp(start, end, k);
            yield return null;
        }
        knob.anchoredPosition = end;
    }
}

