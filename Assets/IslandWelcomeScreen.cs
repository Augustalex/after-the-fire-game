using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IslandWelcomeScreen : MonoBehaviour
{
    public GameObject welcomeScreen;
    private Vector3 _originalPosition;
    private double _showUntil;
    private Rigidbody2D _rigidbody;
    private bool _liftUp;
    private float _liftUpUntil;
    private TMP_Text _text;
    private Joint2D _joint;

    public event Action Done;

    private void Awake()
    {
        _joint = GetComponentInChildren<Joint2D>();
        _originalPosition = welcomeScreen.transform.position;
        _rigidbody = welcomeScreen.GetComponent<Rigidbody2D>();

        _text = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (_liftUp)
        {
            if (Time.time > _liftUpUntil)
            {
                _liftUp = false;
                Done?.Invoke();
            }
            else
            {
                _rigidbody.simulated = false;
                welcomeScreen.transform.position += Vector3.up * Time.deltaTime * 300f;
            }
        }
        else if (Time.time > _showUntil)
        {
            _liftUp = true;
            _liftUpUntil = Time.time + 4;
        }
    }

    public void Play(string text)
    {
        _rigidbody.simulated = true;
        _liftUp = false;
        _showUntil = Time.time + 4;
        welcomeScreen.transform.position = _originalPosition + Vector3.up;

        _text.text = text;
    }
}