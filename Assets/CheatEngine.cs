using System;
using Core;
using UnityEngine;

public class CheatEngine : MonoSingleton<CheatEngine>
{
    private string _cheat = "";
    private bool _cheatMode;

    public bool Cheating()
    {
        return _cheatMode;
    }

    private void Update()
    {
        if (_cheat == "AGGE")
        {
            _cheatMode = true;
        }
        else if (_cheat == "AGG" && Input.GetKeyDown(KeyCode.A))
        {
            _cheat = "AGGE";
        }
        else if (_cheat == "AG" && Input.GetKeyDown(KeyCode.G))
        {
            _cheat = "AGG";
        }
        else if (_cheat == "A" && Input.GetKeyDown(KeyCode.G))
        {
            _cheat = "AG";
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _cheat = "A";
        }
    }
}