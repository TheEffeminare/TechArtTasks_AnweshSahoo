// Written with love, light and rainbow :)

/*
---------------------------------------------
 LevelCompleteDriver.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 The **LevelCompleteDriver** orchestrates the full presentation logic, animations, and VFX 
 behavior for the *Level Completed* screen in our game. It elegantly handles showing, animating, 
 and hiding the victory popup — including confetti, particle bursts, score counters, and 
 fade transitions — ensuring the entire moment feels cinematic and rewarding.  

 When the player completes a level, this script:
 1. Fades in the UI and background dimmer.
 2. Triggers celebratory particle effects (confetti, bursts, etc.).
 3. Animates a smooth score count-up.
 4. Enables player interaction after the transition.
 5. Fades out both the UI and VFX smoothly when closed.

 It is designed with **robust control** over Unity’s `ParticleSystem` behavior, ensuring that 
 every re-opening of the screen cleanly resets visuals, prevents ghost particles, and 
 guarantees consistent results even after repeated triggers.

 Usage:  
 - Attach this script to your **UI_LevelCompleted** prefab.  
 - Assign references for `CanvasGroup`, `ScoreText`, `HomeBtn`, and both VFX roots  
   (`Game_Background_Particle` and `Fx`).  
 - Call `Open()` to play the full animation sequence and `Close()` to fade out and reset.  
 - The script also ensures that every particle system and material color resets to its 
   original state upon reopening.
*/


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CanvasGroup))]
public class LevelCompleteDriver : MonoBehaviour
{
    [Header("Hook these")]
    public CanvasGroup cg;
    public Image dimmer;
    public RectTransform content;
    public TextMeshProUGUI scoreText;
    public Button homeBtn;

    [Header("VFX Roots")]
    public GameObject gameBackgroundParticleRoot;   // Game_Background_Particle
    public GameObject fxRoot;                       // Fx

    ParticleSystem[] bgPS, fxPS;
    ParticleSystemRenderer[] bgRenderers, fxRenderers;
    Color[] bgBaseColors, fxBaseColors;

    Animator anim;
    bool isOpen;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (!cg) cg = GetComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        gameObject.SetActive(false);

        if (homeBtn) homeBtn.onClick.AddListener(Close);

        if (gameBackgroundParticleRoot) gameBackgroundParticleRoot.SetActive(false);
        if (fxRoot) fxRoot.SetActive(false);

        CacheVfx(gameBackgroundParticleRoot, out bgPS, out bgRenderers, out bgBaseColors);
        CacheVfx(fxRoot, out fxPS, out fxRenderers, out fxBaseColors);

        DisablePlayOnAwake(bgPS);
        DisablePlayOnAwake(fxPS);
        ForceNoStopDisable(bgPS);
        ForceNoStopDisable(fxPS);
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        gameObject.SetActive(true);

        if (gameBackgroundParticleRoot) gameBackgroundParticleRoot.SetActive(true);
        if (fxRoot) fxRoot.SetActive(true);

        // RE-ARM VFX for every open
        RestoreBaseColors(bgRenderers, bgBaseColors);
        RestoreBaseColors(fxRenderers, fxBaseColors);
        SetEmission(bgPS, true);
        SetEmission(fxPS, true);
        ClearAll(bgPS); ClearAll(fxPS);

        StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        // stop new spawns now; existing particles will fade out
        SetEmission(bgPS, false);
        SetEmission(fxPS, false);

        StartCoroutine(CloseRoutine());
    }

    IEnumerator OpenRoutine()
    {
        yield return FadeCanvas(0f, 1f, 0.18f);
        yield return new WaitForSeconds(0.32f);

        Play(bgPS);
        Play(fxPS);

        yield return Tick(scoreText, 0, 250, 0.6f);

        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    IEnumerator CloseRoutine()
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float dur = 0.35f, e = 0f;

        // ensured that we started fading from full color (or alpha 1) :)
        RestoreBaseColors(bgRenderers, bgBaseColors);
        RestoreBaseColors(fxRenderers, fxBaseColors);

        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            float a = 1f - Mathf.Clamp01(e / dur);
            cg.alpha = a;
            SetRendererAlpha(bgRenderers, bgBaseColors, a);
            SetRendererAlpha(fxRenderers,  fxBaseColors,  a);
            yield return null;
        }
        cg.alpha = 0f;

        StopAndClear(bgPS);
        StopAndClear(fxPS);

        if (gameBackgroundParticleRoot) gameBackgroundParticleRoot.SetActive(false);
        if (fxRoot) fxRoot.SetActive(false);

        gameObject.SetActive(false);
    }

    // ---------- helpers ----------
    void CacheVfx(GameObject root, out ParticleSystem[] psArr,
                  out ParticleSystemRenderer[] rendArr, out Color[] baseColors)
    {
        if (root)
        {
            psArr = root.GetComponentsInChildren<ParticleSystem>(true);
            rendArr = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
            baseColors = new Color[rendArr.Length];
            for (int i = 0; i < rendArr.Length; i++)
            {
                var mat = rendArr[i].sharedMaterial;
                baseColors[i] = (mat && mat.HasProperty("_Color")) ? mat.color : Color.white;
            }
        }
        else { psArr = System.Array.Empty<ParticleSystem>(); rendArr = System.Array.Empty<ParticleSystemRenderer>(); baseColors = System.Array.Empty<Color>(); }
    }

    void DisablePlayOnAwake(ParticleSystem[] arr)
    {
        foreach (var ps in arr)
        {
            var m = ps.main; m.playOnAwake = false;
        }
    }

    // important: do NOT zero rates; just toggle emission.enabled
    void SetEmission(ParticleSystem[] arr, bool enabled)
    {
        foreach (var ps in arr)
        {
            var em = ps.emission;
            em.enabled = enabled;
        }
    }

    void ForceNoStopDisable(ParticleSystem[] arr)
    {
        foreach (var ps in arr)
        {
            var m = ps.main;
            if (m.stopAction == ParticleSystemStopAction.Disable) m.stopAction = ParticleSystemStopAction.None;
        }
    }

    // void Play(ParticleSystem[] arr) { foreach (var ps in arr) if (ps) ps.Play(true); }
    // void Clear(ParticleSystem[] arr) { foreach (var ps in arr) if (ps) ps.Clear(true); }
    // void StopAndClear(ParticleSystem[] arr) { foreach (var ps in arr) if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }

    void Play(ParticleSystem[] arr) { foreach (var ps in arr) if (ps) ps.Play(true); }

    void ClearAll(ParticleSystem[] arr) { foreach (var ps in arr) if (ps) ps.Clear(true); }

    void StopAndClear(ParticleSystem[] arr)
    {
        foreach (var ps in arr)
            if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }


    void RestoreBaseColors(ParticleSystemRenderer[] r, Color[] baseCols)
    {
        int n = Mathf.Min(r.Length, baseCols.Length);
        for (int i = 0; i < n; i++)
        {
            var mat = r[i] ? r[i].material : null;
            if (mat && mat.HasProperty("_Color")) mat.color = baseCols[i];
            else if (mat && mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseCols[i]);
        }
    }
    void SetRendererAlpha(ParticleSystemRenderer[] r, Color[] baseCols, float a)
    {
        int n = Mathf.Min(r.Length, baseCols.Length);
        for (int i = 0; i < n; i++)
        {
            var mat = r[i] ? r[i].material : null;
            if (!mat) continue;
            Color c = baseCols[i]; c.a *= a;
            if (mat.HasProperty("_Color")) mat.color = c;
            else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        }
    }

    IEnumerator FadeCanvas(float a, float b, float t)
    {
        float e = 0f;
        while (e < t) { e += Time.unscaledDeltaTime; cg.alpha = Mathf.Lerp(a, b, e / t); yield return null; }
        cg.alpha = b;
    }

    IEnumerator Tick(TextMeshProUGUI label, int from, int to, float t)
    {
        if (!label) yield break;
        float e = 0f;
        while (e < t)
        {
            e += Time.unscaledDeltaTime;
            int v = Mathf.RoundToInt(Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, e / t)));
            label.text = v.ToString();
            yield return null;
        }
        label.text = to.ToString();
    }
}
