using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphere : MonoBehaviour
{
    public GameObject hog;
    public GameObject ball;
    public PlayerController playerController;
    private PlayerModeController _playerModeController;

    private const float FlatVelocityOffsetScale = .8f;
    private const float HeightVelocityOffsetScale = .25f;
    private const float LookNormalizationScale = .003f;
    private const float LookGlobalScale = 1;
    private readonly Vector3 _lookScale = new Vector3(8, 8, 8);

    private Vector3 velocity;
    private Vector3 flatLookVelocity;
    private float yLookVelocity;
    private Rigidbody _rigidbody;
    private Vector2 _look;

    private Vector3 _currentLookOffset;
    private Vector3 _followPosition;
    private Transform _followTarget;
    private float _yCurrentLookOffset;
    private float _targetLookY;

    void Start()
    {
        _playerModeController = FindObjectOfType<PlayerModeController>();
        _rigidbody = ball.GetComponentInParent<Rigidbody>();

        _currentLookOffset = Vector3.zero;
        _followPosition = transform.position;

        _followTarget = FindObjectOfType<PlayerFakeBall>().transform;
    }

    void LateUpdate()
    {
        // Adjust camera based on Look controls w/ smoothing
        
        // Pan controls
        var lookSmoothTime = 1f;
        var lookOffset = new Vector3(_look.x, 0, -_look.y) * LookNormalizationScale;
        _currentLookOffset = Vector3.SmoothDamp(_currentLookOffset, lookOffset, ref flatLookVelocity, lookSmoothTime);
        // _currentLookOffset = new Vector3( Mathf.Clamp(currentLookOffset.x, -6f, 6f), 0, Mathf.Clamp(currentLookOffset.z, -2f, 18f));

        // Zoom controls
        var lookYDelta = _look.y * LookNormalizationScale * (Time.deltaTime);
        _targetLookY = Mathf.Clamp(_targetLookY + lookYDelta, -.1f, 1f);
        var yLookSmoothTime = .5f;
        _yCurrentLookOffset = Mathf.SmoothDamp(_yCurrentLookOffset, _targetLookY, ref yLookVelocity, yLookSmoothTime);

        // Final Look offset
        var scaledLookOffset =
            Vector3.Scale(new Vector3(_currentLookOffset.x, _yCurrentLookOffset, _currentLookOffset.z),
                _lookScale * LookGlobalScale);

        
        // Follow player w/ smoothing

        // var target = (_playerModeController.IsSnowBall() ? ball : hog).transform;
        var target = _followTarget;

        var smoothTime = .7f;
        var actualPosition = target.position;
        var bodyVelocity = _rigidbody.velocity;
        var flatVelocity = new Vector3(bodyVelocity.x, 0, ModifyZVelocityOffset(bodyVelocity.z)) *
                           FlatVelocityOffsetScale;
        var heightVelocity = bodyVelocity.magnitude * HeightVelocityOffsetScale;
        var transposedFollow =
            new Vector3(actualPosition.x, actualPosition.y + heightVelocity, actualPosition.z) +
            flatVelocity;
        _followPosition = Vector3.SmoothDamp(_followPosition, transposedFollow, ref velocity, smoothTime);

        // Position follow sphere according to both player-following & look-adjustment
        transform.position = _followPosition + scaledLookOffset;
    }

    private float ModifyZVelocityOffset(float bodyVelocityZ)
    {
        var scale = bodyVelocityZ < 0 ? 1.5f : .5f;
        return bodyVelocityZ * scale;
    }

    public void SetLook(Vector2 look)
    {
        _look = look;
    }
}