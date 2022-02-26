using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MenuActions : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }
    
    private static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);
    private static float OutCirc(float t) => 1 - InCirc(1 - t);

    public void OnMusicVolumeChange(float value)
    {
        var valueNormalized = (value + 80) / 80;
        var factor = OutCirc(valueNormalized);
        var result = (80 * factor) - 80;

        SfxManager.Instance.musicMixer.SetFloat("musicVolume", result);
    }

}
