using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int _seeds = 0;
    public TextMeshProUGUI seedText;

    [SerializeField] private int _worms = 0;
    public TextMeshProUGUI wormsText;

    private int _logs;

    private string _cheat;
    private bool _hasGottenFirstSeed;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            RegisterPickedUpLog();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RegisterPickedUpWorm();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            RegisterPickedUpSeed();
        }

        if (CheatEngine.Instance.Cheating())
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RegisterPickedUpWorm();
                GameManager.Instance.OnIslandCompleted();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RegisterPickedUpSeed();
            }
        }
    }

    public int GetCones()
    {
        return _seeds;
    }

    public int TryGetCones(int n)
    {
        int seedsToReturn = n;

        int newSeeds = _seeds - n;
        if (newSeeds <= 0)
        {
            seedsToReturn = _seeds;
            _seeds = 0;
        }
        else
        {
            _seeds = newSeeds;
        }

        // seedText.text = _seeds.ToString();
        return seedsToReturn;
    }

    public int RemoveCones(int n)
    {
        _seeds -= n;
        if (_seeds <= 0)
        {
            _seeds = 0;
        }

        return _seeds;
    }

    public int GetLogs()
    {
        return _logs;
    }

    public void RegisterPickedUpLog()
    {
        _logs += 1;
    }

    public void RegisterPickedUpSeed()
    {
        if (!_hasGottenFirstSeed)
        {
            _hasGottenFirstSeed = true;

            UIManager.Instance.ShowTemporarySubtitle("~ Press I/SELECT to open inventory ~");
        }

        _seeds += 1;
        seedText.text = _seeds.ToString();
    }

    public void RegisterPickedUpWorm()
    {
        UIManager.Instance.GetABug();
        SfxManager.Instance.PlaySfx("gettingBug");
    }

    // This is called by the GetABug animation
    public void IncreaseBugWithOne()
    {
        _worms += 1;
        wormsText.text = _worms.ToString();
    }

    public void ConsumeWorm()
    {
        Debug.Log("ConsumeWorm");
        _worms -= 1;
        wormsText.text = _worms.ToString();
    }

    public int GetWorms()
    {
        return _worms;
    }

    public int ConsumeWoodUpToAmount(int amount)
    {
        var toTake = Math.Min(_logs, amount);
        _logs -= toTake;

        return toTake;
    }
}