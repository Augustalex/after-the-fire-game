using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralSnow : MonoBehaviour
{
    public bool recalculateNormals = false;

    private bool _started;
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private Rigidbody _playerRigidbody;

    public float snowHeight = .9f;
    
    private PlayerController _player;

    private Vector3[] _originalVertices;
    private MeshCollider _meshCollider;

    private const int VectorRowCount = 50;

    private const float GridCullingMargin = .5f;
    private const float LocalCullingDistance = 5f;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _mesh = _meshFilter.mesh;
    }

    private void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        _playerRigidbody = _player.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(_player.transform.position, transform.position) >
            ProceduralLandscapeGenerator.GridSize + GridCullingMargin) return;
        
        DeformVerticies();
    }

    public void SetStartHeight()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        
        var originalVertices = _mesh.vertices;
        var vertices = new Vector3[originalVertices.Length];

        var noise = new FractalNoise(1,1,1);

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = originalVertices[i];
          
            Vector3 worldVertex = localToWorld.MultiplyPoint3x4(vertex);
         
            var noiseScale = .05f;
            var noiseAmplitude = 4f;
            vertex.y = 0 - noise.BrownianMotion(worldVertex.x * noiseScale, worldVertex.z * noiseScale) * noiseAmplitude;
            
            vertices[i] = vertex;
        }
        
        _originalVertices = vertices.Select(v =>  new Vector3(v.x, v.y, v.z)).ToArray();
        
        _mesh.vertices = vertices;
        
        if (recalculateNormals)
            _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        
        _meshCollider.sharedMesh = _mesh;
    }

    public void DeformVerticies()
    {
        var velocityVector = _playerRigidbody.velocity;
        var velocity = velocityVector.magnitude;
        
        var playerMoving = _player.Moving();
        var playerBoosting = _player.Boosting();
        var boostFactor = Mathf.Clamp(_player.BoostJuice() / 3f, 0f, 1f);
        var playerFalling = velocityVector.y < -4f;

        Matrix4x4 localToWorld = transform.localToWorldMatrix;

        var currentVerticies = _mesh.vertices;
        // var vertices = new Vector3[currentVerticies.Length];
        var vertices = currentVerticies;

        var passUntil = -1;

        for (var i = 0; i < currentVerticies.Length; i++)
        {
            var vertex = currentVerticies[i];
            Vector3 worldVertex = localToWorld.MultiplyPoint3x4(vertex);

            if (passUntil <= i)
            {
                if (velocity > 0f)
                {
                    float staticSpeed;
                    Vector3 playerMorphPoint;
                    
                    if (playerFalling)
                    {
                        playerMorphPoint = _player.transform.position + velocityVector.normalized * -.15f;
                        staticSpeed = _player.transform.localScale.x * velocity * .7f;
                    }
                    else if (!playerMoving)
                    {
                        playerMorphPoint = _player.transform.position + velocityVector.normalized * -.05f;
                        staticSpeed = _player.transform.localScale.x * velocity * .2f;
                    }
                    else if (playerBoosting)
                    {
                        playerMorphPoint = _player.transform.position + velocityVector.normalized * .05f;
                        
                        var boostSpeed = .4f + (boostFactor * .4f);
                        staticSpeed = _player.transform.localScale.x * velocity * boostSpeed;
                    }
                    else
                    {
                        playerMorphPoint = _player.transform.position + velocityVector.normalized * -.1f;
                        staticSpeed = _player.transform.localScale.x * velocity * .4f;
                    }

                    var pointDistance = Vector3.Distance(
                        new Vector3(playerMorphPoint.x, worldVertex.y, playerMorphPoint.z),
                        worldVertex);

                    var rowDistance = playerMorphPoint.x - worldVertex.x;
                    if (rowDistance < 0 && Mathf.Abs(rowDistance) > LocalCullingDistance)
                    {
                        passUntil = i + VectorRowCount - (i % VectorRowCount) + 1;
                    }

                    if (Mathf.Abs(rowDistance) > LocalCullingDistance)
                    {
                        var edgeSize = ProceduralLandscapeGenerator.GridSize / (VectorRowCount - 1);
                        var left = pointDistance - LocalCullingDistance;
                        var edges = (int) Mathf.Floor(left / edgeSize);
                        passUntil = i + edges;
                    }

                    if (pointDistance < LocalCullingDistance && (playerMorphPoint.y - worldVertex.y) < _player.transform.localScale.x * .8f)
                    {
                        var baseHoleSize = .45f; //.55f
                        var boostHoleSize = baseHoleSize + (boostFactor * .1f);
                        var holeSize = _player.transform.localScale.x * (playerFalling ? .7f : playerBoosting ? boostHoleSize : baseHoleSize);
                        var t = Mathf.Clamp(pointDistance, 0f, holeSize);
                        var distanceFactorFromHoleCenter = Mathf.Clamp(t / holeSize, 0f, 1f);
                        var height = Mathf.Clamp(OutCirc(1 - distanceFactorFromHoleCenter), 0f, 1f);

                        var originalHeight = _originalVertices[i].y;

                        var currentDepth = originalHeight - vertex.y;
                        var progress = ((currentDepth) / snowHeight);
                        var digSpeed = Mathf.Max(0, staticSpeed * InExpo(1 - progress)) * height;

                        var maxDepth = originalHeight - snowHeight;
                        vertex.y = Mathf.Clamp(vertex.y - (digSpeed * Time.fixedDeltaTime), maxDepth, originalHeight);
                    }
                }
            }

            vertices[i] = vertex;
        }

        // Debug.Log("PASS TOTAL: " + passTotal);
        _mesh.vertices = vertices;

        if (recalculateNormals)
            _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshCollider.sharedMesh = _mesh;
    }


    public static float OutSine(float t) => (float) Math.Sin(t * Math.PI / 2);

    public static float InExpo(float t) => (float) Math.Pow(2, 10 * (t - 1));

    public static float OutExpo(float t) => 1 - InExpo(1 - t);

    public static float InCirc(float t) => -((float) Math.Sqrt(1 - t * t) - 1);

    public static float OutCirc(float t) => 1 - InCirc(1 - t);

    public static float EaseInExpo(float x)
    {
        return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
    }

    public static float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
        // return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    public static float InBack(float t)
    {
        float s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }

    public static float OutBack(float t) => 1 - InBack(1 - t);

    public static float InOutBack(float t)
    {
        if (t < 0.5) return InBack(t * 2) / 2;
        return 1 - InBack((1 - t) * 2) / 2;
    }

    public static float OutBounce(float t)
    {
        float div = 2.75f;
        float mult = 7.5625f;

        if (t < 1 / div)
        {
            return mult * t * t;
        }
        else if (t < 2 / div)
        {
            t -= 1.5f / div;
            return mult * t * t + 0.75f;
        }
        else if (t < 2.5 / div)
        {
            t -= 2.25f / div;
            return mult * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / div;
            return mult * t * t + 0.984375f;
        }
    }

    public void GeneratePlane()
    {
        var gridSize = ProceduralLandscapeGenerator.GridSize;
        var vectorsRowCount = VectorRowCount;
        var densityFactor = ((gridSize) / (vectorsRowCount - 1));
        var offset = (gridSize + 1) / 2f;

        int width = vectorsRowCount - 1;
        int depth = vectorsRowCount - 1;

        // Defining triangles.
        int[] triangles = new int[width * depth * 2 * 3]; // 2 - polygon per quad, 3 - corners per polygon
        for (int d = 0; d < depth; d++)
        {
            for (int w = 0; w < width; w++)
            {
                // quad triangles index.
                int ti = (d * (width) + w) * 6; // 6 - polygons per quad * corners per polygon
                // First tringle
                triangles[ti] = (d * (width + 1)) + w;
                triangles[ti + 1] = ((d + 1) * (width + 1)) + w;
                triangles[ti + 2] = ((d + 1) * (width + 1)) + w + 1;
                // Second triangle
                triangles[ti + 3] = (d * (width + 1)) + w;
                triangles[ti + 4] = ((d + 1) * (width + 1)) + w + 1;
                triangles[ti + 5] = (d * (width + 1)) + w + 1;
            }
        }

        var gridRadius = ProceduralLandscapeGenerator.GridSize * .5f;

        // Defining vertices.
        Vector3[] vertices = new Vector3[(vectorsRowCount) * (vectorsRowCount)];
        int i = 0;
        for (int d = 0; d < vectorsRowCount; d++)
        {
            for (int w = 0; w < vectorsRowCount; w++)
            {
                var scaleW = w * densityFactor;
                var scaleD = d * densityFactor;
                vertices[i] =
                    new Vector3(scaleW, 0, scaleD) - new Vector3(offset, 0, offset);

                i++;
            }
        }

        // Defining UV.
        Vector2[] uv = new Vector2[vertices.Length];
        for (var i1 = 0; i1 < vertices.Length; i1++)
        {
            var vertex = vertices[i1];
            uv[i1] = new Vector2(vertex.x, vertex.z);
        }

        _originalVertices = vertices;

        // Creating a mesh object.
        Mesh mesh = new Mesh();

        // Assigning vertices, triangles and UV to the mesh.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        // Assigning mesh to mesh filter to display it.
        _meshFilter.mesh = mesh;
        _mesh = mesh;

        if (recalculateNormals)
            _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshCollider.sharedMesh = _mesh;

        // SetStartHeight();
    }
}