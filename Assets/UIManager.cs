using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    public TextMeshProUGUI subtitleText;
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
    }

    public void SetSubtitle(string text)
    {
        subtitleText.text = text;
    }


    public void ClearSubtitle()
    {
        _closing = true;
        _closeDialogueCooldown = 3f;
    }
}
