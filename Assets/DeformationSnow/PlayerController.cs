using System.Linq;
using Cinemachine;
using DeformationSnow;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour, IPlayerInputReceiver
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
    
    private static RaycastHit[] s_HitBuffer = new RaycastHit[16];
    
    void Start()
    {
        _worldLoadCooldown = 1.5f;
        _rigidbody = GetComponent<Rigidbody>();
        _trailParticles = snowParticles.emission;
    }

    public void IntroStun()
    {
        _stunnedCooldown = 10f; // Tweak to aprox. fit the time the player should be unable to move (before entering walk mode) in the intro sequence
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        _jumpThisFrame = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        _sprint = value.isPressed;
    }

    public void OnSwitchMode(InputValue value)
    {
        var grounded = TouchingSnow() || _onIsland;
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
            _stunnedCooldown = 3f; // TODO: Try remove - this probably does not do anything (as this component is disabled until we start rolling again)

            var playerModeController = GetComponentInParent<PlayerModeController>();
            playerModeController.SetToWalkingMode();
        }
    }

    public void OnMenu(InputValue value)
    {
        UIManager.Instance.ToggleMenu();
    }

    void Update()
    {
        if (_worldLoadCooldown > 0f)
        {
            _worldLoadCooldown -= Time.deltaTime;
            return;
        }

        _onIce = OnIce();
        
        AddExtraGravityIfOnIsland();

        if (_inAir)
        {
            TrackIfStillInAir();
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
                HandleJump();
                HandleBoosting();
            }

            HandleMoving();
        }

        if (NotTouchingSnow())
        {
            DisableTrailParticles();
        }
        else
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
        _jumpThisFrame = false;
        if (!_inAir && _inAirLastFrame) _inAirLastFrame = false;
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
        _stunnedCooldown = .5f;
    }

    private bool OnIce()
    {
        RaycastHit hit;
        var dir = Vector3.down;
        
        if(Physics.Raycast(transform.position,dir,out hit,transform.lossyScale.x * .6f))
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
        return _inAir || _onIsland || _onIce || !TouchingSnow();
    }

    private bool TouchingSnow()
    {
        return Physics.OverlapSphere(transform.position, transform.localScale.x).Any(hit => hit.CompareTag("Terrain"));
    }
    
    private void AdjustDrag()
    {
        if (_onIce)
        {
            _rigidbody.drag = 0f;
        }
        else if (_rigidbody.velocity.magnitude > 10f)
        {
            _rigidbody.drag = 2f;
        }
        else if (Boosting())
        {
            _rigidbody.drag = 2f;
        }
        else if (_moving)
        {
            _rigidbody.drag = 2f;
        }
        else
        {
            _rigidbody.drag = 0.1f;
        }
    }

    private bool TouchedGroundThisFrame()
    {
        return _inAirLastFrame && !_inAir;
    }

    private void TrackIfStillInAir()
    {
        _rigidbody.drag = 1f;
        _rigidbody.AddForce(Vector3.down * 100f * Time.deltaTime, ForceMode.Acceleration);

        if (_inAirCooldown > 0)
        {
            _inAirCooldown -= Time.deltaTime;
        }
        else
        {
            var hitGround = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x * .75f);
            if (hitGround) _inAir = false;
        }

        _inAirLastFrame = true;
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

            var shiftBoost = Boosting() ? data.shiftBoost : 0f;
            var minSpeed = 3f;
            var startBoost = (Mathf.Max(0, minSpeed - _rigidbody.velocity.magnitude) / minSpeed) * data.startBoost;
            var inAirPenalty = _inAir ? .3f : 1f;

            _rigidbody.AddForce(
                (IceMovement(direction.normalized * (data.speed + startBoost + shiftBoost))) * Time.deltaTime * inAirPenalty,
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
        else if(GetMoveDirection().magnitude < .25f)
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
            return currentMovement * .4f + Random.insideUnitSphere * Random.Range(100f, 2000f);
        }
        else
        {
            return currentMovement;
        }
        return _onIce ? Random.insideUnitSphere * 1000f : Vector3.zero;
    }

    private Vector3 GetMoveDirection()
    {
        return new Vector3(_move.x, 0, _move.y);
    }

    private void HandleJump()
    {
        var direction = GetMoveDirection();

        if (_jumpThisFrame)
        {
            var grounded = Physics.OverlapSphere(transform.position, transform.localScale.x * .6f).Length > 1;

            if (grounded)
            {
                _inAirCooldown = 1f;
                _inAir = true;

                if (CheatEngine.Instance.Cheating())
                {
                    _rigidbody.AddForce(Vector3.up * data.jumpForce * 2f + direction * 8f, ForceMode.Impulse);
                }
                else
                {
                    _rigidbody.AddForce(Vector3.up * data.jumpForce + direction * 8f, ForceMode.Impulse);
                }
            }
        }
    }

    private void AddExtraGravityIfOnIsland()
    {
        _onIsland = false;
        if (Physics.OverlapSphere(transform.position, 2f).Any(hit => hit.CompareTag("Island")))
        {
            _onIsland = true;
            _rigidbody.AddForce(Vector3.down * 200f * Time.deltaTime, ForceMode.Acceleration);
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

        _rigidbody.AddForce(-_rigidbody.velocity * 1.5f, ForceMode.Impulse);
    }

    public bool Stunned()
    {
        return _stunnedCooldown > 0f;
    }

    public void HitDeadTree()
    {
        HitTree();
    }
}