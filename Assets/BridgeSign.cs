using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BridgeSign : MonoBehaviour
{
    private Bridge _bridge;

    public TMP_Text count;

    private int _woodNeeded = 9;
    
    void Start()
    {
        _bridge = GetComponentInParent<Bridge>();
    }

    void Update()
    {
        count.text = $"x{_woodNeeded}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHog"))
        {
            var woodGotten = other.GetComponentInParent<PlayerInventory>().ConsumeWoodUpToAmount(_woodNeeded);
            _woodNeeded -= woodGotten;
            SfxManager.Instance.PlaySfx("seedPickup", 0.6f);
            if (_woodNeeded == 0)
            {
                _bridge.Toggle();
                SfxManager.Instance.PlaySfx("splash", 0.6f);
                Destroy(gameObject);
            }
        }
    }
}
