using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignAim : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }
}
