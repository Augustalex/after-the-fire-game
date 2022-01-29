using System.Numerics;
using DeformationSnow;
using TMPro;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    public PlayerData data;

    private Rigidbody _rigidbody;
    private bool _moving;
    private double _cooldown;
    private float _boostMeter;

    private Vector2 _move;
    private bool _inAir;
    private float _inAirCooldown;
    private Vector3 _previousPosition;
    private bool _jump;
    private bool _sprint;
    private float _stunnedCooldown;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        _jump = value.isPressed;
    }
    
    public void OnSprint(InputValue value)
    {
        _sprint = value.isPressed;
    }

    void Update()
    {
        if (Stunned())
        {
            _stunnedCooldown -= Time.deltaTime;
            return;
        }

        if (_inAir)
        {
            _rigidbody.drag = 1f;
            _rigidbody.AddForce(Vector3.down * 100f * Time.deltaTime, ForceMode.Acceleration);


            if (_inAirCooldown > 0)
            {
                _inAirCooldown -= Time.deltaTime;
            }
            else
            {
                var hitGround = Physics.Raycast(transform.position, Vector3.down, 1f);
                if (hitGround) _inAir = false;
            }

            return;
        }

        if (_rigidbody.velocity.magnitude > 10f)
        {
            _rigidbody.drag = 2f;
        }
        else if (Boosting())
        {
            _rigidbody.drag = 2f;
        }
        else if (_moving)
        {
            _rigidbody.drag = 2f;
        }
        else
        {
            _rigidbody.drag = 0.1f;
        }

        var direction = new Vector3(_move.x, 0, _move.y);
        
        if (_jump)
        {
            var grounded = Physics.OverlapSphere(transform.position, transform.localScale.x * .6f).Length > 1;
            
            if (grounded)
            {
                _inAirCooldown = 1f;
                _inAir = true;
                _rigidbody.AddForce(Vector3.up * data.jumpForce + direction * 8f, ForceMode.Impulse);
            }
        }
        
        if (direction != Vector3.zero)
        {
            _moving = true;

            var shiftBoost = Boosting() ? data.shiftBoost : 0f;
            var minSpeed = 3f;
            var startBoost = (Mathf.Max(0, minSpeed - _rigidbody.velocity.magnitude) / minSpeed) * data.startBoost;
            _rigidbody.AddForce((direction.normalized * (data.speed + startBoost + shiftBoost)) * Time.deltaTime,
                ForceMode.Acceleration);
        }
        else if (_cooldown < 0)
        {
            _cooldown = .2f;
            _moving = false;
        }
        else
        {
            _cooldown -= Time.deltaTime;
        }

        if (Boosting())
        {
            _boostMeter += Time.deltaTime;
        }

        _previousPosition = transform.position;

        _jump = false;
    }

    public Vector3 GetPreviousPosition()
    {
        return _previousPosition;
    }

    public bool Grounded()
    {
        return !_inAir;
    }

    public bool Moving()
    {
        return _moving;
    }

    public bool Boosting()
    {
        return _sprint;
    }

    public float BoostJuice()
    {
        return _boostMeter;
    }

    public void HitTree()
    {
        _stunnedCooldown = data.treeHitStunTime;
        
        _rigidbody.AddForce(-_rigidbody.velocity * 1.5f, ForceMode.Impulse);
    }

    public bool Stunned()
    {
        return _stunnedCooldown > 0f;
    }
}