using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Core;
using UnityEngine;

public class CameraModeController : MonoSingleton<CameraModeController>
{
    [SerializeField] private CinemachineVirtualCamera introCamera;

    [SerializeField] private CinemachineVirtualCamera playerCamera;

    [SerializeField] private CinemachineVirtualCamera telescopeCamera;

    public void SetToPlayerCamera()
    {
        if (telescopeCamera)
        {
            telescopeCamera.Priority = 0;
        }

        if (introCamera)
        {
            introCamera.Priority = 0;
        }

        playerCamera.Priority = 1;
    }

    public void SetToTelescopeCamera()
    {
        if (introCamera)
        {
            introCamera.Priority = 0;
        }

        if (playerCamera)
        {
            playerCamera.Priority = 0;
        }

        telescopeCamera.Priority = 1;
    }
}