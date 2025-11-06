// SafeArea.cs
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform rt;
    Rect last;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        // only re-apply when safe area changes *and* screen size is valid
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        if (last != Screen.safeArea)
            Apply();
    }

    void Apply()
    {
        // skip if screen size invalid (prevents NaNs in edit/prefab mode)
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        last = Screen.safeArea;

        // compute anchors safely
        var min = last.position;
        var max = last.position + last.size;

        float minX = min.x / Screen.width;
        float minY = min.y / Screen.height;
        float maxX = max.x / Screen.width;
        float maxY = max.y / Screen.height;

        // clamp between 0 and 1 just in case of rounding errors
        rt.anchorMin = new Vector2(Mathf.Clamp01(minX), Mathf.Clamp01(minY));
        rt.anchorMax = new Vector2(Mathf.Clamp01(maxX), Mathf.Clamp01(maxY));

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
