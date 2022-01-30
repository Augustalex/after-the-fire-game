using System;
using UnityEngine;

public class SeedItem : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            other.collider.GetComponentInChildren<PlayerInventory>().RegisterPickedUpSeed();
            Destroy(gameObject, .1f);
        }
    }
}
