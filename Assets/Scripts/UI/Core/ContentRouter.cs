// Written with love, light and rainbow :)
// CounterRouter.cs

/*
---------------------------------------------
 ContentRouter.cs
---------------------------------------------
 Author: Anwesh Sahoo  
 Description:  
 A simple and efficient content management router that connects the
 **Bottom Bar UI** with the corresponding content panels (Home, Shop, Maps, etc.).
 It listens to tab selection events from the `BottomBarView` and automatically
 activates or deactivates the appropriate UI panels based on user interaction.

 The script ensures that only one main panel is visible at any time, maintaining
 a clean and organized user interface flow. It also cleanly unregisters event
 listeners when disabled to prevent memory leaks or duplicate calls.

 Usage:  
 Attach this script to an empty GameObject (e.g., “ContentRouter”) in your UI
 hierarchy. Assign the BottomBarView reference and link your individual
 content panels (Home, Shop, Maps, etc.) in the inspector.  
 The `BottomBarView` in our case broadcasts two key events as requested:
 - **ContentActivated (BottomTab)** → when a tab is selected.  
 - **Closed** → when all tabs are deselected.
*/


using UnityEngine;

public class ContentRouter : MonoBehaviour
{
    [SerializeField] BottomBarView bottomBar;
    [SerializeField] GameObject panelHome, panelShop, panelMaps;

    void OnEnable()
    {
        if (bottomBar == null) bottomBar = FindObjectOfType<BottomBarView>(true);
        bottomBar.ContentActivated.AddListener(OnContent); // expects BottomTab
        bottomBar.Closed.AddListener(CloseAll);
    }

    void OnDisable()
    {
        if (bottomBar == null) return;
        bottomBar.ContentActivated.RemoveListener(OnContent);
        bottomBar.Closed.RemoveListener(CloseAll);
    }

    void OnContent(BottomTab tab)   // <-- changed from string to BottomTab
    {
        CloseAll();
        switch (tab)
        {
            case BottomTab.Home:  if (panelHome)  panelHome.SetActive(true);  break;
            case BottomTab.Shop:  if (panelShop)  panelShop.SetActive(true);  break;
            case BottomTab.Maps: if (panelMaps) panelMaps.SetActive(true); break;
            // LockedA/LockedB are ignored here (no panel)
        }
    }

    void CloseAll()
    {
        if (panelHome)  panelHome.SetActive(false);
        if (panelShop)  panelShop.SetActive(false);
        if (panelMaps) panelMaps.SetActive(false);
    }
}
