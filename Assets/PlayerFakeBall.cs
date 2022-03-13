using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFakeBall : MonoBehaviour
{
    public Transform follow;

    private Vector3 velocity;
    private float yVelocity;
    private float ySmoothingVelocity;
    private float flatSmoothingVelocity;
    private Rigidbody _ballBody;
    private PlayerController _playerController;
    private float _dampedHeightPosition;
    private Vector3 _dampedFlatPosition;
    private float _dampedHeightSmoothTime;
    private float _dampedFlatSmoothTime;

    void Start()
    {
        _ballBody = GetComponentInParent<PlayerModeController>().ballRoot.GetComponentInChildren<Rigidbody>();
        _playerController = GetComponentInParent<PlayerModeController>().ballRoot.GetComponentInChildren<PlayerController>();

        var currentPosition = transform.position;
        _dampedFlatPosition = new Vector3(currentPosition.x, 0, currentPosition.z);
        _dampedHeightPosition = currentPosition.y;
    }

    public void LateUpdate()
    {
        var actualPosition = follow.position;
        var smoothTime = .06f - .03f * Mathf.Clamp(_ballBody.velocity.magnitude / 40f, 0f, 1f);

        var flatSmoothTime = 0f;
        if (_playerController.OnIsland())
        {
            flatSmoothTime = .0f;
        }
        else if (_playerController.InAir() || _playerController.OnIce())
        {
            flatSmoothTime = .0f;
        }
        else if (_dampedHeightPosition < actualPosition.y - 1f)
        {
            flatSmoothTime = .03f;
        }
        else
        {
            flatSmoothTime = Mathf.Clamp(smoothTime, 0f, 1f);
        }
        _dampedFlatSmoothTime = Mathf.SmoothDamp(_dampedFlatSmoothTime, flatSmoothTime, ref flatSmoothingVelocity, .5f); // Smoothing out changing smooth times :)
        var flatActualPosition = new Vector3(actualPosition.x, 0, actualPosition.z);
        _dampedFlatPosition = Vector3.SmoothDamp(_dampedFlatPosition, flatActualPosition, ref velocity, _dampedFlatSmoothTime);

        var heightSmoothTime = 0f;
        if (_playerController.OnIce() || _playerController.OnIsland())
        {
            heightSmoothTime = .01f;
        }
        else if (_playerController.InAir())
        {
            heightSmoothTime = .01f;
        }
        else if (_dampedHeightPosition < actualPosition.y - .5f)
        {
            heightSmoothTime = .1f;
        }
        else
        {
            heightSmoothTime = smoothTime;
        }
        _dampedHeightSmoothTime = Mathf.SmoothDamp(_dampedHeightSmoothTime, heightSmoothTime, ref ySmoothingVelocity, _playerController.InAir() ? 0f : .12f); // Smoothing out changing smooth times :) Except when in air, that should feel VERY direct!
        _dampedHeightPosition = Mathf.SmoothDamp(_dampedHeightPosition, actualPosition.y, ref yVelocity, _dampedHeightSmoothTime, float.PositiveInfinity, Time.deltaTime); 
        
        transform.position = new Vector3(
            _dampedFlatPosition.x,
            _dampedHeightPosition,
            _dampedFlatPosition.z
        );
        transform.rotation = follow.rotation;
        transform.localScale = follow.localScale;
    }
}