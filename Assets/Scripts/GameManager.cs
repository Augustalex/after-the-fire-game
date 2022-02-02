using Core;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public PlayerInventory playerInventory;

    public Island[] islands;

    public PlayerModeController player;
    public Vector3 finishPosition;
    public GameObject partyMode;

    public bool completed = false;
    
    public string victoryText =
        "Yay! You healed the forest! This is the best beetle stew we ever had. We forgive you.";

    private int _islandsCompleted;

    public void OnIslandCompleted(Island island)
    {
        _islandsCompleted += 1;
        
        if (_islandsCompleted >= 4)
        {
            completed = true;
            UIManager.Instance.SetSubtitle(victoryText);
            partyMode.SetActive(true);
            // Set till hog
            // Set hog post till start position
        }
    }
}
