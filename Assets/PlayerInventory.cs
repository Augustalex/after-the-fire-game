using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int _seeds = 0;

    public void RegisterPickedUpSeed()
    {
        _seeds += 1;
        Debug.Log($"SEEDS: {_seeds}");
    }
}
