// Written with love, light and rainbow :)
// BottomBarButton.cs by Anwesh Sahoo


/*
---------------------------------------------
 BottomBarButton.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A comprehensive and animation-driven button controller used in the **Bottom Navigation Bar UI**.  
Each button represents a distinct tab (like Home, Shop, Maps) and is responsible for its own  
visual feedback — handling states such as *On*, *Off*, and *Locked* — while managing smooth  
width, scale, icon, and label transitions.  

This script powers the dynamic, bouncy feel of the navigation experience by using multiple 
coroutines to animate width expansion, icon bounce, label fade-in, and vertical motion — 
creating a modern, polished UI interaction similar to premium mobile apps.  

Usage:  
Attach this script to a UI button inside the bottom bar layout.  
- Assign **Icon**, **Label**, and optionally an **Animator** for state feedback.  
- Define widths and animation timings in the Inspector.  
- The button automatically connects to its parent `BottomBarView`, calling  
  `OnButtonPressed()` whenever it’s clicked.  
- The `BottomBarView` then ensures that only one button remains active while this button 
  animates into its emphasized state.

When pressed:
- If the button is **locked**, it pulses to indicate restriction.  
- If **active**, it expands in width, lifts its icon, fades and raises its label,  
  and optionally plays a bounce animation.  
- When **deselected**, it smoothly returns to its neutral width and layout.  
*/


using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum BottomTab { Home, Shop, Maps, LockedA, LockedB }
public enum BottomBarButtonState { Locked, Off, On }

[RequireComponent(typeof(Button)), RequireComponent(typeof(LayoutElement))]
public class BottomBarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Identity")]
    [SerializeField] string id = "Home";
    [SerializeField] BottomTab tab = BottomTab.Home;

    [Header("State")]
    [SerializeField] bool isLocked;

    [Header("Refs")]
    [SerializeField] Animator animator;            // optional
    [SerializeField] UnityEvent onClicked;         // optional SFX
    [SerializeField] RectTransform icon;           // child "Icon"
    [SerializeField] GameObject label;             // child "Label" (TMP)

    [Header("Layout")]
    [SerializeField] float normalWidth = 140f;
    [SerializeField] float emphasizedWidth = 340f;

    [Header("Anim (shared easing)")]
    [SerializeField] float widthAnimTime = 0.15f;
    [SerializeField] float scaleAnimTime = 0.12f;
    [SerializeField] float iconScaleWhenEmphasized = 1.12f;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Label Tween")]
    [SerializeField] float labelFadeTime = 0.12f;
    [SerializeField] float labelRisePixels = 16f;

    [Header("Icon Rise/Bounce")]
    [SerializeField] float iconRisePixels = 12f;   // how far up the icon moves when ON
    [SerializeField] float iconRiseTime   = 0.18f; // move duration
    [SerializeField] float bounceDuration = 0.25f; // bounce total time
    [SerializeField] float bouncePeak     = 1.20f; // overshoot scale at peak

    Button _btn;
    LayoutElement _layout;
    BottomBarButtonState _state = BottomBarButtonState.Off;
    Coroutine _widthCo, _scaleCo;

    // Label tween state
    CanvasGroup _labelCg;
    Vector2 _labelBasePos;
    Coroutine _labelCo;

    // Icon position tween state
    Vector2 _iconBasePos;
    Coroutine _iconPosCo;

    public string Id => id;
    public BottomTab Tab => tab;
    public bool IsLocked => isLocked;
    public BottomBarButtonState State => _state;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _layout = GetComponent<LayoutElement>();
        if (!animator) animator = GetComponent<Animator>();

        _btn.interactable = !isLocked;
        if (animator) animator.SetBool("Locked", isLocked);

        // baseline size
        _layout.preferredWidth  = normalWidth;
        _layout.preferredHeight = normalWidth; // square base

        // cache icon base pos
        if (icon) _iconBasePos = icon.anchoredPosition;

        // prep label fade/raise
        if (label)
        {
            _labelCg = label.GetComponent<CanvasGroup>();
            if (!_labelCg) _labelCg = label.AddComponent<CanvasGroup>();
            _labelBasePos = (label.transform as RectTransform).anchoredPosition;
            _labelCg.alpha = 0f;      // hidden initially
            label.SetActive(true);    // keep active; we drive alpha/pos
        }

        _btn.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        if (isLocked) { if (animator) animator.SetTrigger("LockedPulse"); return; }
        onClicked?.Invoke();

        var view = GetComponentInParent<BottomBarView>();
        if (view) view.OnButtonPressed(this);
    }

    public void ToggleOn()
    {
        _state = BottomBarButtonState.On;
        SetEmphasis(true);
        if (animator)
        {
            animator.SetBool("Selected", true);
            animator.SetTrigger("On");
        }
    }

    public void ToggleOff()
    {
        if (animator) animator.SetTrigger("Off");
        _state = isLocked ? BottomBarButtonState.Locked : BottomBarButtonState.Off;
        SetEmphasis(false);
        if (animator) animator.SetBool("Selected", false);
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (_btn) _btn.interactable = !locked;
        if (animator) animator.SetBool("Locked", locked);
        _state = locked ? BottomBarButtonState.Locked : BottomBarButtonState.Off;
    }

    // === EMPHASIS WITH TWEEN ===
    public void SetEmphasis(bool on)
    {
        // width tween
        float targetW = on ? emphasizedWidth : normalWidth;
        if (_widthCo != null) StopCoroutine(_widthCo);
        _widthCo = StartCoroutine(TweenWidth(targetW));

        // icon scale + bounce
        float targetScale = on ? iconScaleWhenEmphasized : 1f;
        if (icon)
        {
            if (_scaleCo != null) StopCoroutine(_scaleCo);
            _scaleCo = StartCoroutine(TweenScale(icon, targetScale));

            // subtle pop only when turning ON
            if (on) StartCoroutine(BounceIcon(icon));

            // rise up while ON, return while OFF
            if (_iconPosCo != null) StopCoroutine(_iconPosCo);
            _iconPosCo = StartCoroutine(TweenIconY(on));
        }

        // label fade/rise
        if (label)
        {
            if (_labelCo != null) StopCoroutine(_labelCo);
            _labelCo = StartCoroutine(TweenLabel(on));
        }
    }

    System.Collections.IEnumerator TweenWidth(float target)
    {
        float start = _layout.preferredWidth;

        // pin minWidth to avoid HorizontalLayoutGroup reflow jitters
        _layout.minWidth = start;

        float t = 0f;
        while (t < widthAnimTime)
        {
            t += Time.unscaledDeltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / widthAnimTime));
            float w = Mathf.Lerp(start, target, k);

            _layout.preferredWidth = w;
            _layout.minWidth       = w;

            var row = transform.parent as RectTransform;
            if (row) LayoutRebuilder.MarkLayoutForRebuild(row);

            yield return null;
        }

        _layout.preferredWidth = target;
        _layout.minWidth       = target;
    }


    System.Collections.IEnumerator TweenScale(RectTransform rt, float target)
    {
        Vector3 start = rt.localScale;
        Vector3 end = Vector3.one * target;
        float t = 0f;
        while (t < scaleAnimTime)
        {
            t += Time.unscaledDeltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / scaleAnimTime));
            rt.localScale = Vector3.Lerp(start, end, k);
            yield return null;
        }
        rt.localScale = end;
    }

    // === ICON BOUNCE (pop) ===
    System.Collections.IEnumerator BounceIcon(RectTransform rt)
    {
        if (rt == null) yield break;

        float t = 0f;
        while (t < bounceDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / bounceDuration);

            // up fast to peak, then settle to 1
            float s = (k < 0.5f)
                ? Mathf.Lerp(1f, bouncePeak, k * 2f)
                : Mathf.Lerp(bouncePeak, 1f, (k - 0.5f) * 2f);

            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    // === ICON RISE (position Y) ===
    System.Collections.IEnumerator TweenIconY(bool up)
    {
        if (icon == null) yield break;

        float t = 0f;
        float fromY = icon.anchoredPosition.y;
        float toY   = _iconBasePos.y + (up ? iconRisePixels : 0f);

        while (t < iconRiseTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / iconRiseTime);

            var p = icon.anchoredPosition;
            p.y = Mathf.Lerp(fromY, toY, k);
            icon.anchoredPosition = p;

            yield return null;
        }
        var done = icon.anchoredPosition;
        done.y = toY;
        icon.anchoredPosition = done;
    }

    // === LABEL FADE + RISE ===
    System.Collections.IEnumerator TweenLabel(bool show)
    {
        var rt = label.transform as RectTransform;
        float t = 0f;
        float dur = labelFadeTime;

        float fromA = _labelCg.alpha;
        float toA   = show ? 1f : 0f;

        Vector2 fromP = rt.anchoredPosition;
        Vector2 toP   = _labelBasePos + new Vector2(0f, show ? labelRisePixels : 0f);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            _labelCg.alpha = Mathf.Lerp(fromA, toA, k);
            rt.anchoredPosition = Vector2.Lerp(fromP, toP, k);
            yield return null;
        }

        _labelCg.alpha = toA;
        rt.anchoredPosition = toP;
    }

    // Hover (Editor/PC). 
    public void OnPointerEnter(PointerEventData e)
    {
        if (isLocked) return;
        var view = GetComponentInParent<BottomBarView>();
        if (view) view.OnHover(this, true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (isLocked) return;
        var view = GetComponentInParent<BottomBarView>();
        if (view) view.OnHover(this, false);
    }

    public void OnPointerClick(PointerEventData e) => HandleClick();
}
