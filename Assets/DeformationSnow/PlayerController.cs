using System.Numerics;
using DeformationSnow;
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

    private const float JumpForce = 8f;

    private Vector2 _move;
    private bool _inAir;
    private float _inAirCooldown;
    private Vector3 _previousPosition;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        _move = newMoveDirection;
    }

    void FixedUpdate()
    {
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
        else if (!Physics.Raycast(transform.position, Vector3.down, 1f))
        {
            Ground();
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
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.OverlapSphere(transform.position, transform.localScale.x * .5f).Length > 1)
            {
                _inAirCooldown = 1f;
                _inAir = true;
                _rigidbody.AddForce(Vector3.up * JumpForce + direction * 8f, ForceMode.Impulse);
            }
        }

        // if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        // {
        //     direction += Vector3.left;
        // }
        //
        // if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        // {
        //     direction += Vector3.right;
        // }
        //
        // if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        // {
        //     direction += Vector3.forward;
        // }
        //
        // if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        // {
        //     direction += Vector3.back;
        // }

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
    }

    private void Ground()
    {
        // RaycastHit hit;
        // if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
        //     var distanceToGround = hit.distance;
        //     if (distanceToGround > .1f)
        //     {
        //         transform.position = transform.position - Vector3.up * distanceToGround + Vector3.up * transform.localScale.x * .6f;
        //     }
        //     // _rigidbody.AddForce(Vector3.down * distanceToGround * 5000f * Time.deltaTime, ForceMode.Acceleration);
        // }
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
        return Input.GetKey(KeyCode.LeftShift);
    }

    public float BoostJuice()
    {
        return _boostMeter;
    }
}