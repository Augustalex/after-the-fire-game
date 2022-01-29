using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLandscapeGenerator : MonoBehaviour
{
    public GameObject islandOneTemplate;
    public GameObject terrainTemplate;
    public Transform followTarget;


    private readonly List<Transform> _planes = new List<Transform>();
    public const float GridSize = 10f;

    private HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();
    private HashSet<Vector3> _forrestExistsByPosition = new HashSet<Vector3>();
    private HashSet<Vector3> _islandExistsByPosition = new HashSet<Vector3>();

    private Vector3 _firstIslandLocation = AlignToGrid(new Vector3(5f * GridSize, 0, 5f * GridSize));

    void Start()
    {
        CreateNewPlane(Vector3.zero);
    }

    void Update()
    {
        var followTargetPosition = followTarget.position;
        var alignedPosition = AlignToGrid(followTargetPosition);

        var lookAhead = 6;
        for (var y = -lookAhead; y <= lookAhead; y++)
        {
            for (var x = -lookAhead; x <= lookAhead; x++)
            {
                var newPosition = new Vector3(alignedPosition.x - GridSize * x, 0, alignedPosition.z - GridSize * y);
                var existingPlane =
                    _planeExistsByPosition.Contains(newPosition);

                if (!existingPlane)
                {
                    CreateNewPlane(newPosition);
                }
            }
        }
    }

    private void CreateNewPlane(Vector3 nextPlanePosition)
    {
        var terrain = Instantiate(terrainTemplate);
        terrain.transform.position = nextPlanePosition;

        _planes.Add(terrain.transform);
        _planeExistsByPosition.Add(AlignToGrid(nextPlanePosition));

        StartCoroutine(GeneratePlaneAndContents(terrain));
    }

    private IEnumerator GeneratePlaneAndContents(GameObject terrain)
    {
        yield return terrain.GetComponentInChildren<ProceduralSnow>().GeneratePlane();

        if (CanGenerateIsland(terrain))
        {
            GenerateIsland(terrain);
        }
        else if (Random.value < .05 && CanGenerateForrest(terrain))
        {
            GenerateForrest(terrain);
        }
    }

    private bool CanGenerateForrest(GameObject terrain)
    {
        var gridPosition = AlignToGrid(terrain.transform.position);

        return Vector3.Distance(gridPosition, _firstIslandLocation) > GridSize * 4f;
    }

    private void GenerateIsland(GameObject terrain)
    {
        var island = Instantiate(islandOneTemplate);
        island.transform.position = terrain.transform.position + Vector3.up;

        _islandExistsByPosition.Add(island.transform.position);
    }

    private bool CanGenerateIsland(GameObject terrain)
    {
        if (_islandExistsByPosition.Count > 0) return false;
        if (Vector3.Distance(terrain.transform.position, _firstIslandLocation) < 1f) return true;
        return false;
    }

    private void GenerateForrest(GameObject terrain)
    {
        terrain.GetComponent<WildForrestGenerator>().GenerateForrestOnPlane();
    }

    private static Vector3 AlignToGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % GridSize),
            0f,
            position.z - (position.z % GridSize));
    }
}