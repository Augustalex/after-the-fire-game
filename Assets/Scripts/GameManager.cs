using Core;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public PlayerInventory playerInventory;

    public Island[] islands;

    public PlayerModeController player;
    public Vector3 finishPosition;

    public string victoryText =
        "Yay! You healed the forest! This is the best beetle stew we ever had. We forgive you.";

    public void OnIslandCompleted(Island island)
    {
        if (playerInventory.GetWorms() >= 1)
        {
            UIManager.Instance.SetSubtitle(victoryText);
            player.SetToWalkingMode();
            player.hogRoot.transform.position = finishPosition;
            // Set till hog
            // Set hog post till start position
        }
    }
}
