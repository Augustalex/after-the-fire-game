using DeformationSnow;
using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        public PlayerData data;
        public LayerMask groundLayers;
        
        private const float TimeOffGroundBeforeCountAsHit = .25f;
        private const float CheckDistanceFactor = 4f;
        private SphereCollider _collider;
        private bool _grounded = true;
        private bool _lastGrounded = true;
        private float _lastLeftGround;
        
        private GroundType _currentGroundType = GroundType.Misc;
        private float _lastTouchedDown;
        private float _deGroundTriggeredAt;
        private float _groundDistance;
        private float _groundStartHeight;

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
            if (_deGroundTriggeredAt > _lastTouchedDown && _deGroundTriggeredAt > _lastLeftGround)
            {
                var timeSinceDeGround = Time.fixedTime - _deGroundTriggeredAt;
                
                if (Time.fixedTime >= timeSinceDeGround && Time.fixedTime < data.minTimeBetweenJumps) return false; 
            }
            
            return _grounded || _lastGrounded || Time.fixedTime - _lastTouchedDown < data.coyoteTime;
        }

        public float GroundDistance()
        {  
            var origin = transform.position + (Vector3.up * _collider.radius);
            var ray = new Ray(origin,  -Vector3.up);

            var hits = Physics.RaycastAll(ray, 1000f, groundLayers);
            
            var distance = 1000f;
            foreach (var raycastHit in hits)
            {
                if (raycastHit.distance < distance) distance = raycastHit.distance;
            }

            return distance;
        }

        public float GroundStartHeight()
        {
            return _groundStartHeight;
        }

        public void DebugThing()
        {
            Debug.Log("GROUNDED: " + _grounded + ", LASTGROUNDED: " + _lastGrounded + ", TIME SINCE:  " + (Time.fixedTime - _lastTouchedDown));
        }

        // Triggered when User requests a jump - prevents Jump spamming that abuses Coyote time
        public void DeGround()
        {
            _deGroundTriggeredAt = Time.fixedTime;
        }

        public bool TouchedGroundThisFrame()
        {
            return !_lastGrounded && _grounded;
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
            return _lastGrounded && !_grounded;
        }

        public bool OffGround()
        {
            return !Grounded() && MinimumTimeOffGroundReached();
        }

        public void ManualUpdate()
        {
            _lastGrounded = _grounded;
            
            var groundHit = CheckGrounded();
            if (groundHit.HasValue)
            {
                UpdateGroundType(groundHit.Value);
            }
            _grounded = groundHit.HasValue;
            
            if (LeftGroundThisFrame())
            {
                if (groundHit.HasValue)
                {
                    _groundStartHeight = groundHit.Value.point.y;
                }
                _lastLeftGround = Time.fixedTime;
            }
            else if (TouchedGroundThisFrame())
            {
                _lastTouchedDown = Time.fixedTime;
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

        public float TimeSinceDeGround()
        {
            return Time.fixedTime - _deGroundTriggeredAt;
        }
    }
}