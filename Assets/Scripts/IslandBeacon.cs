using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IslandBeacon : MonoBehaviour
{
    public enum IslandTag
    {
        Home,
        Island_1,
        Island_2,
        Island_3
    }

    public IslandTag islandTag;
    private static List<IslandBeacon> _beacons = new List<IslandBeacon>();

    private void Awake()
    {
        _beacons.Add(this);
    }

    public static GameObject GetWithTag(IslandTag islandTag)
    {
        return _beacons.FirstOrDefault(b => b != null && b.islandTag == islandTag)?.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (islandTag != IslandTag.Home)
        // {
            if (other.CompareTag("Player"))
            {
                IslandWelcomeScreenController.Instance.Show(GetIslandName());
            }
        // }
    }

    private string GetIslandName()
    {
        switch (islandTag)
        {
            case IslandTag.Island_1:
                return "Carpenter\nIsland";
            case IslandTag.Island_2:
                return "Island 2";
            case IslandTag.Island_3:
                return "Island 3";
            default:
                return "Island ???";
        }
    }
}