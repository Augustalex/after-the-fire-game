using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int _seeds = 0;
    public TextMeshProUGUI seedText;

    public void RegisterPickedUpSeed()
    {
        _seeds += 1;
        seedText.text = _seeds.ToString();
    }
}
