using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Object = UnityEngine.Object;

public class IslandWelcomeScreenController : MonoSingleton<IslandWelcomeScreenController>
{
    public GameObject welcomeScreen;
    private IslandWelcomeScreen _welcomeScreen;
    private float _sameIslandSignCooldownUntil;
    private IslandInfo _lastVisitedIsland;

    void Start()
    {
        _welcomeScreen = welcomeScreen.GetComponent<IslandWelcomeScreen>();
        _welcomeScreen.Done += HideWelcomeScreen;

        welcomeScreen.SetActive(false);
    }

    private void HideWelcomeScreen()
    {
        welcomeScreen.SetActive(false);
    }

    public void Show(IslandInfo island)
    {
        if (welcomeScreen.activeSelf) return;
        if (island == _lastVisitedIsland && Time.time < _sameIslandSignCooldownUntil) return;

        _lastVisitedIsland = island;
        _sameIslandSignCooldownUntil = Time.time + 60;

        welcomeScreen.SetActive(true);
        _welcomeScreen.Play(island);
    }
}