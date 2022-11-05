using UnityEngine;

public class IceCracker : MonoBehaviour
{
    private Material _material;
    private float _crackLevel;
    private static readonly int CrackLevel = Shader.PropertyToID("_CrackLevel");

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            if (other.relativeVelocity.y < -15f)
            {
                _crackLevel = Mathf.Min(1f, _crackLevel + .1f);
                _material.SetFloat(CrackLevel, _crackLevel);
            }
        }
    }
}
