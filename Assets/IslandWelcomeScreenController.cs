using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class IslandWelcomeScreenController : MonoSingleton<IslandWelcomeScreenController>
{
    public GameObject welcomeScreen;
    private IslandWelcomeScreen _welcomeScreen;

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

    public void Show(string text)
    {
        if (welcomeScreen.activeSelf) return;

        welcomeScreen.SetActive(true);
        _welcomeScreen.Play(text);
    }
}