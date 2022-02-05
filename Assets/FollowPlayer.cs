using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        var targetPosition = target.position;
        
        var currentTransform = transform;
        currentTransform.position = new Vector3(
            targetPosition.x,
            currentTransform.position.y,
            targetPosition.z
        );
    }
}