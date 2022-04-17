using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform follow;

    void Update()
    {
        var currentPosition = transform.position;
        transform.position = new Vector3(
            follow.position.x,
            currentPosition.y,
            follow.position.z
        );
    }
}