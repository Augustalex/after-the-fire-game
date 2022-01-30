using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class SfxManager : MonoSingleton<SfxManager>
{
    public AudioSource audioSource;
    [Header("SFX clips")]
    [SerializeField] private AudioClip seedDrop;
    [SerializeField] private AudioClip collideWithTree;
    [SerializeField] private AudioClip collideWithTreeSeedDrop;

    public Dictionary<string, AudioClip> Sfx;
    

    protected override void Awake()
    {
        base.Awake();
        Sfx = new Dictionary<string, AudioClip>();
        Sfx.Add("seedPickup", seedDrop);
        Sfx.Add("collideWithTree", collideWithTree);
        Sfx.Add("collideWithTreeSeedDrop", collideWithTreeSeedDrop);
    }

    public void PlaySfx(string sfxName, float volume = 1f, bool randomPitch = false)
    {
        var clip = Sfx.TryGetValue(sfxName, out AudioClip audioClip);
        if (audioClip)
        {

            audioSource.pitch = randomPitch ? Random.Range(0.9f, 1.1f) : 1f; 
            audioSource.PlayOneShot(audioClip, volume);
        }
        else
        {
            Debug.LogWarning("No audio clip found with name: " + sfxName);
        }
    }
}
