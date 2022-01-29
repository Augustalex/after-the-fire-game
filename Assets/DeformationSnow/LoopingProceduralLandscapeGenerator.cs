using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoopingProceduralLandscapeGenerator : MonoBehaviour
{
    public GameObject terrainTemplate;
    public Transform followTarget;


    private readonly List<Transform> _planes = new List<Transform>();
    private Vector3 _lastPosition;
    public const int WorldSize = 6;
    public const float GridSize = 10f;

    private HashSet<Vector3> _planeExistsByPosition = new HashSet<Vector3>();
    private Dictionary<Vector3, GameObject> _world = new Dictionary<Vector3, GameObject>();

    private List<GameObject> _toStitch = new List<GameObject>();
    private List<GameObject> _toStitchLater = new List<GameObject>();

    void Start()
    {
        _lastPosition = Vector3.zero;
        CreateNewPlane(Vector3.zero);
    }

    void Update()
    {
        var followTargetPosition = followTarget.position;
        var alignedPosition = AlignToGrid(followTargetPosition);

        var lookAhead = 1;
        for (var y = -lookAhead; y <= lookAhead; y++)
        {
            for (var x = -lookAhead; x <= lookAhead; x++)
            {
                var newPosition = new Vector3(alignedPosition.x - GridSize * x, 0, alignedPosition.z - GridSize * y);
                var existingPlane =
                    _world.ContainsKey(ModPosition(newPosition));

                if (existingPlane)
                {
                    var modPosition = ModPosition(newPosition);
                    var plane = _world[modPosition];
                    var planeTransform = plane.transform;
                    if (planeTransform.position != newPosition)
                    {
                        var changedPlane = PutPlaneInPosition(newPosition);
                        // _toStitchLater.Add(changedPlane);
                        StitchPlaneAt(changedPlane, newPosition);
                    }
                }
                else
                {
                    var changedPlane = CreateNewPlane(newPosition);
                    // _toStitchLater.Add(changedPlane);
                    StitchPlaneAt(changedPlane, newPosition);
                }
            }
        }

        foreach (var planeToStitch in _toStitch)
        {
            StitchPlaneAt(planeToStitch, planeToStitch.transform.position);
        }

        _toStitch.Clear();
        var temp = _toStitch;
        _toStitch = _toStitchLater;
        _toStitchLater = temp;
    }

    private GameObject PutPlaneInPosition(Vector3 nextPlanePosition)
    {
        var modPosition = ModPosition(nextPlanePosition);
        var plane = _world[modPosition];
        var planeTransform = plane.transform;
        // if (planeTransform.position != nextPlanePosition)
        // {
        planeTransform.position = nextPlanePosition;
        // }

        return plane;
    }

    private Vector3 ModPosition(Vector3 worldPosition)
    {
        return new Vector3(
            worldPosition.x % (WorldSize * GridSize),
            worldPosition.y,
            worldPosition.z % (WorldSize * GridSize)
        );
    }

    private GameObject CreateNewPlane(Vector3 nextPlanePosition)
    {
        var terrain = Instantiate(terrainTemplate);
        terrain.transform.position = nextPlanePosition;

        _planes.Add(terrain.transform);
        _planeExistsByPosition.Add(AlignToGrid(nextPlanePosition));
        _world[ModPosition(nextPlanePosition)] = terrain;

        terrain.GetComponentInChildren<ProceduralSnow>().GeneratePlane();

        return terrain;
    }

    private Vector3 AlignToGrid(Vector3 position)
    {
        return new Vector3(
            position.x - (position.x % GridSize),
            0f,
            position.z - (position.z % GridSize));
    }

    public void StitchPlaneAt(GameObject plane, Vector3 worldPosition)
    {
        var hits = Physics.OverlapBox(plane.transform.position, Vector3.one * (GridSize * 2f));
        foreach (var hit in hits)
        {
            // var hitWorldPosition = hit.gameObject.transform.position;
            // var hitModPosition = ModPosition(hitWorldPosition);
            // if (_world.ContainsKey(hitModPosition))
            // {
                // Stitch(_world[hitModPosition].GetComponentInChildren<MeshFilter>(),
                //     plane.GetComponentInChildren<MeshFilter>());
            // }

            if (hit.GetComponentInChildren<ProceduralSnow>())
            {
                Stitch(plane.GetComponentInChildren<MeshFilter>(),
                hit.GetComponentInChildren<MeshFilter>());
            }
            
        }
    }

    public void Stitch(MeshFilter planeA, MeshFilter planeB)
    {
        var height = 2; //TODO: Use average of both heights edge average height
        var thresholdValue = 1;

        Matrix4x4 localToWorldA = planeA.transform.localToWorldMatrix;
        var planeAMesh = planeA.mesh;
        var verticesA = planeAMesh.vertices;
        var thresholdA = GetThresholdVector(planeA, planeB, thresholdValue);
        var newVerticiesA = new Vector3[verticesA.Length];
        
        
        Matrix4x4 localToWorldB = planeB.transform.localToWorldMatrix;
        var planeBMesh = planeB.mesh;
        var verticesB = planeBMesh.vertices;
        var thresholdB = GetThresholdVector(planeB, planeA, thresholdValue);
        var newVerticiesB = new Vector3[verticesB.Length];
        
        
        for (var i = 0; i < verticesA.Length; i++)
        {
            var vertex = verticesA[i];
            Vector3 worldVertex = localToWorldA.MultiplyPoint3x4(vertex);

            if (WithinThreshold(worldVertex, planeA.transform.position, thresholdA))
            {
                vertex.y = verticesB[i].y;
            }
            // if (Vector3.Distance(worldVertex, thresholdA) < thresholdValue)
            // {
            // }
            newVerticiesA[i] = vertex - Vector3.zero;
        }

        // planeAMesh.vertices = verticesA;
        planeAMesh.vertices = newVerticiesA;
        planeAMesh.RecalculateNormals();
        planeAMesh.RecalculateBounds();
        planeA.GetComponent<MeshCollider>().sharedMesh = planeAMesh;

        for (var i = 0; i < verticesB.Length; i++)
        {
            var vertex = verticesB[i];
            Vector3 worldVertex = localToWorldB.MultiplyPoint3x4(vertex);
            
            if (WithinThreshold(worldVertex, planeB.transform.position, thresholdB))
            {
                var progress = 
                vertex.y = verticesA[i].y;
            }
            // if (Vector3.Distance(worldVertex, thresholdB) < thresholdValue)
            // {
            //     vertex.y = verticesA[i].y;
            // }
            newVerticiesB[i] = vertex - Vector3.zero;
        }
        
        
        planeBMesh.vertices = newVerticiesB;
        planeBMesh.RecalculateNormals();
        planeBMesh.RecalculateBounds();
        planeB.GetComponent<MeshCollider>().sharedMesh = planeBMesh;
    }

    private bool WithinThreshold(Vector3 worldVertex, Vector3 transformPosition, Vector3 thresholdB)
    {
        var thresholdValue = 1f;
        var thresholdPoint = transformPosition + (thresholdB * (GridSize * .5f - thresholdValue));
       
        if (thresholdB == Vector3.right)
        {
            return worldVertex.x > thresholdPoint.x;
        }
        else if (thresholdB == Vector3.back)
        {
            return worldVertex.z > thresholdB.z;
        }
        else if (thresholdB == Vector3.left)
        {
            return worldVertex.x < thresholdB.x;
        }
        else if (thresholdB == Vector3.up)
        {
            return worldVertex.z < thresholdB.z;
        }
        else
        {
            return false;
        }
    }

    public float NewHeight(Vector3 worldVertex, Vector3 transformPosition, Vector3 thresholdB)
    {
        //TODO: Implement should set correct height
        var thresholdValue = 1f;
        var endPoint = transformPosition + (thresholdB * (GridSize * .5f));
        var thresholdPoint = endPoint - (thresholdB * thresholdValue);
        if (thresholdB == Vector3.right)
        {
            var height = 0;
            var progress = (worldVertex.x - thresholdPoint.x) / (endPoint.x - thresholdPoint.x);
            // worldVertex.y < height 
            // return Mathf.Clamp(OutCirc(progress), 
            return 0f;
        }
        else if (thresholdB == Vector3.back)
        {
            return 0f;
        }
        else if (thresholdB == Vector3.left)
        {
            return 0f;
        }
        else if (thresholdB == Vector3.up)
        {
            return 0f;
        }
        else
        {
            return 0f;
        }
    }

    public static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);
    public static float OutCirc(float t) => 1 - InCirc(1 - t);
    
    private static Vector3 GetThresholdVector(MeshFilter planeA, MeshFilter planeB, int thresholdValue)
    {
        var directions = new[]
        {
            Vector3.forward,
            Vector3.right,
            Vector3.back,
            Vector3.left,
        };
        var closestDirectionIndex = 0;
        var closestDirection = Mathf.Infinity;
        for (var i1 = 0; i1 < directions.Length; i1++)
        {
            var distance = Vector3.Distance(planeA.transform.position + directions[i1], planeB.transform.position);
            if (distance < closestDirection)
            {
                closestDirectionIndex = i1;
                closestDirection = distance;
            }
        }

        // var thresholdA = (planeA.transform.position + directions[closestDirectionIndex] * GridSize * .5f) -
        //                  directions[closestDirectionIndex] * thresholdValue;
        return directions[closestDirectionIndex];
    }
}