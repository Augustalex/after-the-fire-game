using System;
using UnityEngine;


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

    void Awake()
    {
        _animator = GetComponent<Animator>();

        if (islandNpc)
        {
            _animator.SetBool("IsBall", true);
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

        if (_currentState == State.AllCompleted)
        {
            if (!_collectedReward)
            {
                _collectedReward = true;

                var playerInventory = other.GetComponentInParent<PlayerInventory>();
                playerInventory.RegisterPickedUpWorm();
                GameManager.Instance.OnIslandCompleted();
            }

            var text = allCompletedText.Replace("#", numberOfConesToFetch.ToString());
            UIManager.Instance.SetSubtitle(text);
        }
    }

    private int LeftToCollect()
    {
        return Mathf.Max(0, numberOfConesToFetch - island.TreesGrown());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!islandNpc) return;

        if (other.CompareTag("Player") && _currentState == State.IslandOnFire)
        {
            if (_currentState == State.IslandOnFire)
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

    public int GetNumberOfCompletedQuests()
    {
        return _collectedReward ? 1 : 0;
    }
}