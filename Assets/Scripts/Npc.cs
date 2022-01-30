using System;
using UnityEngine;


public class Npc : MonoBehaviour
{
    public string part1CompletedText = "Nice! Now fetch me # cones";
    public string allCompletedText = "Thanks for the # cones! Here's a worm to your pot!";
    public int numberOfConesToFetch = 5;
    public int collectedCones = 0;
    public int leftToCollect = 5;

    [SerializeField] private bool _inside;

    [Serializable]
    private enum State
    {
        idle = 0,
        part1completed = 1,
        allCompleted = 2,
    }

    [SerializeField] private State _currentState = State.idle;
    [HideInInspector] public Island island; // Set's in Island onEnable

    private void OnEnable()
    {
        leftToCollect = numberOfConesToFetch;
    }


    public void OnPart1Complete()
    {
        _currentState = State.part1completed;
    }

    private void CheckIfQuestIsCompleted()
    {
        if (collectedCones >= numberOfConesToFetch)
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
            var text = allCompletedText.Replace("#", numberOfConesToFetch.ToString());
            UIManager.Instance.SetSubtitle(text);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHog"))
        {
            _inside = true;

            UpdateStateMachine(other);
        }

        _inside = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHog"))
        {
            UIManager.Instance.ClearSubtitle();
        }
    }
}
