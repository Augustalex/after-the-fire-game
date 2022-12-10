using System;
using quests;
using UnityEngine;

[RequireComponent(typeof(IslandQuests))]
public class Npc : MonoBehaviour
{
    public string part1CompletedText = "Nice! Now fetch me # cones";
    public string allCompletedText = "Thanks for the # cones! Here's a worm to your pot!";
    public int numberOfConesToFetch = 5;

    public bool islandNpc = true;
    
    [Serializable]
    private enum State
    {
        IslandOnFire = 0,
        NeedMoreTrees = 1,
        AllCompleted = 2,
    }

    [HideInInspector] public Island island; // Set's in Island onEnable

    private State _currentState = State.IslandOnFire;

    private Animator _animator;
    private bool _collectedReward;
    private IslandQuests _islandQuests;

    private const string WhimperingFireText = "[whimpering] Fire, fire!! We need snow!";

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _islandQuests = GetComponent<IslandQuests>();

        if (islandNpc)
        {
            _animator.SetBool("IsBall", true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!islandNpc) return;

        if (_currentState == State.IslandOnFire)
        {
            if (other.CompareTag("Player"))
            {
                if (_currentState == State.IslandOnFire)
                {
                    UIManager.Instance.SetSubtitle(WhimperingFireText);
                }
            }
        }
        else
        {
            if (other.CompareTag("PlayerHog"))
            {
                NpcCamera.Instance.FocusOnNpc(transform);
                UpdateStateMachine(other);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!islandNpc) return;

        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            other.GetComponentInParent<PlayerModeController>().CloseToNpc();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!islandNpc) return;

        
        if (_currentState == State.IslandOnFire)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
            {
                ExitDialog();
            }   
        }
        else
        {
            if (other.CompareTag("PlayerHog"))
            {
                ExitDialog();
            }  
        }
    }

    public void OnPart1Complete()
    {
        _currentState = State.NeedMoreTrees;

        _animator.SetBool("IsBall", false);
    }
    
    private void CheckIfQuestIsCompleted()
    {
        if (_currentState == State.NeedMoreTrees && LeftToCollect() == 0)
        {
            _currentState = State.AllCompleted;
        }
    }

    private void UpdateStateMachine(Collider other)
    {
        CheckIfQuestIsCompleted();

        if (_currentState == State.NeedMoreTrees)
        {
            var text = part1CompletedText.Replace("#", LeftToCollect().ToString());
            UIManager.Instance.SetSubtitle(text);
        }
        else if (_currentState == State.AllCompleted && !_collectedReward)
        {
            var playerInventory = other.GetComponentInParent<PlayerInventory>();
            CollectReward(playerInventory);
            
            ShowAllQuestsCompletedText();
        }
        else if (_islandQuests.HasQuest())
        {
            var playerInventory = other.GetComponentInParent<PlayerInventory>();
            _islandQuests.InteractWithOngoingQuest(playerInventory);
        }
        else
        {
            ShowAllQuestsCompletedText();
        }
    }

    private void CollectReward(PlayerInventory playerInventory)
    {
        _collectedReward = true;
        playerInventory.RegisterPickedUpWorm();
        GameManager.Instance.OnIslandCompleted();
    }

    private void ShowAllQuestsCompletedText()
    {
        var text = allCompletedText.Replace("#", numberOfConesToFetch.ToString());
        UIManager.Instance.SetSubtitle(text);
    }

    private int LeftToCollect()
    {
        return Mathf.Max(0, numberOfConesToFetch - island.TreesGrown());
    }
    
    public int GetNumberOfCompletedQuests()
    {
        return _collectedReward ? 1 : 0;
    }

    public void ForceExitDialog()
    {
        ExitDialog();    
    }

    private void ExitDialog()
    {
        NpcCamera.Instance.Reset();
        UIManager.Instance.ClearSubtitle();
    }
}