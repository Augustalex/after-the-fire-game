using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogItem : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            other.collider.GetComponentInParent<PlayerInventory>().RegisterPickedUpLog();
            Destroy(gameObject, .1f);
            SfxManager.Instance.PlaySfx("seedPickup", 0.6f); 
        }
    }
}
