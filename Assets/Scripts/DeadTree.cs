using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;


public class DeadTree : MonoBehaviour
{
    public Material burningMaterial;
    public Material deadMaterial;
    public MeshRenderer treeMesh;
    public ParticleSystem fireParticles;
    [HideInInspector] public Island island; // Will be set in Island.cs onEnable

    // BELOW IS USED FOR SWINGING ANIMATION
    private Quaternion _originalRotation;
    private Quaternion _zeroRotation;

    private const float SwingTime = .15f;
    private float _currentMaxSwingTime = 0f;
    private float _swingTimeLeft = 0f;

    private readonly List<Quaternion> _swings = new List<Quaternion>();
    private CinemachineImpulseSource _impulseSource;
    private bool _falling;

    private double _fallingCooldown;
    // ABOVE IS USED FOR SWINGING ANIMATION

    [Serializable]
    private enum State
    {
        Burning = 0,
        Dead = 1,
        Planted = 2,
    }

    [SerializeField] private State currentState = State.Burning;
    private bool _killWhenDone;

    private void Start()
    {
        _zeroRotation = transform.rotation;
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentState == State.Burning)
            {
                // Check if player has enough snow
                var playerGrower = other.GetComponent<PlayerGrower>();
                var ballMover = other.GetComponent<PlayerBallMover>();
                ballMover.ZeroBoostJuice();
                if (playerGrower.GrowthProgress() > 0.2f || CheatEngine.Instance.Cheating())
                {
                    playerGrower.ReleaseThirdOfSnow();
                    currentState = State.Dead;
                    treeMesh.material = deadMaterial;
                    fireParticles.Stop();
                    SfxManager.Instance.PlaySfx("collideWithTreeFire", 1f, true);
                    island.OnExstuinguishTree();

                    ballMover.TriggerHitGroundParticles();
                }
                else
                {
                    SfxManager.Instance.PlaySfx("collideWithTree",
                        other.attachedRigidbody.velocity.magnitude * 0.05f,
                        true);
                }

                Shake();
                ballMover.HitDeadTree();
            }
            else if (currentState == State.Dead)
            {
                _killWhenDone = true;
                SfxManager.Instance.PlaySfx("collideWithTreeSeedDrop",
                    other.attachedRigidbody.velocity.magnitude * 0.05f,
                    true);

                Shake();
                other.GetComponentInChildren<PlayerBallMover>().HitDeadTree();
            }
        }
    }

    private void Update()
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
                    if (_killWhenDone)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Instantiate(TemplateLibrary.Instance.logTemplate,
                                transform.position + Vector3.up * 2f + Random.insideUnitSphere, Random.rotation, null);
                        }
                    }
                }
                
                if (_swings.Count == 0)
                {
                    transform.rotation = _zeroRotation;

                    if (_killWhenDone)
                    {
                        // Grow sapling instead of dead tree
                        // Instantiate(TemplateLibrary.Instance.saplingTemplate, transform.position, transform.rotation,
                        //     transform.parent);

                        Destroy(gameObject);
                    }
                }
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