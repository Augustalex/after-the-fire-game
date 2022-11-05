using System.Linq;
using Cinemachine;
using Core;
using DeformationSnow;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoSingleton<PlayerController>, IPlayerInputReceiver
{
    private Vector2 _rawMove;

    private FollowSphere _followPlayer;
    private PlayerCameraLookController _lookController;
    private Rigidbody _rigidbody;
    private PlayerGrower _playerGrower;
    private PlayerBallMover _playerBallMover;

    private void Awake()
    {
        _playerBallMover = GetComponent<PlayerBallMover>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerGrower =
            GetComponent<PlayerGrower>();
    }

    private void Start()
    {
        _followPlayer =
            FindObjectOfType<FollowSphere>();
        _lookController =
            FindObjectOfType<PlayerCameraLookController>();
    }

    public void OnMove(InputValue value)
    {
        _rawMove = value.Get<Vector2>();
        _playerBallMover.SetMove(_rawMove);
    }

    public void OnMoveTouch(Vector2 value)
    {
        _rawMove = value;
        _playerBallMover.SetMove(value);
    }

    public void OnLook(InputValue value)
    {
        var look = value.Get<Vector2>();
        _followPlayer.SetLook(look);
        _lookController.SetLook(look);
    }

    public void OnJump(InputValue value)
    {
        var jumpValue = value.Get<float>();
        if (jumpValue > 0f)
            OnJumpAction();
        else
            OnStopJumpAction();
    }

    public void OnJumpTouch()
    {
        OnJumpAction();
    }

    public void OnStopJumpTouch()
    {
        OnStopJumpAction();
    }

    public void OnSprint(InputValue value)
    {
        _playerBallMover.SetSprint(value.isPressed);
    }

    public void OnSprintStartTouch()
    {
        _playerBallMover.SetSprint(true);
    }

    public void OnSprintEndTouch()
    {
        _playerBallMover.SetSprint(false);
    }

    public void OnSwitchMode(InputValue value)
    {
        if (value.isPressed) SwitchModeAction();
    }

    public void OnSwitchModeTouch()
    {
        SwitchModeAction();
    }

    public void OnMenu(InputValue value)
    {
        UIManager.Instance.ToggleMenu();
    }

    public void SyncMove(IPlayerInputReceiver receiver)
    {
        _rawMove = receiver.GetMove();
    }

    public Vector2 GetMove()
    {
        return _rawMove;
    }

    public void OnRelease(InputValue value)
    {
        var releasing = 0f;

        var force = value.Get<float>();
        if (force > .1f)
            releasing = force;

        _playerBallMover.SetReleasing(releasing);
    }

    private void OnJumpAction()
    {
        if (!_playerBallMover.JumpingActionActive())
        {
            _playerBallMover.StartJump();
        }
    }

    private void OnStopJumpAction()
    {
        _playerBallMover.StopJump();
    }

    public void SwitchModeAction()
    {
        var onIsland = _playerBallMover.OnIsland();
        if (!_playerBallMover.Boosting() && _playerBallMover.Grounded())
        {
            if (!onIsland)
            {
                if (_playerGrower.GrowthProgress() > .25f) _playerBallMover.TriggerHitGroundParticles();

                _playerGrower.ReleaseSnow();
            }

            _rigidbody.velocity = Vector3.zero;

            var playerModeController = GetComponentInParent<PlayerModeController>();
            playerModeController.SetToWalkingMode();
        }
    }

    public void PrepareForStopRolling()
    {
        if (_playerGrower.GrowthProgress() > .25f) _playerBallMover.TriggerHitGroundParticles();

        _playerGrower.ReleaseSnow();
    }

    public void PrepareForStartRolling()
    {
        if (!_playerBallMover.OnIsland()) _playerBallMover.TriggerHitGroundParticles();
    }
}