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
}
