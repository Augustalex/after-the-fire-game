using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeInteractionController : MonoBehaviour
{
    private SkyQuad _sky;
    private bool _telescopeOn;

    void Awake()
    {
        _sky = GetComponentInChildren<SkyQuad>();
    }

    private void Start()
    {
        _sky.SetFadedOut();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_telescopeOn)
            {
                _telescopeOn = false;
                CameraModeController.Instance.SetToPlayerCamera();
                _sky.FadeOut();
            }
            else
            {
                _telescopeOn = true;
                CameraModeController.Instance.SetToTelescopeCamera();
                _sky.FadeIn();
            }
        }
    }
}