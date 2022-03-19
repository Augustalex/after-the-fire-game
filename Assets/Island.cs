using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island : MonoBehaviour
{
    public Npc npc;
    public List<DeadTree> trees;
    public GameObject successObjects;

    public MeshRenderer islandMesh;
    public Material fireMaterial;
    public Material ashMaterial;
    public Material grassMaterial;

    [Serializable]
    private enum State
    {
        OnFire = 0,
        FertileSoil = 1,
        LushGreens = 2
    }

    [SerializeField] private State currentState = State.OnFire;

    // States
    [SerializeField] private int _numberOfTreesExtinguished;

    private int _numberOfTrees;
    private int _numberOfTreesNeeded = 0; // Circular dependency on island NPC - Set by NPC
    private int _treesGrown;

    public bool CanGrowTrees()
    {
        return currentState == State.FertileSoil || currentState == State.LushGreens;
    }

    public void PlantedTree()
    {
        _treesGrown += 1;

        if (_treesGrown == _numberOfTreesNeeded)
        {
            OnAllCompleted();
        }
    }

    private void OnEnable()
    {
        foreach (var deadTree in trees)
        {
            deadTree.island = this;
        }

        _numberOfTrees = trees.Count;
        islandMesh.material = fireMaterial;

        successObjects.SetActive(false);

        npc.island = this;
        _numberOfTreesNeeded = npc.numberOfConesToFetch;
    }

    public void OnExstuinguishTree()
    {
        if (currentState == State.LushGreens) return;

        _numberOfTreesExtinguished++;
        if (_numberOfTreesExtinguished >= _numberOfTrees)
        {
            currentState = State.FertileSoil;
            islandMesh.material = ashMaterial;
            npc.OnPart1Complete();
        }
    }

    public void OnAllCompleted()
    {
        if (currentState == State.FertileSoil)
        {
            StartCoroutine(CompleteSoon());
            currentState = State.LushGreens;
        }
    }

    private IEnumerator CompleteSoon()
    {
        yield return new WaitForSeconds(1);
        successObjects.SetActive(true);
        islandMesh.material = grassMaterial;
    }

    public int TreesGrown()
    {
        return _treesGrown;
    }
}