using System;
using System.Collections;
using System.Collections.Generic;
using quests;
using TMPro;
using UnityEngine;

public class BridgeSign : MonoBehaviour
{
    public BridgeQuestComponent npcBridgeQuest;
    
    public TMP_Text count;
    
    private Bridge _bridge;
    
    void Start()
    {
        _bridge = GetComponentInParent<Bridge>();
    }

    void Update()
    {
        count.text = $"x{npcBridgeQuest.WoodNeeded()}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHog") || other.CompareTag("Player"))
        {
            if (npcBridgeQuest.Completed())
            {
                SfxManager.Instance.PlaySfx("seedPickup", 0.6f);
                _bridge.Toggle();
                SfxManager.Instance.PlaySfx("splash", 0.6f);
                Destroy(gameObject);
            }
        }
    }
}
