// ResponsiveCanvasMatch.cs
// Written with love, light and rainbow :)

/*
---------------------------------------------
 ResponsiveCanvasMatch.cs
---------------------------------------------
 Author: Anwesh Sahoo
 Description:
 Dynamically adjusts the Canvas Scaler’s “Match Width or Height”
 setting based on the current device’s screen aspect ratio. This
 helps ensure the UI scales properly across a wide range of devices
 — from tall, narrow phones to wider tablets — maintaining visual
 balance and readability.

 Usage:
 Attach this script to your main Canvas that uses a Canvas Scaler.
 It automatically calculates the aspect ratio at runtime (and in
 the Editor Simulator) and adjusts the “matchWidthOrHeight” value,
 blending smoothly between width-based and height-based scaling
 according to the screen’s proportions.

*/


using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvasMatch : MonoBehaviour
{
    [Tooltip("Aspect<=this → match=0 (width)")]
    public float tallPhoneAspect = 0.46f;   // ~21:9 phones
    [Tooltip("Aspect>=this → match=1 (height)")]
    public float tabletAspect    = 0.75f;   // iPad ~3:4

    CanvasScaler scaler;

    void Awake(){ scaler = GetComponent<CanvasScaler>(); Apply(); }
#if UNITY_EDITOR
    void Update(){ Apply(); } // hot-reload in editor/simulator
#endif

    void Apply()
    {
        float aspect = (float)Screen.width / Screen.height; // portrait value
        float match = Mathf.InverseLerp(tallPhoneAspect, tabletAspect, aspect);
        scaler.matchWidthOrHeight = Mathf.Clamp01(match);
    }
}
