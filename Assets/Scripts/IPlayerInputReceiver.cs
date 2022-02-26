using UnityEngine.InputSystem;

public interface IPlayerInputReceiver
{
    public void OnMove(InputValue value);

    public void OnJump(InputValue value);

    public void OnSprint(InputValue value);

    public void OnSwitchMode(InputValue value);
    
    public void OnMenu(InputValue value);

}