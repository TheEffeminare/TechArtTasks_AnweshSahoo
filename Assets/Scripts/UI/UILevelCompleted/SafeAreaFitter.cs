// Written with love, light and rainbow :)

/*
---------------------------------------------
 SafeAreaFitter.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A robust and defensive Safe Area fitter that ensures UI elements
 stay within visible screen boundaries on devices with notches,
 rounded corners, or dynamic islands — without ever producing NaN
 or invalid RectTransform values. It continuously checks for invalid
 or extreme safe area data (like Infinity, NaN, or negative sizes)
 and safely clamps anchors to valid screen bounds.

 Usage:  
 Attach this script to the **SafeAreaRoot** object — a full-stretch
 RectTransform that sits inside your popup or screen canvas. The
 script automatically updates its anchors to match the device’s
 safe area both in Play Mode and the Unity Editor, preventing layout
 breakage caused by faulty or missing safe area data.
*/


using UnityEngine;

/// Robust safe-area fitter that avoids NaN/Infinity writes and bad anchors.
/// Put this on the SafeAreaRoot (a full-stretch RectTransform) inside your popup canvas.
[ExecuteAlways]
[DisallowMultipleComponent]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;

    // caches to avoid unnecessary writes
    Rect  lastSafe;
    int   lastW, lastH;

    // tiny epsilon to enforce min<=max
    const float EPS = 0.0001f;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        ApplyIfNeeded(force:true);
    }

    void OnDisable()
    {
        // no-op
    }

    void Update()
    {
        ApplyIfNeeded();
    }

    void OnRectTransformDimensionsChange()
    {
        // In Edit mode this can fire before Update; guard as well.
        ApplyIfNeeded();
    }

    bool IsBad(float v) => float.IsNaN(v) || float.IsInfinity(v);
    bool IsBad(Vector2 v) => IsBad(v.x) || IsBad(v.y);
    bool IsBad(Rect r) => IsBad(r.x) || IsBad(r.y) || IsBad(r.width) || IsBad(r.height);

    void ApplyIfNeeded(bool force = false)
    {
        if (!rt) return;

        // Defensive: screen size must be valid
        int w = Mathf.Max(1, Screen.width);
        int h = Mathf.Max(1, Screen.height);

        // Defensive: safe area must be sane; if not, fall back to full screen
        Rect safe = Screen.safeArea;
        if (IsBad(safe) || safe.width <= 0f || safe.height <= 0f)
            safe = new Rect(0, 0, w, h);

        // Clamp safe to the screen rect to avoid OEM quirks
        safe.x      = Mathf.Clamp(safe.x, 0, w);
        safe.y      = Mathf.Clamp(safe.y, 0, h);
        safe.width  = Mathf.Clamp(safe.width,  0, w - safe.x);
        safe.height = Mathf.Clamp(safe.height, 0, h - safe.y);

        if (!force && safe == lastSafe && w == lastW && h == lastH)
            return; // nothing to do

        lastSafe = safe; lastW = w; lastH = h;

        // Normalize to 0..1 anchors
        Vector2 anchorMin = new Vector2(safe.x / w,          safe.y / h);
        Vector2 anchorMax = new Vector2((safe.x + safe.width)  / w,
                                        (safe.y + safe.height) / h);

        // Clamp to [0,1] and ensure min <= max (avoid inverted anchors)
        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        if (anchorMin.x > anchorMax.x - EPS) anchorMin.x = anchorMax.x - EPS;
        if (anchorMin.y > anchorMax.y - EPS) anchorMin.y = anchorMax.y - EPS;

        // Apply safely
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Keep a sane pivot; never NaN
        if (float.IsNaN(rt.pivot.x) || float.IsNaN(rt.pivot.y))
            rt.pivot = new Vector2(0.5f, 0.5f);
    }
}
