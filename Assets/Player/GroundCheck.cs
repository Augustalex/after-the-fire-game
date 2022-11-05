using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        public LayerMask groundLayers;
        
        private const float TimeOffGroundBeforeCountAsHit = .25f;
        private const float CheckDistanceFactor = 3.8f;
        private SphereCollider _collider;
        private bool _grounded = true;
        private bool _lastGrounded = true;
        private float _lastLeftGround;
        
        private GroundType _currentGroundType = GroundType.Misc;
        
        public enum GroundType
        {
            Snow,
            Ice,
            Misc
        }

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
        }

        public GroundType CurrentGroundType()
        {
            return _currentGroundType;
        }

        public bool TouchingGroundType(GroundType groundType)
        {
            return !OffGround() && CurrentGroundType() == groundType;
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

        public bool OffGround()
        {
            return !Grounded() && MinimumTimeOffGroundReached();
        }

        private void FixedUpdate()
        {
            _lastGrounded = _grounded;
            
            var groundHit = CheckGrounded();
            if (groundHit.HasValue) UpdateGroundType(groundHit.Value);
            _grounded = groundHit.HasValue;
            
            if (LeftGroundThisFrame())
            {
                _lastLeftGround = Time.fixedTime;
            }
        }

        private void UpdateGroundType(RaycastHit hit)
        {
            if (hit.collider.CompareTag("Ice"))
            {
                _currentGroundType = GroundType.Ice;
            }
            else if (hit.collider.CompareTag("Terrain"))
            {
                _currentGroundType = GroundType.Snow;
            }
            else
            {
                _currentGroundType = GroundType.Misc;
            }
        }

        private RaycastHit? CheckGrounded()
        {
            RaycastHit hit;
            var maxDistance = _collider.radius * CheckDistanceFactor;
            var ray = new Ray(transform.position + (Vector3.up * _collider.radius),  -Vector3.up);
            if (Physics.Raycast(ray, out hit, maxDistance, groundLayers))
            {
                return hit;
            }
            else
            {
                return null;
            }
        }

        private bool MinimumTimeOffGroundReached()
        {
            return TimeOffGround() > TimeOffGroundBeforeCountAsHit;
        }
    }
}