using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMediator : MonoBehaviour
{
    private IPlayerInputReceiver _currentPlayerInputReceiver;

    public void SetInputReceiver(IPlayerInputReceiver receiver)
    {
        _currentPlayerInputReceiver = receiver;
    }
    
    public void OnMove(InputValue value)
    {
        _currentPlayerInputReceiver.OnMove(value);
    }

    public void OnJump(InputValue value)
    {
        _currentPlayerInputReceiver.OnJump(value);
    }

    public void OnSprint(InputValue value)
    {
        _currentPlayerInputReceiver.OnSprint(value);
    }

    public void OnSwitchMode(InputValue value)
    {
        _currentPlayerInputReceiver.OnSwitchMode(value);
    }
}