using System;
using System.Linq;
using Cinemachine;
using Core;
using DeformationSnow;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(GroundCheck))]
public class PlayerBallMover : MonoBehaviour
{
    public PlayerData data;

    public ParticleSystem snowParticles;
    public GameObject groundCollisionParticles;
    public CinemachineImpulseSource _impulseSource;
    public AudioSource movementSfx;
    private float _boostMeter;
    private float _inAirCooldown;
    private bool _inputJump;
    private bool _inputMove;
    private Vector3 _islandNormal;
    private bool _jumpThisFrame;
    private float _jumpTriggeredAt;

    private Vector2 _move;
    private float _moveTime;
    private bool _moving;
    private bool _onIsland;
    private Vector3 _previousPosition;
    private float _releasing;

    private Rigidbody _rigidbody;
    private bool _sprint;
    private bool _startedJumpMotion;
    private double _stillMovingCooldown;
    private float _stunnedCooldown;
    private ParticleSystem.EmissionModule _trailParticles;

    private bool _jumpActionActive;
    private PlayerSize _playerSize;
    private bool _stunned;

    private GroundCheck _groundCheck;

    private void Awake()
    {
        _groundCheck = GetComponent<GroundCheck>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerSize = GetComponent<PlayerSize>();
    }
    
    private void Start()
    {
        _trailParticles = snowParticles.emission;
    }

    private void Update()
    {
        TrackIsInAir();
        AddExtraGravityIfOnIsland();

        if (CheckInAir())
            ApplyInAirEffects();
        else
            AdjustDrag();

        if (HasStunnedCooldown()) _stunnedCooldown -= Time.deltaTime;

        if (!Stunned())
        {
            if (Grounded()) HandleBoosting();

            HandleJump();
            HandleHovering();
            HandleMoving();
        }

        var showSnowReleaseParticleEffect = _releasing > 0f && _playerSize.HasSnow();
        if (showSnowReleaseParticleEffect || (_groundCheck.TouchingGroundType(GroundCheck.GroundType.Snow) && !_onIsland))
            EnableTrailParticles();
        else
            DisableTrailParticles();

        if (_groundCheck.HitGroundThisFrame())
        {
            if (!_onIsland) TriggerHitGroundParticles();

            _impulseSource.GenerateImpulse();
        }

        _previousPosition = transform.position;
    }

    private void OnCollisionStay(Collision other)
    {
        _islandNormal = other.GetContact(0).normal;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.relativeVelocity.y > 6f)
        {
            SfxManager.Instance.PlaySfxWithPitch("collideWithTree",
                other.relativeVelocity.y * 0.01f,
                .2f);
        }
    }

    public void Stun()
    {
        _stunned = true;
    }
    
    public void ClearStun()
    {
        _stunned = false;
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

    private void AdjustDrag()
    {
        if (_groundCheck.TouchingGroundType(GroundCheck.GroundType.Ice))
            _rigidbody.drag = data.onIceDrag;
        else if (_groundCheck.TouchingGroundType(GroundCheck.GroundType.Ice) && _rigidbody.velocity.magnitude > data.highSpeedDragVelocityThreshold)
            _rigidbody.drag = data.highSpeedDrag;
        else if (Boosting())
            _rigidbody.drag = data.boostDrag;
        else if (_moving)
            _rigidbody.drag = data.movingDrag;
        else
            _rigidbody.drag = data.stillDrag;
    }

    private void TrackIsInAir()
    {
        if (_inAirCooldown > 0)
        {
            _inAirCooldown -= Time.deltaTime;
        }
    }

    private bool CheckInAir()
    {
        return _groundCheck.OffGround();
    }
    
    private void ApplyInAirEffects()
    {
        var inAirDuration = _groundCheck.TimeOffGround();
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
            var inAirPenalty = _groundCheck.OffGround() ? .3f : 1f;

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
        if (_groundCheck.TouchingGroundType(GroundCheck.GroundType.Ice))
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
        {
            if (Grounded())
            {
                _startedJumpMotion = true;

                var force = Vector3.up * (data.jumpForce * .75f) + direction * (.25f * data.jumpDirectionalPush);
                _rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }

        if (_startedJumpMotion)
        {
            if (timeJumping < .3f)
            {
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
        if (_releasing > 0f && _groundCheck.OffGround() && _playerSize.HasSnow())
        {
            var force = Vector3.up * 2750f * Time.deltaTime;
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
        return _groundCheck.Grounded();
    }

    public bool TouchingSnow()
    {
        return _groundCheck.TouchingGroundType(GroundCheck.GroundType.Snow);
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

    public bool OnIsland()
    {
        return _onIsland;
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