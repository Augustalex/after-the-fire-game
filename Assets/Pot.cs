using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    private Animator _animator;
    private int _size = 0;
    private bool _potGrowing;
    private const int MaxSize = 4;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
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
            SfxManager.Instance.PlaySfxWithPitch("seedPickup", 1f, 0.92f + _size * .08f);

            var nextSize = NextSize();
            _animator.SetInteger("Size", nextSize);

            playerInventory.ConsumeWorm();
            GameManager.Instance.Quest1SetProgress();

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