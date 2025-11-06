// Written with love, light and rainbow :)
// correction file

/*
---------------------------------------------
 UIRectNaNGuard.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A safeguard utility designed to prevent RectTransform components
 from ever holding invalid (NaN or Infinity) values during edit or
 runtime. This script constantly monitors critical RectTransform
 properties — such as position, scale, size, anchors, and pivot —
 and restores them to their last known valid state if they ever
 become corrupted.  

 This often happens in Unity when resizing layouts, saving prefabs,
 or switching aspect ratios in Edit Mode, which can lead to UI elements
 disappearing, stretching infinitely, or breaking inspector visuals.
 This script ensures those issues never propagate by applying a
 self-healing mechanism.

 Usage:  
 Attach this script to any UI element prone to corruption (e.g., popups,
 safe area roots, containers). It runs continuously in both Play Mode
 and Edit Mode, automatically fixing invalid RectTransform values
 before they cause visual or structural issues in your layout.
*/


using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways] // run in Edit mode, Prefab Mode, and Play mode
public class UIRectNaNGuard : MonoBehaviour
{
    RectTransform rt;
    Vector2 lastPos, lastSize, lastAnchorMin, lastAnchorMax;
    Vector3 lastScale;
    Vector2 lastPivot;

    void OnEnable()
    {
        rt = transform as RectTransform;
        if (!rt) return;

        // Fix immediately if already NaN (common in Prefab/Edit mode)
        FixNow();

        // Seed "last good" with current (now valid) values
        lastPos      = rt.anchoredPosition;
        lastSize     = rt.sizeDelta;
        lastScale    = rt.localScale;
        lastAnchorMin= rt.anchorMin;
        lastAnchorMax= rt.anchorMax;
        lastPivot    = rt.pivot;
    }

    void Update() // use Update so it also runs in Edit mode
    {
        if (!rt) return;

        // position
        if (IsNaN(rt.anchoredPosition))
            rt.anchoredPosition = lastPos;
        else
            lastPos = rt.anchoredPosition;

        // size
        if (IsNaN(rt.sizeDelta))
            rt.sizeDelta = lastSize;
        else
            lastSize = rt.sizeDelta;

        // scale
        if (IsNaN(rt.localScale))
            rt.localScale = lastScale;
        else
            lastScale = rt.localScale;

        // anchors & pivot (rare but can break inspector)
        if (IsNaN(rt.anchorMin)) rt.anchorMin = lastAnchorMin; else lastAnchorMin = rt.anchorMin;
        if (IsNaN(rt.anchorMax)) rt.anchorMax = lastAnchorMax; else lastAnchorMax = rt.anchorMax;
        if (IsNaN(rt.pivot))     rt.pivot     = lastPivot;     else lastPivot     = rt.pivot;
    }

    void OnValidate()
    {
        // runs in Edit/Prefab mode when values change or prefab is saved
        if (!rt) rt = transform as RectTransform;
        if (!rt) return;
        FixNow(); // ensures serialized values are sane before they get saved
    }


    void FixNow()
    {
        // Provide sane defaults if any field is NaN at attach/enable time
        if (IsNaN(rt.anchoredPosition)) rt.anchoredPosition = Vector2.zero;
        if (IsNaN(rt.sizeDelta))        rt.sizeDelta        = Vector2.zero;
        if (IsNaN(rt.localScale))       rt.localScale       = Vector3.one;
        if (IsNaN(rt.anchorMin))        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        if (IsNaN(rt.anchorMax))        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        if (IsNaN(rt.pivot))            rt.pivot            = new Vector2(0.5f, 0.5f);
    }

    static bool IsNaN(Vector2 v) => float.IsNaN(v.x) || float.IsNaN(v.y);
    static bool IsNaN(Vector3 v) => float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
}
