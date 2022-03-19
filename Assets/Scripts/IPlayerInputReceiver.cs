using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerInputReceiver
{
    public void OnMove(InputValue value);
    
    public void OnLook(InputValue value);

    public void OnJump(InputValue value);
    
    public void OnLongJump(InputValue value);

    public void OnSprint(InputValue value);

    public void OnSwitchMode(InputValue value);
    
    public void OnMenu(InputValue value);

    public void SyncMove(IPlayerInputReceiver receiver);

    public Vector2 GetMove();

}