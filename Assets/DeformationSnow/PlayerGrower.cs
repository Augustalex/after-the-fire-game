using System;
using System.Linq;
using UnityEngine;

public class PlayerGrower : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private PlayerController _controller;
    private bool _maxSizeReached;
    private Vector3 _originalSize;
    private bool _visible;
    private bool _onIsland;
    private bool _onSnow;
    
    private MeshRenderer _ballMeshRenderer;

    private const float MaxSize = 2f;

    void Start()
    {
        _rigidbody = GetComponent<SphereCollider>().attachedRigidbody;
        _controller = GetComponentInParent<PlayerController>();

        _originalSize = transform.localScale;

        _ballMeshRenderer = FindObjectOfType<PlayerFakeBall>().GetComponent<MeshRenderer>();
    }

    void Update()
    {
        _onIsland = OnIsland();
        _onSnow = OnSnow();

        if (transform.localScale.x > _originalSize.x)
        {
            SetVisible();
        }
        else if (_onIsland)
        {
            SetInvisible();
        }

        if (_controller.Stunned()) return;

        if (_onSnow && !_onIsland)
        {
            if (_rigidbody.velocity.magnitude > 1f)
            {
                var moveTime = Mathf.Clamp(Mathf.Max(_controller.TimeMoving() - .5f, 0f) / 12f, 0f, 1f);
                var growthRate = _controller.Boosting() ? (.004f + .8f * moveTime) : .004f + 4f * moveTime;
        
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
                    SetToMaxSize();
                }
            }
        }
    }

    private void SetToMaxSize()
    {
        transform.localScale = Vector3.one * MaxSize;
    }

    public float SizeToMaxSize()
    {
        var progress = Mathf.Clamp((transform.localScale.x - _originalSize.x) / (MaxSize - _originalSize.x), 0f, 1f);
        var easedProgress = Mathf.Clamp(OutSine(1 - progress), 0f, 1f);

        return easedProgress;
    }

    public float GrowthProgress()
    {
        var progress = Mathf.Clamp((transform.localScale.x - _originalSize.x) / (MaxSize - _originalSize.x), 0f, 1f);

        return progress;
    }

    public static float InSine(float t) => (float) -Math.Cos(t * Math.PI / 2);
    public static float OutSine(float t) => (float) Math.Sin(t * Math.PI / 2);

    private void SetVisible()
    {
        _ballMeshRenderer.enabled = true;

        if (!_visible)
        {
            transform.localScale = _originalSize;
        }

        _visible = true;
    }

    private void SetInvisible()
    {
        _visible = false;
        _ballMeshRenderer.enabled = false;
    }

    private bool OnIsland()
    {
        return Physics.OverlapSphere(transform.position, transform.localScale.x).Any(hit => hit.CompareTag("Island"));
    }

    private bool OnSnow()
    {
        return Physics.OverlapSphere(transform.position, transform.localScale.x).Any(hit => hit.CompareTag("Terrain"));
    }

    public void ReleaseSnow()
    {
        _controller.ZeroBoostJuice(); // TODO: Fix circular depedency
        transform.localScale = _originalSize;
    }

    public bool MaxSizeReached()
    {
        return _maxSizeReached;
    }

    public void ReleaseThirdOfSnow()
    {
        var sizeFactor = (MaxSize - _originalSize.x) * .36f;
        var finalSizeFactor = Mathf.Clamp(transform.localScale.x - sizeFactor, _originalSize.x, MaxSize);
        transform.localScale = Vector3.one * finalSizeFactor;

        if (finalSizeFactor < .2f)
        {
            ReleaseSnow();
        }
    }

    public float GetRadius()
    {
        return transform.lossyScale.x * .5f;
    }

    public static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);

    public void PrepareForWalkingMode()
    {
        SetInvisible();
    }

    public void PrepareForBallMode()
    {
        // SetVisible();
    }
}