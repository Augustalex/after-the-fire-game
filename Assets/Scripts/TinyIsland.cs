using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinyIsland : MonoBehaviour
{
    public GameObject[] islandTemplates;

    void Start()
    {
        var randomIsland = islandTemplates[Random.Range(0, islandTemplates.Length)];
        var position = transform.position;
        Instantiate(randomIsland, new Vector3(position.x, randomIsland.transform.position.y, position.z), randomIsland.transform.rotation, transform);
        
        GetComponent<WildForrestGenerator>().GenerateForrestOnPlane();
    }
}