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
        ArtistIsland,
        Island_2,
        Island_3
    }

    public IslandTag islandTag;
    private static List<IslandBeacon> _beacons = new List<IslandBeacon>();
    private IslandInfo _islandInfo;

    private void Awake()
    {
        _beacons.Add(this);
        _islandInfo = GetComponentInParent<IslandInfo>();
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
            IslandWelcomeScreenController.Instance.Show(_islandInfo);
        }

        // }
    }
}