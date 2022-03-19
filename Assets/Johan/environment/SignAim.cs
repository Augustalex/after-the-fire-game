using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignAim : MonoBehaviour
{
    public IslandBeacon.IslandTag targetTag;

    private void Start()
    {
        var target = IslandBeacon.GetWithTag(targetTag);
        if (!target) return;
        
        Vector3 direction = target.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);

        var currenRotation = transform.rotation;
        transform.rotation = new Quaternion(
            currenRotation.x,
            rotation.y,
            currenRotation.z,
            currenRotation.w
            );
    }
}
