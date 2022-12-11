using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMediator : MonoBehaviour
{
    private IPlayerInputReceiver _currentPlayerInputReceiver;

    public void SetInputReceiver(IPlayerInputReceiver receiver)
    {
        if (_currentPlayerInputReceiver != null)
        {
            receiver.SyncMove(_currentPlayerInputReceiver);
        }

        _currentPlayerInputReceiver = receiver;
    }

    public void OnMove(InputValue value)
    {
        _currentPlayerInputReceiver.OnMove(value);
    }

    public void OnMoveTouch(Vector2 value)
    {
        _currentPlayerInputReceiver.OnMoveTouch(value);
    }

    public void OnJumpTouch()
    {
        _currentPlayerInputReceiver.OnJumpTouch();
    }
    
    public void OnLook(InputValue value)
    {
        _currentPlayerInputReceiver.OnLook(value);
    }

    public void OnJump(InputValue value)
    {
        _currentPlayerInputReceiver.OnJump(value);
    }

    public void OnSprint(InputValue value)
    {
        _currentPlayerInputReceiver.OnSprint(value);
    }

    public void OnRelease(InputValue value)
    {
        _currentPlayerInputReceiver.OnRelease(value);
    }

    public void OnSprintStartTouch()
    {
        _currentPlayerInputReceiver.OnSprintStartTouch();
    }


    public void OnSprintEndTouch()
    {
        _currentPlayerInputReceiver.OnSprintEndTouch();
    }

    public void OnSwitchMode(InputValue value)
    {
        _currentPlayerInputReceiver.OnSwitchMode(value);
    }
    
    public void OnSwitchModeTouch()
    {
        _currentPlayerInputReceiver.OnSwitchModeTouch();
    }

    public void OnMenu(InputValue value)
    {
        _currentPlayerInputReceiver.OnMenu(value);
    }

    public void OnGameMenu(InputValue value)
    {
        GameMenuController.Instance.ToggleGameMenu();
    }

    public IPlayerInputReceiver GetInput()
    {
        return _currentPlayerInputReceiver;
    }
}