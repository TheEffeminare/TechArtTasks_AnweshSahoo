// Written with love, light and rainbow :)
// BottomBarDemoToggle.cs

/*
---------------------------------------------
 BottomBarDemoToggle.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A lightweight utility script that provides a simple **toggle button** to show or hide
 the bottom navigation bar (`BottomBarView`) at runtime — complete with smooth movement
 and icon feedback. It’s perfect for demo scenes or test UIs where you want to simulate
 the bar sliding in/out behavior dynamically without navigating away.

 This toggle not only calls the bar’s `Appear()` and `Disappear()` animations but also
 visually adjusts its own position and icon to reflect the current state (visible/hidden),
 giving users an intuitive cue about what will happen when tapped again.

 Usage:  
 Attach this script to a UI **Button** placed near the bottom edge of your screen.  
 - Assign your **BottomBarView** reference in the Inspector.  
 - Optionally link an **icon Image** (like a chevron) and two sprites 
   (`chevronUp` for hidden, `chevronDown` for visible).  
 - Adjust `visibleY` and `hiddenY` to control how far the toggle moves vertically 
   when the bar appears or disappears.  

 When the user taps the button:
 - If the bar is **visible**, it calls `bar.Disappear()` and moves the toggle closer to 
   the bottom (`hiddenY`).
 - If the bar is **hidden**, it calls `bar.Appear()` and moves the toggle upward 
   (`visibleY`).
 The icon also flips or changes sprite accordingly.
*/


using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BottomBarDemoToggle : MonoBehaviour
{
    [SerializeField] BottomBarView bar;
    [Header("Icon (optional)")]
    [SerializeField] Image icon;               
    [SerializeField] Sprite chevronUp;         // show when bar is hidden (tap to show)
    [SerializeField] Sprite chevronDown;       // show when bar is visible (tap to hide)

    [Header("Nudge so it doesn’t collide with the bar")]
    [SerializeField] float visibleY = 72f;     // y when bar is visible (float above it)
    [SerializeField] float hiddenY  = 8f;      // y near screen edge when bar hidden
    [SerializeField] float moveTime = 0.15f;

    RectTransform _rt;
    bool _hidden;
    Coroutine _moveCo;

    void Awake()
    {
        _rt = transform as RectTransform;
        GetComponent<Button>().onClick.AddListener(Toggle);
        if (!bar) bar = FindObjectOfType<BottomBarView>(true);

        // start assuming bar is visible
        _hidden = false;
        ApplyIcon();
        SetYInstant(visibleY);
    }

    public void Toggle()
    {
        if (!bar) return;

        _hidden = !_hidden;
        if (_hidden) bar.Disappear(); else bar.Appear();
        ApplyIcon();

        float targetY = _hidden ? hiddenY : visibleY;
        if (_moveCo != null) StopCoroutine(_moveCo);
        _moveCo = StartCoroutine(TweenY(targetY));
    }

    void ApplyIcon()
    {
        if (!icon) return;
        if (chevronUp && chevronDown)
            icon.sprite = _hidden ? chevronUp : chevronDown;
        else
            icon.rectTransform.localEulerAngles = new Vector3(0, 0, _hidden ? 0f : 180f); // flip if no sprites set
    }

    System.Collections.IEnumerator TweenY(float targetY)
    {
        float t = 0f;
        float start = _rt.anchoredPosition.y;
        while (t < moveTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / moveTime);
            var p = _rt.anchoredPosition;
            p.y = Mathf.Lerp(start, targetY, k);
            _rt.anchoredPosition = p;
            yield return null;
        }
        SetYInstant(targetY);
    }

    void SetYInstant(float y)
    {
        var p = _rt.anchoredPosition; p.y = y; _rt.anchoredPosition = p;
    }
}
