using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphere : MonoBehaviour
{
    public GameObject hog;
    public GameObject ball;
    public PlayerController playerController;
    private PlayerModeController _playerModeController;

    private Vector3 velocity;
    private Vector3 velocity2;
    private Rigidbody _rigidbody;
    private Vector2 _look;

    private Vector3 _currentLookOffset;
    private Vector3 _followPosition;
    private Transform _followTarget;

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
        // Adjust based on Look controls w/ smoothing
        var lookSmoothTime = 1f;
        var lookOffset = new Vector3(
            _look.x,
            _look.y,
            -_look.y * 2f
            ) * .02f;
        var currentLookOffset = Vector3.SmoothDamp(_currentLookOffset, lookOffset, ref velocity2, lookSmoothTime);
        _currentLookOffset = new Vector3(
            Mathf.Clamp(currentLookOffset.x, -6f, 6f),
            Mathf.Clamp(currentLookOffset.y, -1f, 36f),
            Mathf.Clamp(currentLookOffset.z, -2f, 18f)
        );
        Debug.Log("offset: " + _currentLookOffset);
        
        // Follow player w/ smoothing
        
        // var target = (_playerModeController.IsSnowBall() ? ball : hog).transform;
        var target = _followTarget;
        
        var smoothTime = .8f;
        var actualPosition = target.position;
        var bodyVelocity = _rigidbody.velocity;
        var flatVelocity = new Vector3(bodyVelocity.x, 0, bodyVelocity.z) * .25f;
        var transposedFollow = new Vector3(actualPosition.x, actualPosition.y + bodyVelocity.magnitude * .25f, actualPosition.z) + flatVelocity;
        _followPosition = Vector3.SmoothDamp(_followPosition, transposedFollow, ref velocity, smoothTime);
        
        // Position follow sphere according to both player-following & look-adjustment
        transform.position = _followPosition + _currentLookOffset;
    }

    public void SetLook(Vector2 look)
    {
        _look = look;
    }
}
