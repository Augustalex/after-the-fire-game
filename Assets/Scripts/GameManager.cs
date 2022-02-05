using Core;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoSingleton<GameManager>
{
    public PlayerInventory playerInventory;

    public Island[] islands;

    public PlayerModeController player;
    public Vector3 finishPosition;
    public GameObject partyMode;

    private bool _completed;
    public bool Completed
    {
        get => _completed;
        set
        {
            Debug.Log("Game completed: " + value);
            _completed = value;
        }
    }

    public string victoryText =
        "Yay! You healed the forest! This is the best beetle stew we ever had. We forgive you.";

    private int _islandsCompleted;

    public int quest1BeetlesToCollect;

    public int Quest1Progress { get; private set; }

    public int Quest1SetProgress()
    {
        return Quest1Progress++;
    }

    public void OnIslandCompleted()
    {
        _islandsCompleted += 1;
        
        if (_islandsCompleted >= 4)
        {
            Completed = true;
            UIManager.Instance.SetSubtitle(victoryText);
            partyMode.SetActive(true);
            // Set till hog
            // Set hog post till start position
        }
    }

    public void OnIslandCompleted(Island island)
    {
        OnIslandCompleted();
    }
}
