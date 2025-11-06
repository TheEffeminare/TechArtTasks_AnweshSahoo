// Written with love, light and rainbow :)
// BottomBarView.cs — smooth appear/disappear + indicator slide + pill fade/debounce

/*
---------------------------------------------
 BottomBarView.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A polished and responsive bottom navigation bar controller that manages 
 all interactive behavior, visual transitions, and indicator animations 
 for a multi-tab UI system. This script powers the **bottom bar** seen 
 in many modern mobile games and apps, handling smooth tab selection, 
 animated indicator movement, fade/scale transitions, and event-based 
 content routing.  

 It ensures that the navigation feels dynamic and fluid by combining 
 multiple animation layers — fade, slide, and scale — while maintaining 
 strict control over interactivity to avoid double-taps or visual flickers.  
 It also supports a “tap again to close” interaction pattern and emits 
 clean UnityEvents for external scripts (like `ContentRouter`) to react to.

 Usage:  
 Attach this script to your Bottom Bar root GameObject.  
 - Assign the **CanvasGroup**, **ButtonRow**, and **Selected Background (indicator)**.  
 - Hook up your tab buttons (instances of `BottomBarButton`) that emit 
   `BottomTab` values such as Home, Shop, or Maps.  
 - Optionally assign appear/disappear curves, fade times, and indicator 
   animation timings for custom feel.  
 - Use `Appear()` and `Disappear()` for bar transitions, and subscribe 
   to `ContentActivated` or `Closed` events to handle navigation logic.  

 In short, this script acts as the **brains and animation driver** for the bottom 
 navigation bar — delivering a modern, fluid, and maintainable UI system perfect 
 for scalable mobile interfaces.
*/


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

[System.Serializable] public class TabEvent : UnityEvent<BottomTab> {}

public class BottomBarView : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] RectTransform buttonRow;
    [SerializeField] UIImage selectedBg;                // the blue panel

    [Header("Fade (Bar Appear/Disappear)")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeTime = 0.18f;
    [SerializeField] bool fadeOnEnable = true;
    [SerializeField] float appearOffsetY = 28f;         // slide distance
    [SerializeField] float appearScaleFrom = 0.96f;     // start a bit smaller
    [SerializeField] AnimationCurve appearEase;         // optional; if null uses SmoothStep

    [Header("Startup")]
    [SerializeField] bool selectDefaultOnStart = false; // false = nothing selected at start
    [SerializeField] BottomTab defaultTab = BottomTab.Home;

    [Header("Indicator Anim")]
    [SerializeField] float indicatorBottomInset = 50f;  // raise/lower blue panel
    [SerializeField] float indicatorMoveTime = 0.12f;
    [SerializeField] AnimationCurve indicatorEase = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] float indicatorFadeTime = 0.10f;   // pill fade in/out

    [SerializeField] bool tapSelectedToClose = true;

    [Header("Events")]
    public TabEvent ContentActivated;
    public UnityEvent Closed;

    // internals
    List<BottomBarButton> _buttons = new List<BottomBarButton>();
    BottomBarButton _active;
    Coroutine _moveCo;

    RectTransform _rt;
    Vector2 _basePos;

    // pill visibility tween
    CanvasGroup _selCg;
    Coroutine _indicatorVisCo;

    // click debounce while switching
    bool _switching;

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        _rt = transform as RectTransform;
        _basePos = _rt.anchoredPosition;

        if (buttonRow == null) buttonRow = transform as RectTransform;
        if (_buttons.Count == 0) buttonRow.GetComponentsInChildren(true, _buttons);

        // prepare the selected background pill
        if (selectedBg)
        {
            _selCg = selectedBg.GetComponent<CanvasGroup>();
            if (!_selCg) _selCg = selectedBg.gameObject.AddComponent<CanvasGroup>();
            _selCg.alpha = 0f;
            selectedBg.enabled = false;
        }

        // prep the bar so Appear looks smooth on enable
        if (fadeOnEnable && canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _rt.anchoredPosition = _basePos + new Vector2(0f, -appearOffsetY);
            _rt.localScale = Vector3.one * appearScaleFrom;
        }
    }

    void OnEnable()
    {
        if (!fadeOnEnable || !canvasGroup) return;

        PrepareHiddenPose();                // put it below, scaled, and alpha=0
        StartCoroutine(PlayAppearNextFrame());
    }

    void PrepareHiddenPose()
    {
        if (_rt == null) _rt = transform as RectTransform;
        if (_rt) _basePos = _rt.anchoredPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        if (_rt)
        {
            _rt.anchoredPosition = _basePos + new Vector2(0f, -appearOffsetY);
            _rt.localScale       = Vector3.one * appearScaleFrom;
        }
    }

    System.Collections.IEnumerator PlayAppearNextFrame()
    {
        yield return null; // let layout settle
        yield return null;
        Appear();
    }

    void Start()
    {
        if (!selectDefaultOnStart) return;
        if (_buttons.Count == 0) buttonRow.GetComponentsInChildren(true, _buttons);
        var def = _buttons.Find(b => b.Tab == defaultTab && !b.IsLocked);
        if (def) OnButtonPressed(def);
    }

    // ===== Button selection / hover =====

    public void OnButtonPressed(BottomBarButton btn)
    {
        if (_switching || btn.IsLocked) return;

        // tap again = close
        if (_active == btn)
        {
            if (tapSelectedToClose)
            {
                _switching = true;
                _active.ToggleOff();
                _active = null;
                MoveIndicator(null);
                Closed?.Invoke();
                StartCoroutine(ClearSwitchingAfter(indicatorMoveTime));
            }
            return;
        }

        _switching = true;
        if (_active) _active.ToggleOff();
        _active = btn;
        _active.ToggleOn();

        ApplyLayout(_active);
        MoveIndicator(_active.transform as RectTransform);
        ContentActivated?.Invoke(_active.Tab);

        StartCoroutine(ClearSwitchingAfter(indicatorMoveTime));
    }

    System.Collections.IEnumerator ClearSwitchingAfter(float seconds)
    {
        float extra = 0.05f;
        float t = 0f;
        while (t < seconds + extra) { t += Time.unscaledDeltaTime; yield return null; }
        _switching = false;
    }

    public void DeselectAll(bool emitEvent = true)
    {
        if (_active) _active.ToggleOff();
        _active = null;
        MoveIndicator(null);
        if (emitEvent) Closed?.Invoke();
    }

    public void ShowTab(BottomTab tab)
    {
        if (_buttons.Count == 0) buttonRow.GetComponentsInChildren(true, _buttons);
        var btn = _buttons.Find(b => b.Tab == tab && !b.IsLocked);
        if (btn) OnButtonPressed(btn);
    }

    public void OnHover(BottomBarButton btn, bool entering)
    {
        if (_active != null) return;

        foreach (var b in _buttons)
            b.SetEmphasis(b == btn && entering);

        MoveIndicator(entering ? (btn.transform as RectTransform) : null);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRow);
    }

    void ApplyLayout(BottomBarButton selected)
    {
        foreach (var b in _buttons)
            b.SetEmphasis(b == selected);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRow);
    }

    // ===== Selected blue background slide =====

    void MoveIndicator(RectTransform target)
    {
        if (!selectedBg) return;
        var rt = selectedBg.rectTransform;

        if (target == null)
        {
            if (_moveCo != null) StopCoroutine(_moveCo);
            if (_indicatorVisCo != null) StopCoroutine(_indicatorVisCo);
            _indicatorVisCo = StartCoroutine(TweenIndicatorVisible(false));
            return;
        }

        // ensure visible (fade in if needed)
        if (_indicatorVisCo != null) StopCoroutine(_indicatorVisCo);
        _indicatorVisCo = StartCoroutine(TweenIndicatorVisible(true));

        // same parent, behind buttons
        rt.SetParent(target.parent, worldPositionStays: false);
        rt.SetAsFirstSibling();

        // copy ONLY X anchors from the target, pin to bottom
        rt.anchorMin = new Vector2(target.anchorMin.x, 0f);
        rt.anchorMax = new Vector2(target.anchorMax.x, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);

        // lock Y position (height/design stays yours)
        Vector2 p = rt.anchoredPosition;
        p.y = indicatorBottomInset;
        rt.anchoredPosition = p;

        if (_moveCo != null) StopCoroutine(_moveCo);
        _moveCo = StartCoroutine(SlideTo(rt, target));
    }

    System.Collections.IEnumerator SlideTo(RectTransform rt, RectTransform target)
    {
        float startX = rt.anchoredPosition.x;
        float t = 0f;

        while (t < indicatorMoveTime)
        {
            if (!target) yield break; // target vanished during tween
            t += Time.unscaledDeltaTime;
            float k = indicatorEase.Evaluate(Mathf.Clamp01(t / indicatorMoveTime));

            float curTargetX = target.anchoredPosition.x;

            Vector2 p = rt.anchoredPosition;
            p.x = Mathf.Lerp(startX, curTargetX, k);
            rt.anchoredPosition = p;

            yield return null;
        }

        if (target)
        {
            Vector2 done = rt.anchoredPosition;
            done.x = target.anchoredPosition.x;
            rt.anchoredPosition = done;
        }
    }

    // indicator visibility tween
    System.Collections.IEnumerator TweenIndicatorVisible(bool show)
    {
        if (!selectedBg || !_selCg) yield break;

        selectedBg.enabled = true; // needed to see the fade
        float from = _selCg.alpha;
        float to   = show ? 1f : 0f;
        float t = 0f;

        while (t < indicatorFadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / indicatorFadeTime);
            _selCg.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        _selCg.alpha = to;
        if (!show) selectedBg.enabled = false; // disable only after fade out
    }

    // ===== Appear / Disappear (fade + slide + scale) =====

    public void Appear()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(FadeSlideScale(true));
    }

    public void Disappear()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(FadeSlideScale(false));
    }

    System.Collections.IEnumerator FadeSlideScale(bool visible)
    {
        if (!canvasGroup) yield break;

        float dur = Mathf.Max(0.01f, fadeTime);
        float t = 0f;

        float a0 = canvasGroup.alpha;
        float a1 = visible ? 1f : 0f;

        Vector2 p0 = _rt.anchoredPosition;
        Vector2 p1 = visible ? _basePos : _basePos + new Vector2(0f, -appearOffsetY);

        Vector3 s0 = _rt.localScale;
        Vector3 s1 = Vector3.one * (visible ? 1f : appearScaleFrom);

        // disable clicks while tweening
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float e = (appearEase != null) ? appearEase.Evaluate(k) : Mathf.SmoothStep(0f, 1f, k);

            canvasGroup.alpha    = Mathf.Lerp(a0, a1, e);
            _rt.anchoredPosition = Vector2.Lerp(p0, p1, e);
            _rt.localScale       = Vector3.Lerp(s0, s1, e);
            yield return null;
        }

        canvasGroup.alpha    = a1;
        _rt.anchoredPosition = p1;
        _rt.localScale       = s1;

        bool on = visible && a1 >= 0.99f;
        canvasGroup.interactable   = on;
        canvasGroup.blocksRaycasts = on;
    }
}
