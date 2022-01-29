using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSphere : MonoBehaviour
{
    private GameObject _roller;

    void Start()
    {
        _roller = FindObjectOfType<PlayerController>().gameObject;
    }
    
    void Update()
    {
        var targetPosition = _roller.transform.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
    }
}
