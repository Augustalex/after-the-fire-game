using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int _seeds = 0;
    public TextMeshProUGUI seedText;

    private int _worms = 0;
    public TextMeshProUGUI wormsText;
    private string _cheat;

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

        seedText.text = _seeds.ToString();
        return seedsToReturn;
    }

    private void Update()
    {
        if (CheatEngine.Instance.Cheating())
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RegisterPickedUpWorm();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RegisterPickedUpSeed();
            }
        }
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


    public void RegisterPickedUpSeed()
    {
        _seeds += 1;
        seedText.text = _seeds.ToString();
    }

    public void RegisterPickedUpWorm()
    {
        Debug.Log("PICKED UP WORM");
        _worms += 1;
        wormsText.text = _worms.ToString();
    }

    public int GetWorms()
    {
        return _worms;
    }


    public void ConsumeWorm()
    {
        Debug.Log("ConsumeWorm");
        _worms -= 1;
        wormsText.text = _worms.ToString();
    }
}