using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    public Transform player;

    public float scale = 1.0f;
    public bool recalculateNormals = false;

    private bool _started;
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private Rigidbody _playerRigidbody;

    public float snowHeight = .9f;

    private float _playerDiameter;
    private float _snowHeight;
    private float _wakeHeight;
    private float _wakeGap = 60;
    private Transform _player;

    private List<ContactPoint> _contactsQueue = new List<ContactPoint>();
    private Vector3[] _originalVertices;
    private MeshCollider _meshCollider;
    private float _cullingLength;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _mesh = _meshFilter.mesh;

        _player = FindObjectOfType<PlayerController>().transform;
        _playerRigidbody = _player.GetComponent<Rigidbody>();

        _cullingLength = transform.localScale.x * 2.1f;
    }    

    private void FixedUpdate()
    {
        if (!_started)
        {
            _playerDiameter = _player.localScale.x;
            // _snowHeight = _playerDiameter * .8f;
            // _wakeHeight = _playerDiameter * .9f;

            // Very deep snow
            // _snowHeight = 1.3f;
            // _wakeHeight = 1.2f;

            // Normal deep snow
            // _snowHeight = .9f;
            // _wakeHeight = .75f;

            // _snowHeight = snowHeight;
            _snowHeight = 0;
            _wakeHeight = 1f * snowHeight;

            _started = true;
            SetStartHeight();
        }

        if (Vector3.Distance(_player.position, transform.position) > _cullingLength) return;
        
        DeformVerticies();

        // TODO: Move to player class
        if (_playerRigidbody.velocity.magnitude > 1f)
        {
            var maxSize = 4.5f;
        
            var currentSize = _player.localScale.x;
            var growth = .004f * Time.deltaTime;
            var nextSize = currentSize + growth;
            var progress = ((nextSize - 1) / (maxSize - 1));
        
            _player.localScale = Vector3.one * Mathf.Min(maxSize, nextSize);
            _playerDiameter = _player.localScale.x;
        }
    }

    public void SetStartHeight()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        
        var originalVertices = _mesh.vertices;
        var vertices = new Vector3[originalVertices.Length];

        var firstNoiseOffset = new Vector2(0, 0);
        var secondNoiseOffset = new Vector2(1.5f,1.5f);
        var thirdNoiseOffset = new Vector2(3.5f,3.5f);

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = originalVertices[i];
          
            Vector3 worldVertex = localToWorld.MultiplyPoint3x4(vertex);
            var noiseScale = .95f;
            var noiseAmplitude = 8f;
            var secondNoiseScale = .99f;
            var secondNoiseAmplitude = 4f;
            var thirdNoiseScale = .95f;
            var thirdNoiseAmplitude = 1f;
            var firstNoise = Mathf.PerlinNoise((worldVertex.x + firstNoiseOffset.x) * (1f - noiseScale), (worldVertex.z + firstNoiseOffset.y) * (1f - noiseScale));
            var secondNoise = Mathf.PerlinNoise((worldVertex.x + secondNoiseOffset.x) * (1f - secondNoiseScale), (worldVertex.z + secondNoiseOffset.y) * (1f - secondNoiseScale));
            var thirdNoise = Mathf.PerlinNoise((worldVertex.x + thirdNoiseOffset.x) * (1f - thirdNoiseScale), (worldVertex.z + thirdNoiseOffset.y) * (1f - thirdNoiseScale));
            // vertex.y = 0 - (noiseAmplitude * firstNoise + secondNoiseAmplitude * secondNoise + thirdNoiseAmplitude * thirdNoise);
            // vertex.y = 0 - noiseAmplitude * firstNoise * secondNoise * thirdNoise;
            vertex.y = 0 - noiseAmplitude * firstNoise;

            // vertex.y = Random.value;
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

        Matrix4x4 localToWorld = transform.localToWorldMatrix;

        var originalVertices = _mesh.vertices;
        var vertices = new Vector3[originalVertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = originalVertices[i];

            Vector3 worldVertex = localToWorld.MultiplyPoint3x4(vertex);

            if (velocity > 0f)
            {
                float staticSpeed;
                Vector3 pointPosition;
                if (velocityVector.y < -2f)
                {
                    pointPosition = _player.transform.position + velocityVector.normalized * -.15f;
                    staticSpeed = _player.transform.localScale.x * velocity * .75f;
                }
                else
                {
                    pointPosition = _player.transform.position + velocityVector.normalized * -.1f;
                    staticSpeed = _player.transform.localScale.x * velocity * .65f;
                }

                var pointDistance = Vector3.Distance(new Vector3(pointPosition.x, pointPosition.y, pointPosition.z),
                    worldVertex);
                var pointCheckDistance = _player.transform.localScale.x * .55f;
                //&& Math.Abs(pointPosition.y - worldVertex.y) < .6f
                if (pointDistance < pointCheckDistance)
                {
                    float height;
                    if (pointDistance < pointCheckDistance * .2f)
                    {
                        height = 1;
                    }
                    else
                    {
                        height = Mathf.Clamp(OutCirc(pointDistance / pointCheckDistance), 0, 1);
                    }

                    var originalHeight = _originalVertices[i].y;
                    
                    var currentDepth = originalHeight - vertex.y;
                    var progress = ((currentDepth) / snowHeight);
                    var digSpeed = Mathf.Max(0, staticSpeed * InExpo(1 - progress)) * height;
                    
                    var maxDepth = originalHeight - snowHeight;
                    vertex.y = Mathf.Clamp(vertex.y - (digSpeed * Time.fixedDeltaTime), maxDepth, originalHeight);
                }
            }

            vertices[i] = vertex;
        }

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

    public void AddPoints(ContactPoint[] otherContacts)
    {
        if (_contactsQueue.Count > 100) return;
        _contactsQueue.AddRange(otherContacts);
    }

    public void GeneratePlane()
    {
        // Multiplier for noise.
        float noisePower = 1;
        // Noise offset.
        Vector2 noiseOffset = Vector2.one;
        // Noise scale.
        float noiseScale = 1;
        
        int width = 10;
        int depth = 10;
        
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

        // Defining vertices.
        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)];
        int i = 0;
        for (int d = 0; d <= depth; d++)
        {
            for (int w = 0; w <= width; w++)
            {
                // Setting vertice position.
                vertices[i] = new Vector3(w, 0, d) - new Vector3(width / 2f, 0, depth / 2f);
                
                // Adding elevation from perlin noise.
                float noiseXCoord = noiseOffset.x + w / (float) width * noiseScale;
                float noiseYCoord = noiseOffset.y + d / (float) depth * noiseScale;
                vertices[i].y = (Mathf.PerlinNoise(noiseXCoord, noiseYCoord) - 0.5f) * noisePower;
                i++;
            }
        }
        
        // Defining UV.
        Vector2[] uv = new Vector2[(width + 1) * (depth + 1)];
        i = 0;
        for (int d = 0; d <= depth; d++)
        {
            for (int w = 0; w <= width; w++)
            {
                uv[i] = new Vector2(w / (float)width, d / (float)depth);
                i++;
            }
        }
        
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
        
        // _mesh.vertices = vertices;
    }
}