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
    [SerializeField] private AudioClip collideWithTreeFire;
    [SerializeField] private AudioClip splash;
    [SerializeField] private AudioClip success;
    [SerializeField] private AudioClip gettingBug;

    public Dictionary<string, AudioClip> Sfx;
    

    protected override void Awake()
    {
        base.Awake();
        Sfx = new Dictionary<string, AudioClip>();
        Sfx.Add("seedPickup", seedDrop);
        Sfx.Add("collideWithTree", collideWithTree);
        Sfx.Add("collideWithTreeSeedDrop", collideWithTreeSeedDrop);
        Sfx.Add("collideWithTreeFire", collideWithTreeFire);
        Sfx.Add("splash", splash);
        Sfx.Add("success", success);
        Sfx.Add("gettingBug", gettingBug);
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
    
    public void PlaySfxWithPitch(string sfxName, float volume = 1f, float pitch = .5f)
    {
        var clip = Sfx.TryGetValue(sfxName, out AudioClip audioClip);
        if (audioClip)
        {

            audioSource.pitch = pitch; 
            audioSource.PlayOneShot(audioClip, volume);
        }
        else
        {
            Debug.LogWarning("No audio clip found with name: " + sfxName);
        }
    }
}
