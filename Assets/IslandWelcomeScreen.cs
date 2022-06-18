using System;
using UnityEngine;

public class IslandWelcomeScreen : MonoBehaviour
{
    public GameObject welcomeScreen;
    private Vector3 _originalPosition;
    private double _showUntil;
    private Rigidbody2D _rigidbody;
    private bool _liftUp;
    private float _liftUpUntil;
    private RectTransform _rectTransform;

    public event Action Done;

    private void Awake()
    {
        _rectTransform = welcomeScreen.GetComponent<RectTransform>();
        _originalPosition = _rectTransform.anchoredPosition;

        _rigidbody = welcomeScreen.GetComponent<Rigidbody2D>();
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
                _rectTransform.anchoredPosition += Vector2.up * Time.deltaTime * 300f;
            }
        }
        else if (Time.time > _showUntil)
        {
            _liftUp = true;
            _liftUpUntil = Time.time + 4;
        }
    }

    public void Play(IslandInfo island)
    {
        _rigidbody.simulated = true;
        _liftUp = false;
        _showUntil = Time.time + 4;
        _rectTransform.anchoredPosition = _originalPosition;
        
        GetComponentInChildren<IslandSign>().Refresh(island);
    }
}