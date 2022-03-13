using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerCameraLookController : MonoBehaviour
{
    private Vector2 _look;
    private CinemachineRecomposer _camera;

    private Vector3 flatLookVelocity;
    private float yLookVelocity;
    private float xLookVelocity;
    private float _yCurrentLookOffset;
    private float _xCurrentLookOffset;
    private const float LookNormalizationScale = .003f;
    
    void Start()
    {
        _camera = GetComponent<CinemachineRecomposer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Adjust camera based on Look controls w/ smoothing
        var targetLookY = Mathf.Clamp(_look.y * LookNormalizationScale, -1f, .05f);
        var yLookSmoothTime = .5f;
        _yCurrentLookOffset = Mathf.SmoothDamp(_yCurrentLookOffset, targetLookY, ref yLookVelocity, yLookSmoothTime);
        
        _camera.m_Tilt = _yCurrentLookOffset * 40f;
              
        // Pan controls
        var lookSmoothTime = 1f;
        var targetLookX = Mathf.Clamp(_look.x * LookNormalizationScale, -1f, 1f);;
        _xCurrentLookOffset = Mathf.SmoothDamp(_xCurrentLookOffset, targetLookX, ref xLookVelocity, yLookSmoothTime);

        _camera.m_Pan = _xCurrentLookOffset * 40f;
    }
    
    public void SetLook(Vector2 look)
    {
        _look = look;
    }
}
