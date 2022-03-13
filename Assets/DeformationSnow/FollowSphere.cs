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
    private Rigidbody _rigidbody;

    void Start()
    {
        _playerModeController = FindObjectOfType<PlayerModeController>();
        _rigidbody = ball.GetComponentInParent<Rigidbody>();
    }
    
    void Update()
    {
        var target = _playerModeController.IsSnowBall() ? ball : hog;
        // var targetPosition = target.transform.position;
        // transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);

        var follow = target.transform;
        
        var smoothTime = .25f;
        
        var actualPosition = follow.position;
        var velocity = _rigidbody.velocity;
        var flatVelocity = new Vector3(velocity.x, 0, velocity.z) * .25f;
        var transposedFollow = new Vector3(actualPosition.x, actualPosition.y + _rigidbody.velocity.magnitude * .25f, actualPosition.z) + flatVelocity;

        // Define a target position above and behind the target transform
        Vector3 targetPosition = follow.TransformPoint(new Vector3(0, 0, 0));
     
        // Smoothly move the camera towards that target position
        var dampenedPosition = Vector3.SmoothDamp(transform.position, transposedFollow, ref velocity, smoothTime);
        transform.position = new Vector3(
            dampenedPosition.x,
            dampenedPosition.y,
            dampenedPosition.z
        );
    }
}
