using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralLandscapeGenerator : MonoBehaviour
{
    public GameObject terrainTemplate;
    public Transform followTarget;


    private readonly List<Transform> _planes = new List<Transform>();
    public const float GridSize = 10f;

    private HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();

    void Start()
    {
        CreateNewPlane(Vector3.zero);
    }

    void Update()
    {
        var followTargetPosition = followTarget.position;
        var alignedPosition = AlignToGrid(followTargetPosition);

        var lookAhead = 3;
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

        terrain.GetComponentInChildren<ProceduralSnow>().GeneratePlane();
    }

    private Vector3 AlignToGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % GridSize),
            0f,
            position.z - (position.z % GridSize));
    }
}