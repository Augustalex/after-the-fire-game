using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island : MonoBehaviour
{
    public Npc npc;
    public List<DeadTree> trees;
    public GameObject successObjects;
    private int numberOfTrees;

    public MeshRenderer islandMesh;
    public Material fireMaterial;
    public Material ashMaterial;
    public Material grassMaterial;
    
    [Serializable]
    private enum State
    {
        init = 0,
        part1done = 1,
        allCompleted = 2
    }

    [SerializeField] private State currentState = State.init;

    // States
    [SerializeField]private int _numberOfTreesExstingueshed;
    
    private void OnEnable()
    {
        foreach (var deadTree in trees)
        {
            deadTree.island = this;
        }

        numberOfTrees = trees.Count;
        islandMesh.material = fireMaterial;
        
        successObjects.SetActive(false);
    }

    public void OnExstuinguishTree()
    {
        if (currentState ==State.allCompleted) return;
        
        _numberOfTreesExstingueshed++;
        if (_numberOfTreesExstingueshed >= numberOfTrees)
        {
            currentState = State.part1done;
            islandMesh.material = ashMaterial;
            
            // TODO: send state to NPC, and then whern NPC state is done, set to completed and show this
            successObjects.SetActive(true);
            islandMesh.material = grassMaterial;
        }
    }
}
