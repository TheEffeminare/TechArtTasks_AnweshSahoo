// Written with love, light and rainbow :)

/*
---------------------------------------------
 ScaleToSafeHeight.cs
---------------------------------------------
 Author: Anwesh Sahoo
 Description:
 Automatically scales a target UI element (typically the content
 area within a popup or panel) based on the device’s current safe
 screen height. This ensures that the UI remains comfortably visible
 and proportionate across devices with notches, dynamic islands, or
 varying aspect ratios — preventing oversized or clipped layouts.

 Usage:
 Attach this script to any active UI object and assign the RectTransform
 you want to scale (e.g., the popup Content). The script compares the
 device’s safe area height to a defined reference height (e.g., 1920px)
 and smoothly adjusts the element’s overall scale between the defined
 minimum and maximum limits in real time.
*/


using UnityEngine;

[ExecuteAlways]
public class ScaleToSafeHeight : MonoBehaviour
{
    public RectTransform target;          // assign Content
    public float referenceHeight = 1920f; // matches CanvasScaler reference
    public float minScale = 0.2f;        // how small we allow
    public float maxScale = 1.0f;

    void LateUpdate()
    {
        if (!target) return;
        float h = Screen.safeArea.height;
        float s = Mathf.Clamp(h / referenceHeight, minScale, maxScale);
        target.localScale = new Vector3(s, s, 1f);
    }
}
