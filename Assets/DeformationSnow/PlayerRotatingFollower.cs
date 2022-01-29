using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotatingFollower : MonoBehaviour
{
    private GameObject _roller;
    private Rigidbody _playerRigidbody;
    private bool _particlesStopped;

    void Start()
    {
        _roller = FindObjectOfType<PlayerController>().gameObject;
        _playerRigidbody = _roller.GetComponent<Rigidbody>();
    }

    void Update()
    {
        var targetPosition = _roller.transform.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);

        var velocity = _playerRigidbody.velocity;
        if (velocity.magnitude > .1f)
        {
            if (_particlesStopped) StartParticles();
            transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        }
        else
        {
            StopParticles();
        }
    }

    private void StopParticles()
    {
        _particlesStopped = true;
    }

    private void StartParticles()
    {
        _particlesStopped = false;
    }
}