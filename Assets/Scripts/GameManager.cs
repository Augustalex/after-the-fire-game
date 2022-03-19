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

    private bool _quest1AllBeetlesCollected;
    public bool Quest1AllBeetlesCollected
    {
        get => _quest1AllBeetlesCollected;
        set
        {
            Debug.Log("Game completed: " + value);
            _quest1AllBeetlesCollected = value;
        }
    }

    private bool _quest1Completed;
    public bool Quest1Completed
    {
        get => _quest1Completed;
        private set
        {
            Debug.Log("Quest1 completed completed: " + value);
            _quest1Completed = value;
        }
    }

    public string victoryText =
        "We got 4 beetles now, let's go finish that stew at home!";

    private int _islandsCompleted;

    public int quest1BeetlesToCollect;

    public int Quest1Progress { get; private set; }

    public int Quest1SetProgress()
    {
        
        Quest1Progress++;
        if (Quest1Progress == quest1BeetlesToCollect)
        {
            Quest1Completed = true;
        }
        return Quest1Progress;
    }

    public void OnIslandCompleted()
    {
        _islandsCompleted += 1;
        
        if (_islandsCompleted >= 4)
        {
            Quest1AllBeetlesCollected = true;
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
