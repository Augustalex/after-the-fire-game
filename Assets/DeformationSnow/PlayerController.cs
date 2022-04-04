using System;
using System.Linq;
using Cinemachine;
using Core;
using DeformationSnow;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoSingleton<PlayerController>, IPlayerInputReceiver
{
    public PlayerData data;

    private Rigidbody _rigidbody;
    private bool _moving;
    private double _stillMovingCooldown;
    private float _boostMeter;

    private Vector2 _move;
    private bool _inAir;
    private float _inAirCooldown;
    private bool _inAirLastFrame;
    private Vector3 _previousPosition;
    private bool _jumpThisFrame;
    private bool _sprint;
    private float _stunnedCooldown;
    [SerializeField] private bool _hitGroundTrigger;
    private bool _onIsland;

    public ParticleSystem snowParticles;
    private ParticleSystem.EmissionModule _trailParticles;
    public GameObject groundCollisionParticles;
    public CinemachineImpulseSource _impulseSource;
    public AudioSource movementSfx;

    private float _worldLoadCooldown;
    private float _moveTime;
    private bool _onIce;

    private float _wentInAirAt;
    private bool _touchingSnow;
    private Vector3 _islandNormal;
    private FollowSphere _followPlayer;
    private PlayerCameraLookController _lookController;
    private float _jumpTriggeredAt;
    private bool _startedJump;

    void Start()
    {
        _worldLoadCooldown = 1.5f;
        _rigidbody = GetComponent<Rigidbody>();
        _trailParticles = snowParticles.emission;

        _followPlayer =
            FindObjectOfType<FollowSphere>(); // TODO: Set reference from Editor? Will there really only be 1 follow sphere ever?
        _lookController =
            FindObjectOfType<PlayerCameraLookController>(); // TODO: Set reference from Editor?
    }

    public void IntroStun()
    {
        _stunnedCooldown =
            10f; // Tweak to aprox. fit the time the player should be unable to move (before entering walk mode) in the intro sequence
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        var look = value.Get<Vector2>();
        _followPlayer.SetLook(look);
        _lookController.SetLook(look);
    }

    public void OnJump(InputValue value)
    {
        // TODO The weirdness with movement is that there is no longer any friction so the speedy initial movement is super fast and weird !!!

        var jumpValue = value.Get<float>();
        if (jumpValue > 0f)
        {
            Debug.Log(Time.time);
            if (_jumpTriggeredAt < 0) _jumpTriggeredAt = Time.time;
        }
        else
        {
            _jumpTriggeredAt = -1f;
        }
    }

    public void OnSprint(InputValue value)
    {
        _sprint = value.isPressed;
    }

    public void OnSwitchMode(InputValue value)
    {
        var grounded = _touchingSnow || _onIsland;
        if (value.isPressed && !Boosting() && grounded)
        {
            if (!_onIsland)
            {
                var playerGrower =
                    GetComponent<PlayerGrower>(); // TODO: Fix circular dependency - Probably the grower shouldn't depend on the movement controller?

                if (playerGrower.GrowthProgress() > .25f)
                {
                    TriggerHitGroundParticles();
                }

                playerGrower.ReleaseSnow();
            }

            _rigidbody.velocity = Vector3.zero;
            _stunnedCooldown =
                3f; // TODO: Try remove - this probably does not do anything (as this component is disabled until we start rolling again)

            var playerModeController = GetComponentInParent<PlayerModeController>();
            playerModeController.SetToWalkingMode();
        }
    }

    public void OnMenu(InputValue value)
    {
        UIManager.Instance.ToggleMenu();
    }

    public void SyncMove(IPlayerInputReceiver receiver)
    {
        _move = receiver.GetMove();
    }

    public Vector2 GetMove()
    {
        return _move;
    }

    void Update()
    {
        if (_rigidbody.velocity.y > 1f)
        {
            Debug.Log("UP: " + Time.time);
        }

        TrackIsTouchingSnow();
        TrackIsInAir();

        if (_worldLoadCooldown > 0f)
        {
            _worldLoadCooldown -= Time.deltaTime;
            return;
        }

        _onIce = CheckIfOnIce();

        AddExtraGravityIfOnIsland();

        if (_inAir)
        {
            ApplyInAirEffects();
        }
        else
        {
            AdjustDrag();
        }

        if (Stunned())
        {
            _stunnedCooldown -= Time.deltaTime;
        }

        if (!Stunned())
        {
            if (!_inAir)
            {
                HandleBoosting();
            }

            HandleJump();
            HandleMoving();
        }

        if (NotTouchingSnow())
        {
            DisableTrailParticles();
        }
        else if (_touchingSnow && !_onIsland && !_inAir)
        {
            EnableTrailParticles();
        }

        if (TouchedGroundThisFrame())
        {
            if (!_onIsland)
            {
                TriggerHitGroundParticles();
            }

            _impulseSource.GenerateImpulse();
        }

        _previousPosition = transform.position;
        if (!_inAir && _inAirLastFrame) _inAirLastFrame = false;
    }

    private void TrackIsTouchingSnow()
    {
        _touchingSnow = TouchingSnow();
    }

    public void PrepareForStopRolling()
    {
        var playerGrower =
            GetComponent<PlayerGrower>(); // TODO: Fix circular dependency - Probably the grower shouldn't depend on the movement controller?

        if (playerGrower.GrowthProgress() > .25f)
        {
            TriggerHitGroundParticles();
        }

        playerGrower.ReleaseSnow();
    }

    public void PrepareForStartRolling()
    {
        if (!_onIsland)
        {
            TriggerHitGroundParticles();
        }

        _stunnedCooldown = 0f;
    }

    public bool CheckIfOnIce()
    {
        RaycastHit hit;
        var dir = Vector3.down;

        if (Physics.Raycast(transform.position, dir, out hit, transform.lossyScale.x * .6f))
        {
            return hit.collider.CompareTag("Ice");
        }

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
        _trailParticles.rateOverTime = (velocityMagnitude < 10f ? 0f : velocityMagnitude * 30f);
        if (velocityMagnitude < 3.5f)
        {
            movementSfx.volume = velocityMagnitude * 0.01f;
        }
        else
        {
            movementSfx.volume = velocityMagnitude * 0.05f;
        }

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

    private bool TouchingSnow()
    {
        return Physics.OverlapSphere(transform.position, transform.localScale.x).Any(hit => hit.CompareTag("Terrain"));
    }

    private void AdjustDrag()
    {
        if (_onIce)
        {
            _rigidbody.drag = data.onIceDrag;
        }
        else if (_touchingSnow && _rigidbody.velocity.magnitude > data.highSpeedDragVelocityThreshold)
        {
            _rigidbody.drag = data.highSpeedDrag;
        }
        else if (Boosting())
        {
            _rigidbody.drag = data.boostDrag;
        }
        else if (_moving)
        {
            _rigidbody.drag = data.movingDrag;
        }
        else
        {
            _rigidbody.drag = data.stillDrag;
        }
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

            if (_inAir && !_inAirLastFrame)
            {
                _wentInAirAt = Time.time;
            }
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
        var go = Instantiate(groundCollisionParticles, this.transform.position, Quaternion.identity);
        Destroy(go, 5f); // TODO: make a safer destory    
    }

    private void HandleMoving()
    {
        var direction = GetMoveDirection();

        if (direction != Vector3.zero)
        {
            _moving = true;

            var islandBoostMultiplier = _onIsland ? 2f : 1f;
            var shiftBoost = Boosting() ? (data.shiftBoost * islandBoostMultiplier) : 0f;
            var minSpeed = data.minSpeed;
            var startBoost = (Mathf.Max(0, minSpeed - _rigidbody.velocity.magnitude) / minSpeed) * data.startBoost;
            var inAirPenalty = _inAir ? .3f : 1f;

            _rigidbody.AddForce(
                (IceMovement(direction.normalized * (data.speed + startBoost + shiftBoost))) * Time.deltaTime *
                inAirPenalty,
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
        {
            _moveTime += Time.deltaTime;
        }
        else if (GetMoveDirection().magnitude < .25f)
        {
            _moveTime = 0f;
        }

        if (Boosting())
        {
            _boostMeter += Time.deltaTime;
        }
        else
        {
            _boostMeter = 0;
        }
    }

    private Vector3 IceMovement(Vector3 currentMovement)
    {
        if (_onIce)
        {
            return currentMovement * data.onIceMovementMultiplier + Random.insideUnitSphere *
                Random.Range(data.onIceMinRandomMotion, data.onIceMaxRandomMotion);
        }
        else
        {
            return currentMovement;
        }
    }

    private Vector3 GetMoveDirection()
    {
        return new Vector3(_move.x, 0, _move.y);
    }

    private void HandleJump()
    {
        var direction = GetMoveDirection();

        var timeJumping = Time.time - _jumpTriggeredAt;
        if (_jumpTriggeredAt > 0 && !_startedJump && _inAirCooldown <= 0f && timeJumping < .4f)
        {
            if (!_inAir)
            {
                _startedJump = true;

                var force = Vector3.up * data.jumpForce * .65f + direction * .44f * data.jumpDirectionalPush;
                _rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }

        if (_startedJump)
        {
            if (timeJumping < .25f)
            {
                _inAir = true;
                _wentInAirAt = Time.time;

                var force = (Vector3.up * data.jumpForce * 200f + direction * 140f * data.jumpDirectionalPush) *
                            Time.deltaTime;
                _rigidbody.AddForce(force, ForceMode.Acceleration);
            }
            else
            {
                _inAirCooldown = 1f;
                _startedJump = false;
            }
        }
    }

    private void AddExtraGravityIfOnIsland()
    {
        _onIsland = false;
        if (Physics.OverlapSphere(transform.position, transform.localScale.x * .75f)
            .Any(hit => hit.CompareTag("Island")))
        {
            _onIsland = true;
            _rigidbody.AddForce(-_islandNormal * data.extraDownwardForceOnIsland * Time.deltaTime,
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
        return _stunnedCooldown > 0f;
    }

    public void HitDeadTree()
    {
        HitTree();
    }

    private void OnCollisionStay(Collision other)
    {
        _islandNormal = other.GetContact(0).normal;
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
}