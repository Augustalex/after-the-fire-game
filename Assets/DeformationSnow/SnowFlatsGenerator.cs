using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeformationSnow;
using UnityEngine;
using Random = UnityEngine.Random;

// Generate a fixed sized snowy surface - not necessarily flat (that depends on the generationData)
public class SnowFlatsGenerator : MonoBehaviour
{
    public GenerationData generationData;
    public GameObject terrainTemplate;
    public Transform followTarget;

    public const float GridSize = 10f;

    private readonly HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();
    
    void Start()
    {
        CreateNewPlane(Vector3.zero);

        var followTargetPosition = followTarget.position;
        var alignedPosition = AlignToGrid(followTargetPosition);

        var lookAhead = 16;
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
        var snowComponent = terrain.GetComponentInChildren<ProceduralSnow>();
        snowComponent.generationData = generationData;
        yield return snowComponent.GeneratePlane();
    }

    private static Vector3 AlignToGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % GridSize),
            0f,
            position.z - (position.z % GridSize));
    }
}