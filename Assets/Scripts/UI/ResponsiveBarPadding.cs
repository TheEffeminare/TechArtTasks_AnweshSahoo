// Written with love, light and rainbow :)

/*
---------------------------------------------
 ResponsiveBarPadding.cs
---------------------------------------------
 Author: Anwesh Sahoo
 Description:
 Dynamically adjusts the left/right padding and spacing
 of the top and bottom horizontal layout groups (HLGs)
 based on the device’s screen aspect ratio.

 This ensures UI bars look balanced across different devices:
 - On tall phones (e.g., 21:9), elements are tighter together.
 - On tablets (e.g., 4:3), elements have more breathing room.
 - Automatically interpolates (lerps) padding and spacing
   values between these aspect ranges.

 Usage:
 Attach this component to any persistent UI object in your
 scene (e.g., Canvas). Assign your top and bottom bar
 HorizontalLayoutGroups. The script will continuously adjust
 their layout in both Play Mode and Editor (Simulator).

*/


using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ResponsiveBarPadding : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] HorizontalLayoutGroup topBarHLG;     // e.g. TopBar_Container
    [SerializeField] HorizontalLayoutGroup bottomBarHLG;  // e.g. BottomBar_Root/ButtonRow

    [Header("Tablet vs Tall Phone")]
    [Tooltip("<= this (portrait) behaves like a very tall phone (e.g., Z-Fold)")]
    public float tallPhoneAspect = 0.46f;     // ~21:9 (1080x2400 -> 0.45)
    [Tooltip(">= this behaves like tablet (iPad ~ 0.75)")]
    public float tabletAspect    = 0.75f;

    [Header("Top Bar Padding (L/R) across aspect")]
    public int topPadAtTall   = 24;           // tight on tall phones
    public int topPadAtMid    = 40;           // normal phones ~19.5:9 (≈0.5–0.6)
    public int topPadAtTablet = 110;          // roomy on iPad 3:4
    public int topSpacingAtTall = 8;
    public int topSpacingAtMid  = 12;
    public int topSpacingAtTablet = 18;

    [Header("Bottom Bar Padding (L/R) across aspect")]
    public int bottomPadAtTall   = 24;        // bring buttons closer on Z-Fold
    public int bottomPadAtMid    = 48;        // default phones
    public int bottomPadAtTablet = 80;        // spread out on iPad
    public int bottomSpacingAtTall = 6;
    public int bottomSpacingAtMid  = 10;
    public int bottomSpacingAtTablet = 14;

    void OnEnable()  { Apply(); }
#if UNITY_EDITOR
    void Update()    { Apply(); } // live in editor/simulator
#endif

    void Apply()
    {
        float aspect = (float)Screen.width / Screen.height; // portrait aspect
        float t = Mathf.InverseLerp(tallPhoneAspect, tabletAspect, aspect); // 0=tall, 1=tablet

        // Lerp helper
        int LerpI(int a, int b, float k) => Mathf.RoundToInt(Mathf.Lerp(a, b, k));

        // === TOP BAR ===
        if (topBarHLG)
        {
            var p = topBarHLG.padding;
            // three-point blend (tall->mid->tablet)
            int padL = LerpI(topPadAtTall, topPadAtMid, Mathf.Clamp01(t*2f));
            padL     = LerpI(padL, topPadAtTablet, Mathf.Clamp01((t-0.5f)*2f));
            int padR = padL;

            int sp   = LerpI(topSpacingAtTall, topSpacingAtMid, Mathf.Clamp01(t*2f));
            sp       = LerpI(sp, topSpacingAtTablet, Mathf.Clamp01((t-0.5f)*2f));

            p.left = padL; p.right = padR;
            topBarHLG.padding = p;
            topBarHLG.spacing = sp;
            LayoutRebuilder.ForceRebuildLayoutImmediate(topBarHLG.transform as RectTransform);
        }

        // === BOTTOM BAR === (the HLG that lays out the 5 buttons)
        if (bottomBarHLG)
        {
            var p = bottomBarHLG.padding;

            int padL = LerpI(bottomPadAtTall, bottomPadAtMid, Mathf.Clamp01(t*2f));
            padL     = LerpI(padL, bottomPadAtTablet, Mathf.Clamp01((t-0.5f)*2f));
            int padR = padL;

            int sp   = LerpI(bottomSpacingAtTall, bottomSpacingAtMid, Mathf.Clamp01(t*2f));
            sp       = LerpI(sp, bottomSpacingAtTablet, Mathf.Clamp01((t-0.5f)*2f));

            p.left = padL; p.right = padR;
            bottomBarHLG.padding = p;
            bottomBarHLG.spacing = sp;
            LayoutRebuilder.ForceRebuildLayoutImmediate(bottomBarHLG.transform as RectTransform);
        }
    }
}
