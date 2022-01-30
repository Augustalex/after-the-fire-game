using System;
using UnityEngine;

public class SeedItem : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            other.collider.GetComponentInParent<PlayerInventory>().RegisterPickedUpSeed();
            Destroy(gameObject, .1f);
            SfxManager.Instance.PlaySfx("seedPickup", 0.6f); 
        }
    }
}
