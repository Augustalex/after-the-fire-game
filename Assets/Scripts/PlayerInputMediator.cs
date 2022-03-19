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
    
    public void OnLook(InputValue value)
    {
        _currentPlayerInputReceiver.OnLook(value);
    }

    public void OnJump(InputValue value)
    {
        _currentPlayerInputReceiver.OnJump(value);
    }

    public void OnLongJump(InputValue value)
    {
        _currentPlayerInputReceiver.OnLongJump(value);
    }

    public void OnSprint(InputValue value)
    {
        _currentPlayerInputReceiver.OnSprint(value);
    }

    public void OnSwitchMode(InputValue value)
    {
        _currentPlayerInputReceiver.OnSwitchMode(value);
    }

    public void OnMenu(InputValue value)
    {
        _currentPlayerInputReceiver.OnMenu(value);
    }
}