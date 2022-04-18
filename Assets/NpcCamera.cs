using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Core;
using UnityEngine;

public class NpcCamera : MonoSingleton<NpcCamera>
{
    private CinemachineVirtualCamera _camera;
    private Transform _npcTransform;

    private void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
    }

    public void FocusOnNpc(Transform npc)
    {
        CameraModeController.Instance.SetToNpcCamera();
        _camera.m_Follow = npc;
        _camera.m_LookAt = npc;
    }

    public void Reset()
    {
        CameraModeController.Instance.SetToPlayerCamera();
    }
}