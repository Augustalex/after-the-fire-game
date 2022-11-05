using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/* Note: animations are called via the controller for both the character and capsule using animator null checks */

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour, IPlayerInputReceiver
{
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)] [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    // player
    private Vector2 _move;
    private float _speed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private bool _hasAnimator;
    private FollowSphere _followPlayer;
    private PlayerCameraLookController _lookController;
    private bool _stunned;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
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

    private void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();

        _followPlayer =
            FindObjectOfType<FollowSphere>(); // TODO: Set reference from Editor? Will there really only be 1 follow sphere ever?
        _lookController =
            FindObjectOfType<PlayerCameraLookController>(); // TODO: Set reference from Editor?

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        AddExtraGravityIfOnIsland();

        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
        Move();

        if (_controller.velocity.magnitude > 0.01f)
        {
            if (!_animator.GetBool("IsWalking"))
            {
                _animator.SetBool("IsWalking", true);
            }
        }
        else if (_animator.GetBool("IsWalking"))
        {
            _animator.SetBool("IsWalking", false);
        }
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    public void OnMoveTouch(Vector2 value)
    {
        _move = value;
    }

    public void OnLook(InputValue value)
    {
        var look = value.Get<Vector2>();
        _followPlayer.SetLook(look);
        _lookController.SetLook(look);
    }

    public void OnJump(InputValue value)
    {
        // When in walk mode - the player cannot jump
    }

    public void OnJumpTouch()
    {
        // When in walk mode - the player cannot jump
    }

    public void OnStopJumpTouch()
    {
        // When in walk mode - the player cannot jump
    }

    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            GetComponentInParent<PlayerModeController>().SetToBallMode();
        }
    }

    public void OnSprintStartTouch()
    {
        GetComponentInParent<PlayerModeController>().SetToBallMode();
    }

    public void OnSprintEndTouch()
    {
        // Do nothing
    }

    public void OnSwitchMode(InputValue value)
    {
        // When in walk mode - switching happens when player starts moving
    }

    public void OnSwitchModeTouch()
    {
        // When in walk mode - switching happens when player starts moving
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

    public void OnRelease(InputValue value)
    {
        // When in walk mode - cannot release any snow
    }

    private void AddExtraGravityIfOnIsland()
    {
        if (Physics.OverlapSphere(transform.position, 2f).Any(hit => hit.CompareTag("Island")))
        {
            Gravity = -30f;
        }
        else
        {
            Gravity = -15f;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
    }

    private void Move()
    {
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        if(!_stunned)
        {
            if (_move.magnitude > .1f)
            {
                var playerModeController = GetComponentInParent<PlayerModeController>();
                if (playerModeController.CanTurnToBallRightNow())
                {
                    playerModeController.SetToBallMode();
                }
            }
        }
    }

    private void OnEnable()
    {
        _move = Vector2.zero;
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    // _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }
}