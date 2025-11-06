using UnityEngine;
using UnityEditor;

public class FixRectNaN : EditorWindow
{
    [MenuItem("Tools/Fix NaN RectTransforms")]
    static void FixSelected()
    {
        int fixedCount = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            foreach (RectTransform rt in go.GetComponentsInChildren<RectTransform>(true))
            {
                Vector3 pos = rt.localPosition;
                Vector2 anchored = rt.anchoredPosition;
                Vector2 size = rt.sizeDelta;
                Vector3 scale = rt.localScale;

                bool changed = false;

                if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
                { pos = Vector3.zero; changed = true; }

                if (float.IsNaN(anchored.x) || float.IsNaN(anchored.y))
                { anchored = Vector2.zero; changed = true; }

                if (float.IsNaN(size.x) || float.IsNaN(size.y))
                { size = new Vector2(100, 100); changed = true; }

                if (float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z))
                { scale = Vector3.one; changed = true; }

                if (changed)
                {
                    Undo.RecordObject(rt, "Fix NaN RectTransform");
                    rt.localPosition = pos;
                    rt.anchoredPosition = anchored;
                    rt.sizeDelta = size;
                    rt.localScale = scale;
                    fixedCount++;
                }
            }
        }

        Debug.Log($"✅ Fixed {fixedCount} RectTransforms with NaN values.");
    }
}
