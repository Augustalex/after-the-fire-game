using UnityEngine;
using Random = UnityEngine.Random;

public class WildTree : MonoBehaviour
{
    private float _shakeDuration;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    void Update()
    {
        if (_shakeDuration > 0f)
        {
            _shakeDuration -= Time.deltaTime;
            if (_shakeDuration <= 0)
            {
                transform.position = _originalPosition;
                transform.rotation = _originalRotation;
            }
            else
            {
                transform.rotation = _originalRotation * Quaternion.Euler(RandomShakeOffset());
            }
        }
    }

    private Vector3 RandomShakeOffset()
    {
        return Random.insideUnitSphere * 2f;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player") && _shakeDuration <= 0)
        {
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            Shake();
        }
    }

    private void Shake()
    {
        _shakeDuration = .25f;
    }
}
