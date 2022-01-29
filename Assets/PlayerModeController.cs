using System;
using System.Linq;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerModeController : MonoBehaviour
{
    public GameObject ballRoot;
    public GameObject hogRoot;
    private bool _isBall;
    private CharacterController _hogCharacterController;
    private ThirdPersonController _hogThirdPersonController;
    private PlayerInput _hogInput;
    private BoxCollider _hogCollider;
    private PlayerInput _ballInput;
    private Rigidbody _ballRigidbody;
    private SphereCollider _ballCollider;

    private void Awake()
    {
        _hogCharacterController = hogRoot.GetComponent<CharacterController>();
        _hogThirdPersonController = hogRoot.GetComponent<ThirdPersonController>();
        _hogInput = hogRoot.GetComponent<PlayerInput>();
        _hogCollider = hogRoot.GetComponent<BoxCollider>();

        _ballInput = ballRoot.GetComponentInChildren<PlayerInput>();
        _ballRigidbody = ballRoot.GetComponentInChildren<Rigidbody>();
        _ballCollider = ballRoot.GetComponentInChildren<SphereCollider>();

        SetToBallMode();
    }

    void Update()
    {
        if (Random.value < .001f)
        {
            Debug.Log($"OnIsland(): {OnIsland()}");
        }
        
        if (OnIsland())
        {
            SetToWalkingMode();
        }
        else
        {
            SetToBallMode();
        }

        if (_isBall)
        {
            hogRoot.transform.position = ballRoot.transform.position;
            hogRoot.transform.rotation = ballRoot.transform.rotation;
        }

        // if (Input.GetKeyDown(KeyCode.X))
        // {
        //     if (_isBall)
        //     {
        //         hogRoot.GetComponent<Animator>().SetBool("IsBall", false);
        //         ballRoot.SetActive(false);
        //     }
        //     else
        //     {
        //         hogRoot.GetComponent<Animator>().SetBool("IsBall", true);
        //         ballRoot.SetActive(true);
        //     }
        //
        //     _isBall = !_isBall;
        // }
    }

    private void SetToBallMode()
    {
        hogRoot.GetComponent<Animator>().SetBool("IsBall", true);
        _hogCharacterController.enabled = false;
        _hogThirdPersonController.enabled = false;
        _hogInput.enabled = false;
        _hogCollider.enabled = false;
        // hogRoot.SetActive(false);

        _ballInput.enabled = true;
        _ballRigidbody.isKinematic = false;
        _ballCollider.enabled = true;
        ballRoot.SetActive(true);

        _isBall = true;
    }

    private void SetToWalkingMode()
    {
        _ballInput.enabled = false;
        _ballRigidbody.isKinematic = true;
        _ballCollider.enabled = false;
        ballRoot.SetActive(false);

        hogRoot.GetComponent<Animator>().SetBool("IsBall", false);
        _hogCharacterController.enabled = true;
        _hogThirdPersonController.enabled = true;
        _hogInput.enabled = true;
        _hogCollider.enabled = true;

        _isBall = false;
    }

    private bool OnIsland()
    {
        return Physics.OverlapSphere(ballRoot.transform.position, 5f).Any(hit => hit.CompareTag("Island"));
    }

    public bool IsSnowBall()
    {
        return _isBall;
    }
}