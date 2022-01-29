using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private bool _moving;
    private double _cooldown;
    private float _boostMeter;

    private const float JumpForce = 12f;

    private Vector2 _move;
    private bool _inAir;

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
    
    void Update()
    {
        if (_inAir)
        {
            var hitGround = Physics.Raycast(transform.position, Vector3.down, 1f);
            if (hitGround) _inAir = false;
            return;
        }
        
        Debug.Log(_inAir);
        
        if (_rigidbody.velocity.magnitude > 10f)
        {
            _rigidbody.drag = 1f;
        }
        else if (Boosting())
        {
            _rigidbody.drag = 3f;
        }
        else if (_moving)
        {
            _rigidbody.drag = 2f;
        }
        else
        {
            _rigidbody.drag = 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.OverlapSphere(transform.position, transform.localScale.x * .5f).Length > 1)
            {
                _inAir = true;
                _rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            }
        }

        var direction = new Vector3(_move.x, 0, _move.y);;
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

            var xShiftBoost = 1000f;
            var xSpeed = 1000f;
            var xBoost = 2500f;

            var shiftBoost = Boosting() ? xShiftBoost : 0f;
            var minSpeed = 3f;
            var startBoost = (Mathf.Max(0, minSpeed - _rigidbody.velocity.magnitude) / minSpeed) * xBoost;
            _rigidbody.AddForce((direction.normalized * (xSpeed + startBoost + shiftBoost)) * Time.deltaTime,
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