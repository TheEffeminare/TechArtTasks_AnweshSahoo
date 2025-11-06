// Written with love, light and rainbow :)

/*
---------------------------------------------
 PopupDriver.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A modular and reusable popup controller that cleanly manages
 the opening and closing of UI popups with smooth transitions,
 dimmed backgrounds, and proper input handling. It ensures that
 only one popup remains interactive at a time and provides optional
 tap-to-close behavior through a dimmer overlay.

 Usage:  
 Attach this script to your popup root GameObject that has an
 Animator and CanvasGroup. Assign the dimmer background, dimmer
 image (for raycast blocking), and optional dimmer button if you
 want users to tap outside the popup to close it.  
 The popup stays inactive by default and becomes active when
 `Open()` is called — playing the “Open” animation and enabling
 interactivity.  
 When `Close()` is called, it triggers the “Close” animation,
 disables interactivity, hides the dimmer, and finally turns off
 the popup after a short delay.
*/


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CanvasGroup))]
public class PopupDriver : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] GameObject dimmer;     // Dimmer_BG GO (full-screen)
    [SerializeField] Image dimmerImage;     // Its Image (optional but recommended)
    [SerializeField] Button dimmerButton;   // If you want tap-to-close

    Animator anim;
    CanvasGroup cg;
    bool isOpen;

    void Awake()
    {
        anim = GetComponent<Animator>();
        cg   = GetComponent<CanvasGroup>();

        if (dimmerButton)
        {
            dimmerButton.onClick.RemoveAllListeners();
            dimmerButton.onClick.AddListener(Close);
        }

        // Start closed
        SetDimmer(false);
        gameObject.SetActive(false);
        cg.alpha = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        isOpen = false;
    }

    public void Open()
    {
        if (isOpen) return;

        // Bring dimmer up BEFORE enabling popup raycasts
        SetDimmer(true);

        gameObject.SetActive(true);
        anim.ResetTrigger("Close");
        anim.SetTrigger("Open");

        cg.interactable   = true;
        cg.blocksRaycasts = true;

        isOpen = true;
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void Close()
    {
        if (!isOpen) return;

        anim.ResetTrigger("Open");
        anim.SetTrigger("Close");

        // Immediately stop popup from eating clicks
        cg.interactable   = false;
        cg.blocksRaycasts = false;

        // Immediately remove the blocker
        SetDimmer(false);

        // Turn off after the close anim finishes (adjust to your clip length)
        CancelInvoke(nameof(DisableAfterClose));
        Invoke(nameof(DisableAfterClose), 0.25f);

        isOpen = false;
        EventSystem.current?.SetSelectedGameObject(null);
    }

    


    void DisableAfterClose()
    {
        gameObject.SetActive(false);
        // optional cleanup
        anim.ResetTrigger("Open");
        anim.ResetTrigger("Close");
    }


    void SetDimmer(bool on)
    {
        if (dimmer) dimmer.SetActive(on);
        if (dimmerImage) dimmerImage.raycastTarget = on; // critical line
    }
}


