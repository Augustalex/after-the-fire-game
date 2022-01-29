using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphere : MonoBehaviour
{
    public GameObject hog;
    public GameObject ball;
    public PlayerController playerController;
    private PlayerModeController _playerModeController;

    void Start()
    {
        _playerModeController = FindObjectOfType<PlayerModeController>();
    }
    
    void Update()
    {
        var target = _playerModeController.IsSnowBall() ? ball : hog;
        var targetPosition = target.transform.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
    }
}
