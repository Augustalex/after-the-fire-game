using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFakeBall : MonoBehaviour
{
    public Transform follow;

    private Vector3 velocity;
    private Rigidbody _ballBody;

    void Start()
    {
        _ballBody = GetComponentInParent<PlayerModeController>().ballRoot.GetComponentInChildren<Rigidbody>();
    }

    public void LateUpdate()
    {
        var smoothTime = .1f - .1f * Mathf.Clamp(_ballBody.velocity.magnitude / 30f, 0f, 1f);
        Debug.Log("mag: " + _ballBody.velocity.magnitude + ", factor: " + (_ballBody.velocity.magnitude / 30f) +
                  " , result: " + smoothTime);
        var actualPosition = follow.position;
        var transposedFollow = new Vector3(actualPosition.x, actualPosition.y, actualPosition.z);

        // Define a target position above and behind the target transform
        Vector3 targetPosition = follow.TransformPoint(new Vector3(0, 0, 0));

        // Smoothly move the camera towards that target position
        var dampenedFlatPosition = Vector3.SmoothDamp(transform.position, transposedFollow, ref velocity, smoothTime * .25f);
        var dampenedPosition = Vector3.SmoothDamp(transform.position, transposedFollow, ref velocity, smoothTime);
        transform.position = new Vector3(
            dampenedFlatPosition.x,
            dampenedPosition.y,
            dampenedFlatPosition.z
        );
        //
        // transform.position = Vector3.Lerp(transform.position, follow.position, Time.deltaTime * 100);
        // transform.rotation = Quaternion.Lerp(transform.rotation, follow.rotation, Time.deltaTime * 100);
        transform.rotation = follow.rotation;
        transform.localScale = follow.localScale;
    }
}