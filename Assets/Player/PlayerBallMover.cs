using System.Linq;
using Cinemachine;
using Core;
using DeformationSnow;
using Player;
using UnityEngine;

public class PlayerBallMover : MonoSingleton<PlayerBallMover>
{
    public PlayerData data;

    public ParticleSystem snowParticles;
    public GameObject groundCollisionParticles;
    public CinemachineImpulseSource _impulseSource;
    public AudioSource movementSfx;
    private float _boostMeter;
    private bool _inAir;
    private float _inAirCooldown;
    private bool _inAirLastFrame;
    private bool _inputJump;
    private bool _inputMove;
    private Vector3 _islandNormal;
    private bool _jumpThisFrame;
    private float _jumpTriggeredAt;

    private Vector2 _move;
    private float _moveTime;
    private bool _moving;
    private bool _onIce;
    private bool _onIsland;
    private Vector3 _previousPosition;
    private float _releasing;

    private Rigidbody _rigidbody;
    private bool _sprint;
    private bool _startedJumpMotion;
    private double _stillMovingCooldown;
    private float _stunnedCooldown;
    private bool _touchingSnow;
    private ParticleSystem.EmissionModule _trailParticles;

    private float _wentInAirAt;

    private float _worldLoadCooldown;
    private bool _jumpActionActive;
    private PlayerSize _playerSize;
    private bool _stunned;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerSize = GetComponent<PlayerSize>();
    }
    
    private void Start()
    {
        _worldLoadCooldown = 1.5f;
        _trailParticles = snowParticles.emission;
    }

    private void Update()
    {
        TrackIsTouchingSnow();
        TrackIsInAir();

        // if (_worldLoadCooldown > 0f)
        // {
        //     _worldLoadCooldown -= Time.deltaTime;
        //     return;
        // }

        _onIce = CheckIfOnIce();

        AddExtraGravityIfOnIsland();

        if (_inAir)
            ApplyInAirEffects();
        else
            AdjustDrag();

        if (HasStunnedCooldown()) _stunnedCooldown -= Time.deltaTime;

        if (!Stunned())
        {
            if (!_inAir) HandleBoosting();

            HandleJump();
            HandleHovering();
            HandleMoving();
        }

        var showSnowReleaseParticleEffect = _releasing > 0f && _playerSize.HasSnow();
        if (showSnowReleaseParticleEffect || (_touchingSnow && !_onIsland && !_inAir))
            EnableTrailParticles();
        else
            DisableTrailParticles();

        // else if (_touchingSnow && !_onIsland && !_inAir) EnableTrailParticles();

        if (TouchedGroundThisFrame())
        {
            if (!_onIsland) TriggerHitGroundParticles();

            _impulseSource.GenerateImpulse();
        }

        _previousPosition = transform.position;
        if (!_inAir && _inAirLastFrame) _inAirLastFrame = false;
    }

    private void OnCollisionStay(Collision other)
    {
        _islandNormal = other.GetContact(0).normal;
    }

    public void Stun()
    {
        _stunned = true;
    }
    
    public void ClearStun()
    {
        _stunned = false;
    }

    private void TrackIsTouchingSnow()
    {
        _touchingSnow = TouchingSnow();
    }

    public bool CheckIfOnIce()
    {
        RaycastHit hit;
        var dir = Vector3.down;

        if (Physics.Raycast(transform.position, dir, out hit, transform.lossyScale.x * .6f))
            return hit.collider.CompareTag("Ice");

        return false;
        // return (RuntimeUtility.RaycastIgnoreTag(new Ray(transform.position, Vector3.down),
        // out RaycastHit hitInfo, 2f, new LayerMask(), "Terrain"));
        // return Physics.OverlapSphere(transform.position, 2f).Any(hit => hit.CompareTag("Ice"));
    }

    private void EnableTrailParticles()
    {
        _trailParticles.enabled = true;

        // < xf ? - dont emit any particle below this value
        // * xf - Emitter multiplier
        var velocityMagnitude = _rigidbody.velocity.magnitude;
        _trailParticles.rateOverTime = _releasing > 0f ? 1000f : (velocityMagnitude < 10f ? 0f : velocityMagnitude * 30f);
        if (velocityMagnitude < 3.5f)
            movementSfx.volume = velocityMagnitude * 0.01f;
        else
            movementSfx.volume = velocityMagnitude * 0.05f;

        movementSfx.pitch = Mathf.Clamp(velocityMagnitude * 0.05f, 0.5f, 1f);
    }

    private void DisableTrailParticles()
    {
        _trailParticles.enabled = false;
        _trailParticles.rateOverTime = 0f;
        movementSfx.volume = 0f;
    }

    private bool NotTouchingSnow()
    {
        return _inAir || _onIsland || _onIce || !_touchingSnow;
    }

    public bool TouchingSnow()
    {
        return Physics.OverlapSphere(transform.position, transform.localScale.x).Any(hit => hit.CompareTag("Terrain"));
    }

    private void AdjustDrag()
    {
        if (_onIce)
            _rigidbody.drag = data.onIceDrag;
        else if (_touchingSnow && _rigidbody.velocity.magnitude > data.highSpeedDragVelocityThreshold)
            _rigidbody.drag = data.highSpeedDrag;
        else if (Boosting())
            _rigidbody.drag = data.boostDrag;
        else if (_moving)
            _rigidbody.drag = data.movingDrag;
        else
            _rigidbody.drag = data.stillDrag;
    }

    private bool TouchedGroundThisFrame()
    {
        return _inAirLastFrame && !_inAir;
    }

    private void TrackIsInAir()
    {
        if (_inAirCooldown > 0)
        {
            _inAirCooldown -= Time.deltaTime;
        }
        else
        {
            var hitGround = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x * .75f);
            _inAir = !hitGround;

            if (_inAir && !_inAirLastFrame) _wentInAirAt = Time.time;
        }

        if (_inAir) _inAirLastFrame = true;
    }

    private void ApplyInAirEffects()
    {
        var inAirDuration = Time.time - _wentInAirAt;
        var inAirTimeMultiplier =
            Mathf.Clamp(1 + Mathf.Pow(inAirDuration * data.gravityMultiplierBase, data.gravityMultiplierGrowthExponent),
                1, data.gravityMultiplierMax);
        var downForce = data.gravity * inAirTimeMultiplier;

        _rigidbody.drag = data.inAirDrag;
        _rigidbody.AddForce(Vector3.down * downForce * Time.deltaTime, ForceMode.Acceleration);
    }

    public void TriggerHitGroundParticles()
    {
        var go = Instantiate(groundCollisionParticles, transform.position, Quaternion.identity);
        Destroy(go, 5f); // TODO: make a safer destory    
    }

    private void HandleMoving()
    {
        var direction = GetMoveDirection();

        if (direction != Vector3.zero)
        {
            _moving = true;

            var islandBoostMultiplier = _onIsland ? 2f : 1f;
            var shiftBoost = Boosting() ? data.shiftBoost * islandBoostMultiplier : 0f;
            var minSpeed = data.minSpeed;
            var startBoost = Mathf.Max(0, minSpeed - _rigidbody.velocity.magnitude) / minSpeed * data.startBoost;
            var inAirPenalty = _inAir ? .3f : 1f;

            _rigidbody.AddForce(
                IceMovement(direction.normalized * (data.speed + startBoost + shiftBoost)) * (Time.deltaTime * inAirPenalty),
                ForceMode.Acceleration);
        }
        else if (_stillMovingCooldown < 0)
        {
            _stillMovingCooldown = .2f;
            _moving = false;
        }
        else
        {
            _stillMovingCooldown -= Time.deltaTime;
        }
    }

    private void HandleBoosting()
    {
        // Regular moveTime - might come to replace boosting
        if (_rigidbody.velocity.magnitude > 2f)
            _moveTime += Time.deltaTime;
        else if (GetMoveDirection().magnitude < .25f) _moveTime = 0f;

        if (Boosting())
            _boostMeter += Time.deltaTime;
        else
            _boostMeter = 0;
    }

    private Vector3 IceMovement(Vector3 currentMovement)
    {
        if (_onIce)
            return currentMovement * data.onIceMovementMultiplier + Random.insideUnitSphere *
                Random.Range(data.onIceMinRandomMotion, data.onIceMaxRandomMotion);
        return currentMovement;
    }

    private Vector3 GetMoveDirection()
    {
        return new Vector3(_move.x, 0, _move.y);
    }

    private void HandleJump()
    {
        var direction = GetMoveDirection();

        var timeJumping = Time.time - _jumpTriggeredAt;
        if (JumpingActionActive() && !_startedJumpMotion && _inAirCooldown <= 0f && timeJumping < .4f)
            if (!_inAir)
            {
                _startedJumpMotion = true;

                var force = Vector3.up * (data.jumpForce * .75f) + direction * (.25f * data.jumpDirectionalPush);
                _rigidbody.AddForce(force, ForceMode.Impulse);
            }

        if (_startedJumpMotion)
        {
            if (timeJumping < .3f)
            {
                _inAir = true;
                _wentInAirAt = Time.time;

                var force = (Vector3.up * (data.jumpForce * 100f) + direction * (200f * data.jumpDirectionalPush)) *
                            Time.deltaTime;
                _rigidbody.AddForce(force, ForceMode.Acceleration);
            }
            else
            {
                _inAirCooldown = 1f;
                _startedJumpMotion = false;
            }
        }
    }

    private void HandleHovering()
    {
        if (_releasing > 0f && _inAir && _playerSize.HasSnow())
        {
            var force = Vector3.up * 2000f * Time.deltaTime;
            _rigidbody.AddForce(force, ForceMode.Force);
        }
    }
    
    private void AddExtraGravityIfOnIsland()
    {
        _onIsland = false;
        if (Physics.OverlapSphere(transform.position, transform.localScale.x * .75f)
            .Any(hit => hit.CompareTag("Island")))
        {
            _onIsland = true;
            _rigidbody.AddForce(-_islandNormal * (data.extraDownwardForceOnIsland * Time.deltaTime),
                ForceMode.Acceleration);
        }
    }

    public Vector3 GetPreviousPosition()
    {
        return _previousPosition;
    }

    public bool Grounded()
    {
        return !_inAir;
    }

    public bool Moving()
    {
        return _moving;
    }

    public bool Boosting()
    {
        return _sprint;
    }

    public float BoostJuice()
    {
        return _boostMeter;
    }

    public float TimeMoving()
    {
        return _moveTime;
    }

    public void ZeroBoostJuice()
    {
        // TODO: Update method name to reflect new boost replacement
        _moveTime = 0f;

        _boostMeter = 0f;
    }

    public void HitTree()
    {
        _stunnedCooldown = data.treeHitStunTime;

        _rigidbody.AddForce(-_rigidbody.velocity * data.hitTreeReturnForceMultiplier, ForceMode.Impulse);
    }

    public bool Stunned()
    {
        return HasStunnedCooldown() || _stunned;
    }

    public bool HasStunnedCooldown()
    {
        return _stunnedCooldown > 0f;
    }

    public void HitDeadTree()
    {
        HitTree();
    }

    public bool InAir()
    {
        return _inAir;
    }

    public bool OnIsland()
    {
        return _onIce;
    }

    public bool OnIce()
    {
        return _onIce;
    }

    public float Releasing()
    {
        return _releasing;
    }

    public void SetSprint(bool sprint)
    {
        _sprint = sprint;
    }

    public void SetReleasing(float releasing)
    {
        _releasing = releasing;
    }

    public bool JumpingActionActive()
    {
        return _jumpActionActive;
    }

    public void StartJump()
    {
        _jumpTriggeredAt = Time.time;
        _jumpActionActive = true;
    }

    public void StopJump()
    {
        _jumpActionActive = false;
    }

    public void SetMove(Vector2 rawMove)
    {
        _move = rawMove;
    }
}