using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class WildTree : MonoBehaviour
{
    public GameObject itemTemplate;

    private Quaternion _originalRotation;
    private Quaternion _zeroRotation;

    private const float SwingTime = .15f;
    private float _currentMaxSwingTime = 0f;
    private float _swingTimeLeft = 0f;

    private readonly List<Quaternion> _swings = new List<Quaternion>();
    private CinemachineImpulseSource _impulseSource;

    private void Start()
    {
        _zeroRotation = transform.rotation;
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        if (_swings.Count > 0)
        {
            _swingTimeLeft -= Time.deltaTime;

            var progress = Mathf.Clamp((_swingTimeLeft) / SwingTime, 0f, 1f);
            transform.rotation = KeepY(Quaternion.Lerp(_originalRotation, _swings[0], 1f - progress));

            if (progress <= 0)
            {
                _swings.RemoveAt(0);
                _currentMaxSwingTime = _currentMaxSwingTime * .8f;
                _swingTimeLeft = SwingTime;
                _originalRotation = transform.rotation;

                if (_swings.Count == 1)
                {
                    DropItem();
                }

                if (_swings.Count == 0)
                {
                    transform.rotation = _zeroRotation;
                }
            }
        }
    }

    private void DropItem()
    {
        Instantiate(itemTemplate, transform.position + Vector3.up * 3f + Random.insideUnitSphere, Random.rotation,
            null);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player") && _swings.Count == 0)
        {
            var grower = other.collider.GetComponentInChildren<PlayerGrower>();
            if (grower.SizeToMaxSize() < .2f)
            {
                Shake();
                other.collider.GetComponentInChildren<PlayerController>().HitTree();
                grower.ReleaseSnow();
                SfxManager.Instance.PlaySfx("collideWithTreeSeedDrop"); 
            }
            else
            {
                SfxManager.Instance.PlaySfx("collideWithTree",  other.rigidbody.velocity.magnitude * 0.05f, true); 
            }
            
        }
    }

    private void Shake()
    {
        var a = GenerateRandomShakeOffset(1f);
        var b = Quaternion.Inverse(a);
        var c = GenerateRandomShakeOffset(.5f);
        var d = Quaternion.Inverse(c);
        var e = GenerateRandomShakeOffset(.1f);

        _swings.AddRange(new[]
        {
            a,
            b,
            c,
            d,
            e,
            _zeroRotation
        });
        _originalRotation = transform.rotation;
        _currentMaxSwingTime = SwingTime;
        _swingTimeLeft = SwingTime;

        _impulseSource.GenerateImpulse();
    }

    private Quaternion GenerateRandomShakeOffset(float scale)
    {
        var offset = RandomShakeOffset();
        return KeepY(Quaternion.Euler(offset * scale));
    }

    private Quaternion KeepY(Quaternion other)
    {
        return new Quaternion(
            other.x,
            _zeroRotation.y,
            other.z,
            _zeroRotation.w
            );
    }

    private Vector3 RandomShakeOffset()
    {
        return Random.insideUnitSphere.normalized * 4f;
    }
}