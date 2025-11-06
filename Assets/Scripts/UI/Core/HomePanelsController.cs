// HomePanelsController.cs

using UnityEngine;

public class HomePanelsController : MonoBehaviour
{
    [SerializeField] GameObject panelHome, panelShop, panelMaps;

    public void ShowTab(BottomTab tab)
    {
        CloseAll();
        switch (tab)
        {
            case BottomTab.Home: if (panelHome) panelHome.SetActive(true); break;
            case BottomTab.Shop: if (panelShop) panelShop.SetActive(true); break;
            case BottomTab.Maps: if (panelMaps) panelMaps.SetActive(true); break;
        }
    }

    public void CloseAll()
    {
        if (panelHome) panelHome.SetActive(false);
        if (panelShop) panelShop.SetActive(false);
        if (panelMaps) panelMaps.SetActive(false);
    }
}


