using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    private Animator _animator;
    private int _size = 0;
    private bool _potGrowing;
    private const int MaxSize = 4;
    [SerializeField] private ParticleSystem stewParticles;
    private ParticleSystem.EmissionModule _stewParticles;
    [SerializeField] private ParticleSystem stewSplashParticles;
    [SerializeField] private ParticleSystem fireworks;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _stewParticles = stewParticles.emission;
        _stewParticles.enabled = false;
        _stewParticles.rateOverTime = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_potGrowing) return;
        
        if (other.CompareTag("PlayerHog") || other.CompareTag("Player"))
        {
            var playerInventory = other.GetComponentInParent<PlayerInventory>();
            var worms = playerInventory.GetWorms();
            if (worms > 0)
            {
                StartCoroutine(AnimatePotGrowing(worms, playerInventory));
            }
        }
    }

    private IEnumerator AnimatePotGrowing(int worms, PlayerInventory playerInventory)
    {
        _potGrowing = true;
        
        for (var i = 0; i < worms; i++)
        {

            var nextSize = NextSize();
            _animator.SetInteger("Size", nextSize);

            playerInventory.ConsumeWorm();
            GameManager.Instance.Quest1SetProgress();
            _stewParticles.enabled = true;
            _stewParticles.rateOverTime = GameManager.Instance.Quest1Progress;
            stewSplashParticles.Play();
            SfxManager.Instance.PlaySfxWithPitch("seedPickup", 1f, 0.92f + _size * .08f);
            SfxManager.Instance.PlaySfx("splash");

            if (GameManager.Instance.Quest1Completed)
            {
                fireworks.Play();
                SfxManager.Instance.PlaySfx("success", 0.4f);
            }

            yield return new WaitForSeconds(1f);
        }
        
        _potGrowing = false;
    }

    private int NextSize()
    {
        if (_size == MaxSize)
        {
            return MaxSize;
        }
        else
        {
            _size += 1;

            return _size;
        }
    }
}