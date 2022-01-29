using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrower : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private PlayerController _controller;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _controller = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_rigidbody.velocity.magnitude > 1f)
        {
            var boostFactor = Mathf.Clamp(_controller.BoostJuice() / 12f, 0f, 1f);
            var boostRate = .004f + .08f * boostFactor;
            var growthRate = _controller.Boosting() ? boostRate : .004f;
            
            var maxSize = 4.5f;
        
            var progress = Mathf.Clamp(transform.localScale.x / maxSize, 0f, 1f);
            var easedProgress = Mathf.Clamp(InCirc(1 - progress), 0f, 1f);
            var toGrow = growthRate * easedProgress * Time.deltaTime;

            transform.localScale += Vector3.one * toGrow;
        }
    }
    
    public static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);

}
