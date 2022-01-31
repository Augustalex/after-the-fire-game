using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLandscapeGenerator : MonoBehaviour
{
    public GameObject waySignTemplate;
    public GameObject terrainTemplate;
    public Transform followTarget;

    public const float GridSize = 10f;

    private readonly HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();
    

    void Start()
    {
        CreateNewPlane(Vector3.zero);
    }

    void Update()
    {
        var followTargetPosition = followTarget.position;
        var alignedPosition = AlignToGrid(followTargetPosition);

        var lookAhead = 4;
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

        if (Random.value < .3 && CanGenerateForrest(terrain))
        {
            GenerateForrest(terrain);
        }
        
        if(terrain.transform.position.magnitude > 150f && Random.value < .3)
        {
            if (CanGenerateWaySign(terrain))
            {
                GenerateWaySign(terrain);
            }
        }
    }

    private bool CanGenerateWaySign(GameObject terrain)
    {
        return !Physics.OverlapSphere(AlignToGrid(terrain.transform.position), 25f).Any(s => s.CompareTag("WaySign")) &&
               !Physics.OverlapSphere(AlignToGrid(terrain.transform.position), 100f).Any(s => s.CompareTag("Island"));
    }

    private void GenerateWaySign(GameObject terrain)
    {
        var waySign = Instantiate(waySignTemplate);
        waySign.transform.position = terrain.transform.position + Vector3.up * 6f;
    }

    private bool CanGenerateForrest(GameObject terrain)
    {
        var gridPosition = AlignToGrid(terrain.transform.position);
        
        return !Physics.OverlapSphere(gridPosition, GridSize * 3f).Any(s => s.CompareTag("Island") || s.CompareTag("Tree"));
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