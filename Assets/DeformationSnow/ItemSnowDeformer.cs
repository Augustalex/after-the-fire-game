using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSnowDeformer : MonoBehaviour
{
    private List<TerrainDeformer> _deformers = new List<TerrainDeformer>();

    void Update()
    {
        // if (_deformers.Count > 0)
        // {
        // }
        // else
        // {
        //     var hits = Physics.OverlapSphere(transform.position, 15f);
        //     var terrains = hits.Where(hit => hit.CompareTag("Terrain"));
        //     var deformers = terrains.Select(terrain => terrain.GetComponent<TerrainDeformer>());
        //     _deformers = deformers.ToList();
        // }
    }

    private void OnCollisionEnter(Collision other)
    {
        // foreach (var terrainDeformer in _deformers)
        // {
        //     terrainDeformer.AddPoints(other.contacts);
        // }
    }

    private void OnCollisionStay(Collision other)
    {
        // foreach (var terrainDeformer in _deformers)
        // {
        //     terrainDeformer.AddPoints(other.contacts);
        // }
    }
}