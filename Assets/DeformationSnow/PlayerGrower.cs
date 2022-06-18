using System;
using System.Linq;
using Player;
using UnityEngine;

public class PlayerGrower : MonoBehaviour
{
    private const float MaxSize = 2f;

    private MeshRenderer _ballMeshRenderer;
    private PlayerBallMover _ballMover;
    private bool _maxSizeReached;
    private Vector3 _originalSize;
    private Rigidbody _rigidbody;
    private bool _visible;
    private PlayerSize _playerSize;

    private void Start()
    {
        _rigidbody = GetComponent<SphereCollider>().attachedRigidbody;
        _ballMover = GetComponentInParent<PlayerBallMover>();
        _playerSize = GetComponentInParent<PlayerSize>();

        _originalSize = transform.localScale;

        _ballMeshRenderer = FindObjectOfType<PlayerFakeBall>().GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        var onIsland = _ballMover.OnIsland();
        var onSnow = _ballMover.TouchingSnow();

        if (transform.localScale.x > _originalSize.x)
            SetVisible();
        else if (onIsland) SetInvisible();

        if (_ballMover.Stunned()) return;

        var releasing = _ballMover.Releasing();
        if (releasing > 0f)
        {
            if (GrowthProgress() > 0.001f)
            {
                // _controller.TriggerHitGroundParticles();
            }

            ReleaseSomeSnow(releasing * Time.deltaTime * .5f);
        }
        else if (onSnow && !onIsland)
        {
            if (_rigidbody.velocity.magnitude > 1f)
            {
                var moveTime = Mathf.Clamp(Mathf.Max(_ballMover.TimeMoving() - .5f, 0f) / 12f, 0f, 1f);
                var growthRate = _ballMover.Boosting() ? .004f + 4f * moveTime : .004f + .8f * moveTime;

                var toGrow = growthRate * SizeToMaxSize() * Time.deltaTime;

                if (transform.localScale.x >= MaxSize * .98f)
                    _maxSizeReached = true;
                else
                    _maxSizeReached = false;

                transform.localScale += Vector3.one * toGrow;
                if (transform.localScale.x > MaxSize) SetToMaxSize();
            }
        }

        _playerSize.Change(GrowthProgress());
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

    public static float InSine(float t)
    {
        return (float) -Math.Cos(t * Math.PI / 2);
    }

    public static float OutSine(float t)
    {
        return (float) Math.Sin(t * Math.PI / 2);
    }

    private void SetVisible()
    {
        _ballMeshRenderer.enabled = true;

        if (!_visible) transform.localScale = _originalSize;

        _visible = true;
    }

    private void SetInvisible()
    {
        _visible = false;
        _ballMeshRenderer.enabled = false;
    }

    public void ReleaseSnow()
    {
        _ballMover.ZeroBoostJuice(); // TODO: Fix circular depedency
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

        if (finalSizeFactor < .2f) ReleaseSnow();
    }

    public float GetRadius()
    {
        return transform.lossyScale.x * .5f;
    }

    public static float InCirc(float t)
    {
        return -((float) Math.Sqrt(1 - t * t) - 1);
    }

    public void PrepareForWalkingMode()
    {
        SetInvisible();
    }

    public void PrepareForBallMode()
    {
        // SetVisible();
    }

    public void ReleaseSomeSnow(float amount)
    {
        var finalSizeFactor = Mathf.Clamp(transform.localScale.x - amount, _originalSize.x, MaxSize);
        transform.localScale = Vector3.one * finalSizeFactor;

        if (finalSizeFactor < .2f) ReleaseSnow();
    }
}