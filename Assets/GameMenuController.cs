using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

public class GameMenuController : MonoSingleton<GameMenuController>
{
    public GameObject[] views;
    private int _currentViewIndex = 0;

    void Start()
    {
        HideAll();
    }

    public void ToggleGameMenu()
    {
        if (views.Any(v => v.activeSelf))
        {
            HideAll();
        }
        else
        {
            var currentView = views[_currentViewIndex];
            currentView.SetActive(true);

            var visibleInventory = currentView.GetComponentInChildren<Inventory>();
            if (visibleInventory)
            {
                visibleInventory.Refresh();
            }
        }
    }

    private void HideAll()
    {
        foreach (var view in views)
        {
            view.SetActive(false);
        }
    }
}