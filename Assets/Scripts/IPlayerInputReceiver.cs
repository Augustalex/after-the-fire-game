using System.Numerics;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public interface IPlayerInputReceiver
{
    public void OnMove(InputValue value);
    public void OnMoveTouch(Vector2 value);

    public void OnLook(InputValue value);

    public void OnJump(InputValue value);
    public void OnJumpTouch();
    public void OnStopJumpTouch();

    public void OnSprint(InputValue value);
    public void OnSprintStartTouch();
    public void OnSprintEndTouch();

    public void OnSwitchMode(InputValue value);

    public void OnSwitchModeTouch();

    public void OnMenu(InputValue value);

    public void SyncMove(IPlayerInputReceiver receiver);

    public Vector2 GetMove();
    
    void OnRelease(InputValue value);
}