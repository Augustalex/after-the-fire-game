using System;
using System.Collections;
using System.Collections.Generic;
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
    private PlayerController _ballController;
    private Rigidbody _ballRigidbody;
    private SphereCollider _ballCollider;
    private PlayerGrower _ballGrower;

    private int _npcsNearbyCount;
    private float _lastCloseToNpc;
    private float _lastTurnedToBall;
    private double _lastSwitchedToWalking;
    private bool _intro;
    private PlayerInputMediator _playerInputMediator;

    private void Awake()
    {
        _hogCharacterController = hogRoot.GetComponent<CharacterController>();
        _hogThirdPersonController = hogRoot.GetComponent<ThirdPersonController>();
        _hogCollider = hogRoot.GetComponent<BoxCollider>();

        _ballController = ballRoot.GetComponentInChildren<PlayerController>();
        _ballRigidbody = ballRoot.GetComponentInChildren<Rigidbody>();
        _ballCollider = ballRoot.GetComponentInChildren<SphereCollider>();
        _ballGrower = ballRoot.GetComponentInChildren<PlayerGrower>();

        _playerInputMediator = GetComponent<PlayerInputMediator>();

        SetToBallMode();
        _ballController.IntroStun();

        StartCoroutine(ExitIntroScene());
    }

    private IEnumerator ExitIntroScene()
    {
        _intro = true;
        yield return new WaitForSeconds(6f);

        SetToWalkingMode();
        _hogThirdPersonController.IntroStun();

        yield return new WaitForSeconds(4f);
        CameraModeController.Instance.SetToPlayerCamera();
        _intro = false;
    }

    void Update()
    {
        Transform ballRootTransform = ballRoot.transform;

        if (_isBall)
        {
            hogRoot.transform.position = ballRootTransform.position;
            hogRoot.transform.rotation = ballRootTransform.rotation;
        }
        else
        {
            ballRootTransform.position = hogRoot.transform.position;
            ballRootTransform.rotation = hogRoot.transform.rotation;
        }

        // TODO: Remove when decided to not use automatic "switch to walk when stopped"
        // if (CanSwitchToWalkingMode())
        // {
        //        SetToWalkingMode();
        // }

        if (_intro)
        {
            _ballRigidbody.velocity = Vector3.zero;
        }
    }

    public bool CanSwitchToWalkingMode()
    {
        var beenBallForMoreThan1Second = Time.time - _lastTurnedToBall > 1f;
        if (beenBallForMoreThan1Second && ballRoot.activeSelf && _ballGrower.GrowthProgress() < .25f)
        {
            if (_ballRigidbody.velocity.magnitude < 2f)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanTurnToBallRightNow()
    {
        return Time.time - _lastSwitchedToWalking > 1f;
    }

    public void SetToBallMode()
    {
        _lastTurnedToBall = Time.time;

        hogRoot.GetComponent<Animator>().SetBool("IsWalking", false);
        hogRoot.GetComponent<Animator>().SetBool("IsBall", true);
        _hogCharacterController.enabled = false;
        _hogThirdPersonController.enabled = false;
        _hogCollider.enabled = false;
        // hogRoot.SetActive(false);

        _ballController.enabled = true;
        // _ballRigidbody.isKinematic = false;
        _ballCollider.enabled = true;
        ballRoot.SetActive(true);

        _ballController.PrepareForStartRolling();

        _playerInputMediator.SetInputReceiver(_ballController);

        _isBall = true;
    }

    public void SetToWalkingMode()
    {
        _lastSwitchedToWalking = Time.time;

        _ballController.PrepareForStopRolling();

        _ballController.enabled = false;
        _ballCollider.enabled = false;
        // _ballRigidbody.isKinematic = true;
        ballRoot.SetActive(false);

        var zeroRotation = Quaternion.identity;
        var currentRotation = hogRoot.transform.rotation.eulerAngles;
        hogRoot.transform.rotation = Quaternion.Euler(
            zeroRotation.x,
            currentRotation.y,
            zeroRotation.z
        );
        _hogCharacterController.enabled = true;
        _hogThirdPersonController.enabled = true;
        _hogCollider.enabled = true;
        hogRoot.GetComponent<Animator>().SetBool("IsBall", false);

        _playerInputMediator.SetInputReceiver(_hogThirdPersonController);

        _isBall = false;
    }

    private bool HogNotOnIsland()
    {
        var onIsland = Physics.OverlapSphere(hogRoot.transform.position, 2f).Any(hit => hit.CompareTag("Island"));

        // return onIsland;

        return HogOnSnow() || HogInAir();
    }

    public bool HogInAir()
    {
        var hitGround = Physics.Raycast(hogRoot.transform.position, Vector3.down, transform.localScale.x * 2f);

        return !hitGround;
    }

    private bool HogOnIce()
    {
        return Physics.OverlapSphere(transform.position, 2f).Any(hit => hit.CompareTag("Ice"));
    }

    private bool HogOnSnow()
    {
        return Physics.OverlapSphere(transform.position, 2f).Any(hit => hit.CompareTag("Terrain"));
    }

    public bool IsSnowBall()
    {
        return _isBall;
    }

    public void CloseToNpc()
    {
        _lastCloseToNpc = Time.time;
    }

    public bool OnIsland()
    {
        var onIsland = Physics.OverlapSphere(hogRoot.transform.position, 2f).Any(hit => hit.CompareTag("Island"));

        return onIsland;
    }

    public Vector3 GetPlayerFeetPosition()
    {
        if (_isBall)
        {
            var position = _ballController.transform.position;
            return new Vector3(
                position.x,
                position.y - _ballGrower.GetRadius(),
                position.z
            );
        }
        else
        {
            return _hogThirdPersonController.transform.position;
        }
    }
}