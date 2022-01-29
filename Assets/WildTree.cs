using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class WildTree : MonoBehaviour
{
    public GameObject itemTemplate;
    
    private float _shakeDuration;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Quaternion _zeroRotation;

    private const float SwingTime = .15f;
    private float _currentMaxSwingTime = 0f;
    private float _swingTimeLeft = 0f;
    
    private readonly List<Quaternion> _swings = new List<Quaternion>();

    private void Start()
    {
        _zeroRotation = transform.rotation;
    }

    void Update()
    {
        if (_swings.Count > 0)
        {
            _swingTimeLeft -= Time.deltaTime;
            
            var progress = (_swingTimeLeft) / SwingTime;
            transform.rotation = Quaternion.Slerp(_originalRotation, _swings[0], 1f - progress);

            if (progress <= 0)
            {
                _swings.RemoveAt(0);
                _currentMaxSwingTime = _currentMaxSwingTime * .8f;
                _swingTimeLeft = SwingTime;
                _originalRotation = transform.rotation;    

                if (_swings.Count == 0)
                {
                    transform.rotation = _zeroRotation;
                }
            }
        }
        
        
        // if (_shakeDuration > 0f)
        // {
        //     _shakeDuration -= Time.deltaTime;
        //     if (_shakeDuration <= 0)
        //     {
        //         transform.position = _originalPosition;
        //         transform.rotation = _originalRotation;
        //
        //         DropItem();
        //     }
        //     else
        //     {
        //         transform.rotation = _originalRotation * Quaternion.Euler(RandomShakeOffset());
        //     }
        // }
    }

    private void DropItem()
    {
        Instantiate(itemTemplate, transform.position + Vector3.up * 3f + Random.insideUnitSphere, Random.rotation, null);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player") && _swings.Count == 0)
        {
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            Shake();
        }
    }

    private void Shake()
    {
        _swings.AddRange(new []
        {
            GenerateRandomShakeOffset(1f),
            GenerateRandomShakeOffset(.6f),
            GenerateRandomShakeOffset(.4f),
            GenerateRandomShakeOffset(.2f),
            GenerateRandomShakeOffset(.1f),
            _zeroRotation
        });
        _originalRotation = transform.rotation;
        _currentMaxSwingTime = SwingTime;
        _swingTimeLeft = SwingTime;
        
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();        
        _shakeDuration = .5f;
    }

    private Quaternion GenerateRandomShakeOffset(float scale)
    {
        return _originalRotation * Quaternion.Euler(RandomShakeOffset() * scale);
    }

    private Vector3 RandomShakeOffset()
    {
        return Random.insideUnitSphere.normalized * 5f;
    }
}
