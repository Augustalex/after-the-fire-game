using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLandscapeGenerator : MonoBehaviour
{
    public GameObject terrainTemplate;
    public Transform followTarget;

    public const float GridSize = 10f;

    private readonly HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();
    private HashSet<Vector3> _islandExistsByPosition = new HashSet<Vector3>();

    public IslandInfo[] islandInfos;

    [Serializable]
    public struct IslandInfo
    {
        public GameObject template;
        public int gridX;
        public int gridZ;
    }

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

        _planeExistsByPosition.Add(AlignToGrid(nextPlanePosition));

        StartCoroutine(GeneratePlaneAndContents(terrain));
    }

    private IEnumerator GeneratePlaneAndContents(GameObject terrain)
    {
        yield return terrain.GetComponentInChildren<ProceduralSnow>().GeneratePlane();

        // if (CanGenerateIsland(terrain))
        // {
        //     GenerateIsland(terrain);
        // }
        if (Random.value < .05 && CanGenerateForrest(terrain))
        {
            GenerateForrest(terrain);
        }
    }

    private bool CanGenerateForrest(GameObject terrain)
    {
        var gridPosition = AlignToGrid(terrain.transform.position);

        return !Physics.OverlapSphere(gridPosition, GridSize * 2f).Any(s => s.CompareTag("Island"));
    }

    public Vector3 IslandLocation(IslandInfo info)
    {
        return AlignToGrid(new Vector3(info.gridX * GridSize, 0, info.gridZ * GridSize));
    }

    private void GenerateIsland(GameObject terrain)
    {
        var islandInfo = islandInfos.FirstOrDefault(info =>
            Vector3.Distance(AlignToGrid(terrain.transform.position), IslandLocation(info)) > GridSize * 4f);
        if (islandInfo.template != null)
        {
            var islandLocation = IslandLocation(islandInfo);
            if (!_islandExistsByPosition.Contains(islandLocation))
            {
                var island = Instantiate(islandInfo.template);
                island.transform.position = islandLocation;

                _islandExistsByPosition.Add(islandLocation);
            }
        }
    }

    private bool CanGenerateIsland(GameObject terrain)
    {
        if (_islandExistsByPosition.Count > islandInfos.Length) return false;

        return islandInfos.Any(islandInfo =>
        {
            var distanceToIsland = Vector3.Distance(
                AlignToGrid(terrain.transform.position),
                IslandLocation(islandInfo));

            return !_islandExistsByPosition.Contains(IslandLocation(islandInfo))
                   && distanceToIsland > GridSize * 4f;
        });
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