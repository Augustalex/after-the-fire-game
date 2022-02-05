using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLandscapeGenerator : MonoBehaviour
{
    public GameObject wildNpcTemplate;
    public GameObject waySignTemplate;
    public GameObject terrainTemplate;
    public Transform followTarget;

    public const float GridSize = 10f;
    public const float ItemGridSize = 80f;
    public const float ForrestGridSize = 40f;

    private readonly HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();
    private readonly HashSet<Vector3> _itemExistsByPosition = new HashSet<Vector3>();
    private readonly HashSet<Vector3> _forrestExistsByPosition = new HashSet<Vector3>();


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

        if (Random.value < .35 && CanGenerateForrestOnPosition(terrain.transform.position))
        {
            GenerateForrest(terrain);
        }
        else if (CanGenerateItemOnPosition(terrain.transform.position))
        {
            if (terrain.transform.position.magnitude > 150f && Random.value < .3f)
            {
                GenerateWaySign(terrain);
            }
            else if (Random.value < .3f)
            {
                GenerateWildNpc(terrain);
            }
        }
    }

    private bool CanGenerateForrestOnPosition(Vector3 unalignedPosition)
    {
        return !_itemExistsByPosition.Contains(AlignToForrestGrid(unalignedPosition)) && 
               !_forrestExistsByPosition.Contains(AlignToForrestGrid(unalignedPosition)) &&
               !Physics.OverlapSphere(AlignToGrid(unalignedPosition), 20f).Any(s => s.CompareTag("Island"));
    }

    private bool CanGenerateItemOnPosition(Vector3 unalignedPosition)
    {
        return !_itemExistsByPosition.Contains(AlignToItemGrid(unalignedPosition)) &&
               !_forrestExistsByPosition.Contains(AlignToGrid(unalignedPosition)) &&
               !Physics.OverlapSphere(AlignToItemGrid(unalignedPosition), 100f).Any(s => s.CompareTag("Island"));
    }

    private void GenerateWildNpc(GameObject terrain)
    {
        var alignedPosition = AlignToItemGrid(terrain.transform.position);
        _itemExistsByPosition.Add(alignedPosition);
        
        var npc = Instantiate(wildNpcTemplate);
        npc.transform.position = alignedPosition + Vector3.up * 6f;
    }

    private void GenerateWaySign(GameObject terrain)
    {
        var alignedPosition = AlignToItemGrid(terrain.transform.position);
        _itemExistsByPosition.Add(alignedPosition);
        
        var waySign = Instantiate(waySignTemplate);
        waySign.transform.position = alignedPosition + Vector3.up * 6f;
    }

    private void GenerateForrest(GameObject terrain)
    {
        _forrestExistsByPosition.Add(AlignToForrestGrid(terrain.transform.position));
        
        terrain.GetComponent<WildForrestGenerator>().GenerateForrestOnPlane();
    }

    private static Vector3 AlignToGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % GridSize),
            0f,
            position.z - (position.z % GridSize));
    }

    private static Vector3 AlignToItemGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % ItemGridSize),
            0f,
            position.z - (position.z % ItemGridSize));
    }
    
    private static Vector3 AlignToForrestGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % ForrestGridSize),
            0f,
            position.z - (position.z % ForrestGridSize));
    }
}