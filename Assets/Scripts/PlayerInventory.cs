using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public struct InventoryState
    {
        public int pineCones;
        public int beetles;
        public int logs;
    }
    
    private InventoryState _inventoryState = new InventoryState
    {
        pineCones = 0,
        beetles = 0,
        logs = 0,
    };
    
    public event Action Updated;
    
    private string _cheat;

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

    public int TryGetCones(int n)
    {
        int seedsToReturn = n;

        int cones = GetCones();
        int newSeeds = cones - n;
        if (newSeeds <= 0)
        {
            seedsToReturn = cones;
            UpdatePineConeCount(0);
        }
        else
        {
            UpdatePineConeCount(newSeeds);
        }

        return seedsToReturn;
    }

    public int RemoveCones(int n)
    {
        var newSeeds = GetCones() - n;
        if (newSeeds <= 0)
        {
            newSeeds = 0;
        }
        UpdatePineConeCount(newSeeds);
        
        return newSeeds;
    }

    public void RegisterPickedUpLog()
    {
        UpdateLogCount(GetLogs() + 1);
        TutorialManager.Instance.CollectedLogs();
    }

    public void RegisterPickedUpSeed()
    {
        UpdatePineConeCount(GetCones() + 1);
        TutorialManager.Instance.PickedUpPineCone();
    }

    public void RegisterPickedUpWorm()
    {
        // This will trigger an animation that in the end triggers the method "IncreaseBugWithOne"
        UIManager.Instance.GetABug();
        SfxManager.Instance.PlaySfx("gettingBug");
    }

    public void IncreaseBugWithOne() // This is called by the GetABug animation
    {
        UpdateBeetleCount(GetWorms() + 1);
    }

    public void ConsumeWorm()
    {
        UpdateBeetleCount(GetWorms() - 1);
    }

    public int ConsumeWoodUpToAmount(int amount)
    {
        var currentLogCount = GetLogs();
        var toTake = Math.Min(currentLogCount, amount);
        UpdateLogCount(currentLogCount - toTake);

        return toTake;
    }
    
    public int GetCones()
    {
        return _inventoryState.pineCones;
    }

    public int GetLogs()
    {
        return _inventoryState.logs;
    }
    
    public int GetWorms()
    {
        return _inventoryState.beetles;
    }
    
    private void UpdatePineConeCount(int newCount)
    {
        _inventoryState = new InventoryState
        {
            pineCones = newCount,
            beetles = _inventoryState.beetles,
            logs = _inventoryState.logs
        };
        TriggerUpdateEvent();
    }
    private void UpdateBeetleCount(int newCount)
    {
        _inventoryState = new InventoryState
        {
            pineCones = _inventoryState.pineCones,
            beetles = newCount,
            logs = _inventoryState.logs
        };
        TriggerUpdateEvent();
    }
    private void UpdateLogCount(int newCount)
    {
        _inventoryState = new InventoryState
        {
            pineCones = _inventoryState.pineCones,
            beetles = _inventoryState.beetles,
            logs = newCount
        };
        TriggerUpdateEvent();
    }
    
    private void TriggerUpdateEvent()
    {
        Updated?.Invoke();
    }
}