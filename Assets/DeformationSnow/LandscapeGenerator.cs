using System;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    public GameObject planeTemplate;

    private void Start()
    {
        var scale = planeTemplate.transform.localScale.x * 2f;
        var offsetFactor = .05f;
        
        for (var y = 0; y < 50; y++)
        {
            for (var x = 0; x < 50; x++)
            {
                var plane = Instantiate(planeTemplate);
                plane.transform.position = new Vector3(x * scale * (1f - offsetFactor), 0, y * scale * (1f- offsetFactor));
            }
        }
    }
}