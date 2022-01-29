using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerController : MonoBehaviour
{
    private Vector3 _direction;
    private float _acceleration = 0;
    private Rigidbody _rigidbody;
    private bool _activated;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rigidbody.isKinematic = false;
            _activated = true;
        }
        if (!_activated) return;
        
        var hits = Physics.OverlapSphere(transform.position, 6f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Snow"))
            {
                if (!hit.gameObject.GetComponent<Rigidbody>())
                {
                    var rb = hit.gameObject.AddComponent<Rigidbody>();
                    rb.drag = .1f;
                    rb.interpolation = RigidbodyInterpolation.Extrapolate;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                    var snow = hit.GetComponent<SnowParticle>();
                    snow.rigidbodyDead = false;
                    snow.rigidbodyKillTimer = 10f;
                }
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            _rigidbody.AddForce(-_rigidbody.velocity * .1f, ForceMode.Impulse);
            return;
        }

        if (Input.GetKey(KeyCode.W))
        {
            _acceleration = 600;
        }
        else
        {
            _acceleration = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _direction += Vector3.left * Time.deltaTime * 20f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            _direction += Vector3.right * Time.deltaTime * 20f;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(_direction);
        }

        var _targetDirection = _direction.normalized;
        _rigidbody.AddForce(_acceleration * _targetDirection * Time.deltaTime + Vector3.up * .15f * Time.deltaTime,
            ForceMode.Acceleration);
    }
}