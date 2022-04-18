using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoSingleton<UIManager>
{
    public GameObject startView;

    public GameObject settingsView;

    // public GameObject creditsView;
    public GameObject startButton;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject bugAnim;
    private float _closeDialogueCooldown;
    private bool _closing;

    private void Update()
    {
        if (!_closing) return;

        if (_closeDialogueCooldown > 0f)
        {
            _closeDialogueCooldown -= Time.deltaTime;
        }
        else
        {
            _closing = false;
            subtitleText.text = "";
        }
    }

    private void OnEnable()
    {
        subtitleText.text = "";
        bugAnim.SetActive(false);
    }

    public void SetSubtitle(string text)
    {
        subtitleText.text = text;
    }

    public void ShowTemporarySubtitle(string text)
    {
        subtitleText.text = text;
        _closing = true;
        _closeDialogueCooldown = 6;
    }


    public void ClearSubtitle()
    {
        _closing = true;
        _closeDialogueCooldown = 3f;
    }

    public void GetABug()
    {
        bugAnim.SetActive(true);
    }

    public void InactivateGetABug()
    {
        bugAnim.SetActive(false);
    }

    public void ToggleMenu()
    {
        var eventSystem = EventSystem.current;
        startView.SetActive(!startView.activeSelf);
        settingsView.SetActive(false);
        // creditsView.SetActive(false);
        eventSystem.SetSelectedGameObject(startButton);
    }
}