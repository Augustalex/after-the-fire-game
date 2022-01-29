using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrower : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private PlayerController _controller;
    private bool _maxSizeReached;
    private Vector3 _originalSize;

    void Start()
    {
        _rigidbody = GetComponent<SphereCollider>().attachedRigidbody;
        _controller = GetComponentInParent<PlayerController>();

        _originalSize = transform.localScale;
    }

    void Update()
    {
        if (_controller.Stunned()) return;
        
        if (_rigidbody.velocity.magnitude > 1f)
        {
            var boostFactor = Mathf.Clamp(_controller.BoostJuice() / 12f, 0f, 1f);
            var boostRate = .004f + 60f * boostFactor;
            var growthRate = _controller.Boosting() ? boostRate : .004f;
            
            var maxSize = 2f;
        
            var progress = Mathf.Clamp(transform.localScale.x / maxSize, 0f, 1f);
            var easedProgress = Mathf.Clamp(InCirc(1 - progress), 0f, 1f);
            var toGrow = growthRate * easedProgress * Time.deltaTime;

            if (transform.localScale.x >= maxSize * .98f)
            {
                _maxSizeReached = true;
            }
            else
            {
                _maxSizeReached = false;
            }
            
            transform.localScale += Vector3.one * toGrow;
            if (transform.localScale.x > maxSize)
            {
                transform.localScale = Vector3.one * maxSize;
            }
        }
    }

    public void ReleaseSnow()
    {
        transform.localScale = _originalSize;
    }

    public bool MaxSizeReached()
    {
        return _maxSizeReached;
    }
    
    public static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);

}
