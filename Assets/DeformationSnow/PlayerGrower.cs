using System;
using System.Linq;
using UnityEngine;

public class PlayerGrower : MonoBehaviour
{
    public MeshRenderer ballMeshRenderer;
    
    private Rigidbody _rigidbody;
    private PlayerController _controller;
    private bool _maxSizeReached;
    private Vector3 _originalSize;
    private bool _visible;

    private const float MaxSize = 2f;
    
    private void Awake()
    {
        ballMeshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    void Start()
    {
        _rigidbody = GetComponent<SphereCollider>().attachedRigidbody;
        _controller = GetComponentInParent<PlayerController>();

        _originalSize = transform.localScale;
    }

    void Update()
    {
        if (_controller.Stunned()) return;
        if (_visible && transform.localScale == _originalSize && OnIsland())
        {
            SetInvisible();
        }
        else if(!_visible && OnSnow())
        {
            ReleaseSnow();
            SetVisible();
        }

        if (_rigidbody.velocity.magnitude > 1f)
        {
            var boostFactor = Mathf.Clamp(_controller.BoostJuice() / 12f, 0f, 1f);
            var boostRate = .004f + 5f * boostFactor;
            var growthRate = _controller.Boosting() ? boostRate : .004f;
            
            var toGrow = growthRate * SizeToMaxSize() * Time.deltaTime;
            
            if (transform.localScale.x >= MaxSize * .98f)
            {
                _maxSizeReached = true;
            }
            else
            {
                _maxSizeReached = false;
            }
            
            transform.localScale += Vector3.one * toGrow;
            if (transform.localScale.x > MaxSize)
            {
                transform.localScale = Vector3.one * MaxSize;
            }
        }
    }

    public float SizeToMaxSize()
    {
        var progress = Mathf.Clamp((transform.localScale.x - _originalSize.x) / (MaxSize - _originalSize.x), 0f, 1f);
        var easedProgress = Mathf.Clamp(OutSine(1 - progress), 0f, 1f);

        return easedProgress;
    }
    
    public static float InSine(float t) => (float)-Math.Cos(t * Math.PI / 2);
    public static float OutSine(float t) => (float)Math.Sin(t * Math.PI / 2);

    private void SetVisible()
    {
        _visible = true;
        ballMeshRenderer.enabled = true;
    }

    private void SetInvisible()
    {
        _visible = false;
        ballMeshRenderer.enabled = false;
    }

    private bool OnIsland()
    {
        return Physics.OverlapSphere(transform.position, ballMeshRenderer.transform.localScale.x).Any(hit => hit.CompareTag("Island"));
    }
    
    private bool OnSnow()
    {
        return Physics.OverlapSphere(transform.position, ballMeshRenderer.transform.localScale.x).Any(hit => hit.CompareTag("Terrain"));
    }
    
    public void ReleaseSnow()
    {
        transform.localScale = _originalSize;
    }

    public bool MaxSizeReached()
    {
        return _maxSizeReached;
    }
    
    public static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);

}
