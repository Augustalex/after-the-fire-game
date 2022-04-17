using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public bool emptySlot;
    public Texture icon;
    public int count;

    public RawImage iconHolder;
    public TMP_Text textHolder;

    void Start()
    {
        if (emptySlot)
        {
            iconHolder.color = Color.clear;
        }
        else
        {
            iconHolder.texture = icon;
            textHolder.text = count.ToString();
        }
    }
}