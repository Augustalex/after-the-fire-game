using UnityEngine;
using Random = UnityEngine.Random;

public class SnowGenerator : MonoBehaviour
{
    public GameObject snowParticleTemplate;
    private int _count;
    private float _countdown;

    void Update()
    {
        if (_countdown > 0)
        {
            _countdown -= Time.deltaTime;
            return;
        }

        if (_count > 20) return;

        _count += 1;
        for (var i = 0; i < 1000; i++)
        {
            var randomCircle = Random.insideUnitCircle * 100;
            var position = transform.position + new Vector3(randomCircle.x, 10f, randomCircle.y);
            Instantiate(snowParticleTemplate, position, Random.rotation);
        }

        _countdown = 1.5f;
    }
}