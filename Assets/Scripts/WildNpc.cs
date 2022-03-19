using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WildNpc : MonoBehaviour
{
    private string text = "Got lost? I think I've seen some signs around here, they usually point towards home.";
    private string text2 = "Did you know trees drop pine cones if you hit them with enough snow?";
    private bool _firstTime = true;

    private bool _falling = true;
    private double _fallingCooldown = 10f;
    private string _showingText = "";

    private void Start()
    {
        var rotationY = Random.insideUnitCircle.y;

        var currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(
            currentRotation.x,
            rotationY,
            currentRotation.z
        );
    }

    void Update()
    {
        DestroyRigidbodyWhenFirmlyGrounded();
    }

    private void DestroyRigidbodyWhenFirmlyGrounded()
    {
        if (_falling)
        {
            if (_fallingCooldown > 0f)
            {
                _fallingCooldown -= Time.deltaTime;
            }
            else
            {
                _falling = false;
                Destroy(GetComponent<Rigidbody>());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            other.GetComponentInParent<PlayerModeController>().CloseToNpc();
        }

        if (other.CompareTag("PlayerHog"))
        {
            if (_showingText == "")
            {
                _showingText = Random.value < .5f ? text2 : text;
            }

            RotateTowards(other.gameObject);

            UIManager.Instance.SetSubtitle(_showingText);
        }
    }

    private void RotateTowards(GameObject other)
    {
        var originalRotation = transform.rotation.eulerAngles;

        transform.LookAt(other.transform.position, Vector3.up);
        var newRotation = transform.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(
            originalRotation.x,
            newRotation.y,
            originalRotation.z
        );
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerHog"))
        {
            _showingText = "";
            UIManager.Instance.ClearSubtitle();
        }
    }
}