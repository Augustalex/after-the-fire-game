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
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }
    }

    private void TurnOn()
    {
        if (_telescopeOn) return;
        
        _telescopeOn = true;
        CameraModeController.Instance.SetToTelescopeCamera();
        _sky.FadeIn();
    }

    public void TurnOff()
    {
        if (!_telescopeOn) return;
        
        _telescopeOn = false;
        CameraModeController.Instance.SetToPlayerCamera();
        _sky.FadeOut();
    }

    // private void OnTriggerStay(Collider other)
    // {
    //     if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
    //     {
    //         other.GetComponentInParent<PlayerModeController>().NearTelescope();
    //     }
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHog"))
        {
            TurnOn();
        }
        else if (other.CompareTag("Player"))
        {
            TurnOff();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHog"))
        {
            TurnOff();
        }
    }
}