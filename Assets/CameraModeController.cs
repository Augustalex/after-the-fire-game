using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Core;
using UnityEngine;

public class CameraModeController : MonoSingleton<CameraModeController>
{

    [SerializeField]
    private CinemachineVirtualCamera introCamera;
    
    [SerializeField]
    private CinemachineVirtualCamera playerCamera;

    public void SetToPlayerCamera()
    {
        introCamera.Priority = 0;
        playerCamera.Priority = 1;
    }
}
