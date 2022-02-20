using System;
using UnityEngine;


public class Npc : MonoBehaviour
{
    public string part1CompletedText = "Nice! Now fetch me # cones";
    public string allCompletedText = "Thanks for the # cones! Here's a worm to your pot!";
    public int numberOfConesToFetch = 5;
    public int collectedCones = 0;
    public int leftToCollect = 5;

    public bool islandNpc = true;

    [Serializable]
    private enum State
    {
        idle = 0,
        part1completed = 1,
        allCompleted = 2,
    }

    [SerializeField] private State _currentState = State.idle;
    [HideInInspector] public Island island; // Set's in Island onEnable

    private Animator _animator;
    private bool _collectedReward;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        if (islandNpc)
        {
            _animator.SetBool("IsBall", true);
        }
    }

    private void OnEnable()
    {
        leftToCollect = numberOfConesToFetch;
    }

    public void OnPart1Complete()
    {
        _currentState = State.part1completed;

        _animator.SetBool("IsBall", false);
    }

    private void CheckIfQuestIsCompleted()
    {
        if (_currentState == State.part1completed && collectedCones >= numberOfConesToFetch)
        {
            _currentState = State.allCompleted;
        }
    }

    private void TryToGetPinesFromPlayer(Collider other)
    {
        var playerInventory = other.GetComponentInParent<PlayerInventory>();

        if (!playerInventory)
        {
            Debug.LogWarning("No Inventory");
            return;
        }

        leftToCollect = numberOfConesToFetch - collectedCones;
        collectedCones += playerInventory.TryGetCones(leftToCollect);
        leftToCollect = numberOfConesToFetch - collectedCones;
        if (leftToCollect <= 0)
        {
            _currentState = State.allCompleted;
            playerInventory.RegisterPickedUpWorm();
            island.OnAllCompleted();
        }
    }

    private void UpdateStateMachine(Collider other)
    {
        CheckIfQuestIsCompleted();

        if (_currentState == State.part1completed)
        {
            TryToGetPinesFromPlayer(other);
            var text = part1CompletedText.Replace("#", leftToCollect.ToString());
            UIManager.Instance.SetSubtitle(text);
        }

        if (_currentState == State.allCompleted)
        {
            if (numberOfConesToFetch == 0 && !_collectedReward)
            {
                _collectedReward = true;
                
                var playerInventory = other.GetComponentInParent<PlayerInventory>();
                playerInventory.RegisterPickedUpWorm();
                island.OnAllCompleted();
            }
            
            var text = allCompletedText.Replace("#", numberOfConesToFetch.ToString());
            UIManager.Instance.SetSubtitle(text);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!islandNpc) return;
        
        if (other.CompareTag("Player") && _currentState == State.idle)
        {
            if (_currentState == State.idle)
            {
                UIManager.Instance.SetSubtitle("[whimpering] Fire, fire!! We need snow!");
            }
            else
            {
                UIManager.Instance.ClearSubtitle();
            }
        }

        if (other.CompareTag("PlayerHog"))
        {
            UpdateStateMachine(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!islandNpc) return;
        
        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            other.GetComponentInParent<PlayerModeController>().CloseToNpc();
        }

        if (other.CompareTag("PlayerHog"))
        {
            UpdateStateMachine(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!islandNpc) return;
        
        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            UIManager.Instance.ClearSubtitle();
        }
    }
}