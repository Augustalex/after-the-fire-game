using System;
using UnityEngine;

public class SnowParticle : MonoBehaviour
{
    private int _crushed = 0;
    private bool _hitTerrain;
    
    [NonSerialized]
    public bool rigidbodyDead = false;
    [NonSerialized]
    public float rigidbodyKillTimer = 6f;
    
    void Update()
    {
        if (!rigidbodyDead)
        {
            if (rigidbodyKillTimer > 0)
            {
                rigidbodyKillTimer -= Time.deltaTime;
            }
            else
            {
                Destroy(GetComponent<Rigidbody>());
                rigidbodyDead = true;
            }
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if (_crushed > 2) return;
        
        if (!other.collider.CompareTag("Snow") && !other.collider.CompareTag("Terrain"))
        {
            _crushed += 1;
            GetComponent<BoxCollider>().size *= .2f;
        }
    }
}
