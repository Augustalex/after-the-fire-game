using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        private const float TimeOffGroundBeforeCountAsHit = .25f;
        private const float CheckDistanceFactor = 3.8f;
        private SphereCollider _collider;
        private bool _grounded = true;
        private bool _lastGrounded = true;
        private float _lastLeftGround;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
        }

        public bool Grounded()
        {
            return _grounded;
        }

        public bool TouchedGroundThisFrame()
        {
            return _lastGrounded != _grounded;
        }

        public bool HitGroundThisFrame()
        {
            var timeSinceLastLeftGround = Time.fixedTime - _lastLeftGround;
            return TouchedGroundThisFrame() && timeSinceLastLeftGround > TimeOffGroundBeforeCountAsHit;
        }

        public float TimeOffGround()
        {
            if (_grounded) return 0f;
            return Time.fixedTime - _lastLeftGround;
        }
        
        public bool LeftGroundThisFrame()
        {
            return _lastGrounded != _grounded;
        }

        private void FixedUpdate()
        {
            _lastGrounded = _grounded;
            _grounded = CheckGrounded();

            if (LeftGroundThisFrame())
            {
                _lastLeftGround = Time.fixedTime;
            }
        }

        private bool CheckGrounded()
        {
            RaycastHit hit;
            var maxDistance = _collider.radius * CheckDistanceFactor;
            return Physics.Raycast(transform.position + (Vector3.up * _collider.radius), -Vector3.up, out hit, maxDistance);
        }

        public bool OffGround()
        {
            return !Grounded() && MinimumTimeOffGroundReached();
        }

        private bool MinimumTimeOffGroundReached()
        {
            return TimeOffGround() > TimeOffGroundBeforeCountAsHit;
        }
    }
}