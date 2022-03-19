using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class LoadingMenu : MonoBehaviour
{
    public GameObject startMenu;
    private ProceduralLandscapeGenerator _landscapeGenerator;
    private TMP_Text _loadingText;
    private double _cooldown;

    private string _fullText = " Loading";
    private int _textLength;

    private static readonly List<string> _fixedTexts = new List<string>
    {
        "Too slow",
        "infuriating?",
        "why? why... why!",
        "almost there..."
    };

    private Queue<string> _texts;

    void Start()
    {
        startMenu.SetActive(false);
     
        _texts = new Queue<string>(_fixedTexts.OrderBy(a => Random.Range(-1000, 1000)));
        
        _landscapeGenerator = FindObjectOfType<ProceduralLandscapeGenerator>();
        _loadingText = GetComponentInChildren<TMP_Text>();
        _loadingText.text = "Loading";
    }
    
    void Update()
    {
        if (_landscapeGenerator.IsReady())
        {
            startMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (_cooldown < 0)
        {
            _cooldown = 1.5f;

            _textLength += 1;
            if (_textLength > _fullText.Length)
            {
                _textLength = 0;
                _fullText = _texts.Dequeue();
            }
            
            _loadingText.text = _fullText.Substring(0, _textLength);
        }
        else
        {
            _cooldown -= Time.deltaTime;
        }
    }
}
