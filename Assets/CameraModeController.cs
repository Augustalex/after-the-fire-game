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

    [SerializeField]
    private CinemachineVirtualCamera telescopeCamera;
    
    public void SetToPlayerCamera()
    {
        telescopeCamera.Priority = 0;
        introCamera.Priority = 0;
        
        playerCamera.Priority = 1;
    }
    
    public void SetToTelescopeCamera()
    {
        introCamera.Priority = 0;
        playerCamera.Priority = 0;
        
        telescopeCamera.Priority = 1;
    }
}
