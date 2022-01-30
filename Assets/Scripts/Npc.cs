using System;
using UnityEngine;


public class Npc : MonoBehaviour
{
    public string part1CompletedText = "Nice! Now fetch me 5 cones";
    public string allCompletedText = "Thanks! Here's a worm to your pot!";
    public int numberOfConesToFetch = 5;
    public int collectedCones = 0;

    [SerializeField] private bool _inside;

    [Serializable]
    private enum State
    {
        idle= 0,
        part1completed = 1,
        allCompleted = 2,
    }

    [SerializeField] private State _currentState = State.idle;
    
    
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
        Debug.Log("Tru to get pines from player");
    }

    private void UpdateStateMachine(Collider other)
    {
        CheckIfQuestIsCompleted();
            
        if (_currentState == State.part1completed)
        {
            UIManager.Instance.SetSubtitle(part1CompletedText);
            TryToGetPinesFromPlayer(other);
        }
        if (_currentState == State.allCompleted)
        {
            UIManager.Instance.SetSubtitle(allCompletedText);
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
