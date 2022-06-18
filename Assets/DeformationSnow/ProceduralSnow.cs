using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeformationSnow;
using Player;
using UnityEngine;

public class ProceduralSnow : MonoBehaviour
{
    private const int VectorRowCount = 42;

    private const float GridCullingMargin = .5f;
    private const float LocalCullingDistance = 5f;
    public bool recalculateNormals;

    public float snowHeight = .9f;
    public GenerationData generationData;
    public SnowDeformationData snowDeformationData;
    private bool _doneGenerating;
    private Mesh _mesh;
    private MeshCollider _meshCollider;
    private MeshFilter _meshFilter;

    private readonly Queue<Vector3[]> _meshPasses = new Queue<Vector3[]>();

    private Vector3[] _originalVertices;

    private PlayerBallMover _ballMover;
    private PlayerGrower _playerGrower;
    private PlayerSize _playerSize;
    private PlayerModeController _playerModeController;

    private Rigidbody _playerRigidbody;
    private bool _setup;

    private bool _started;
    private Mesh _virtualMesh;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _mesh = _meshFilter.mesh;
        _virtualMesh = Instantiate(_mesh);
        _virtualMesh.vertices = _mesh.vertices;
    }

    private void Start()
    {
        _playerModeController = FindObjectOfType<PlayerModeController>();
    }

    private void Update()
    {
        if (!_doneGenerating) return;
        if (!_playerModeController.IsSnowBall()) return;
        if (!_setup)
        {
            _ballMover = _playerModeController.GetComponentInChildren<PlayerBallMover>();
            _playerGrower = _playerModeController.GetComponentInChildren<PlayerGrower>();
            _playerSize = _playerModeController.GetComponentInChildren<PlayerSize>();
            _playerRigidbody = _playerGrower.GetComponent<SphereCollider>().attachedRigidbody;
            _setup = true;
        }

        if (Vector3.Distance(_ballMover.transform.position, transform.position) <
            ProceduralLandscapeGenerator.GridSize + GridCullingMargin)
            DeformVerticies();
    }

    public void SetStartHeight()
    {
        var localToWorld = transform.localToWorldMatrix;

        var originalVertices = _mesh.vertices;
        var vertices = new Vector3[originalVertices.Length];

        var noise = new FractalNoise(1, 1, 1);

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = originalVertices[i];

            var worldVertex = localToWorld.MultiplyPoint3x4(vertex);

            vertex.y = MultipliedMixedNoiseHeight(worldVertex, noise);

            vertices[i] = vertex;
        }

        _originalVertices = vertices.Select(v => new Vector3(v.x, v.y, v.z)).ToArray();

        _mesh.vertices = vertices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _virtualMesh.vertices = vertices;
        _virtualMesh.RecalculateNormals();
        _virtualMesh.RecalculateBounds();
        _meshCollider.sharedMesh = _virtualMesh;
    }

    private float PerlinNoiseHeight(Vector3 worldVertex, FractalNoise noise)
    {
        var noiseScale = generationData.perlinNoiseScale;
        var noiseAmplitude = generationData.perlinNoiseAmplitude;
        var perlinNoise = noise.BrownianMotion(worldVertex.x * noiseScale, worldVertex.z * noiseScale) * noiseAmplitude;

        var totalNoise = perlinNoise + generationData.perlinNoiseOffset;

        return generationData.heightOffset - totalNoise;
    }

    private float MixedNoiseHeight(Vector3 worldVertex, FractalNoise noise)
    {
        var cellNoiseScale = generationData.cellNoiseScale;
        var cellNoiseAmplitude = generationData.cellNoiseAmplitude;
        var cellNoise = new FastNoiseLite();
        cellNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        var cellNoiseResult = cellNoise.GetNoise(worldVertex.x * cellNoiseScale, worldVertex.z * cellNoiseScale) *
                              cellNoiseAmplitude;

        var noiseScale = generationData.perlinNoiseScale;
        var noiseAmplitude = generationData.perlinNoiseAmplitude;
        var perlinNoise = noise.BrownianMotion(worldVertex.x * noiseScale, worldVertex.z * noiseScale) * noiseAmplitude;

        var totalNoise = Mathf.Min(-cellNoiseResult + generationData.cellNoiseOffset,
            perlinNoise + generationData.perlinNoiseOffset);

        return generationData.heightOffset - totalNoise;
    }

    private float MultipliedMixedNoiseHeight(Vector3 worldVertex, FractalNoise noise)
    {
        var cellNoise = new FastNoiseLite();
        cellNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);

        var cellNoiseScale = generationData.cellNoiseScale;
        var cellNoiseResult = cellNoise.GetNoise(worldVertex.x * cellNoiseScale, worldVertex.z * cellNoiseScale);

        var noiseScale = generationData.perlinNoiseScale;
        var perlinNoise = noise.BrownianMotion(worldVertex.x * noiseScale, worldVertex.z * noiseScale);

        var noiseAmplitude = generationData.perlinNoiseAmplitude;
        var totalNoise = (1 - cellNoiseResult) * perlinNoise * noiseAmplitude + generationData.perlinNoiseOffset;

        return generationData.heightOffset - totalNoise;
    }

    private float InterpolatedDeform(Vector3 previousPosition, Vector3 currentPosition, Vector3 originalVertex,
        Vector3 vertex, Vector3 worldVertex, float staticDigSpeed, float playerScaleX, bool playerBoosting,
        bool playerFalling, float boostFactor)
    {
        var steps = 1f;

        var direction = (currentPosition - previousPosition).normalized;
        var length = Vector3.Distance(previousPosition, currentPosition);
        var stepSize = length / steps;

        var vertexY = vertex.y;
        for (var i = 0; i < steps; i++)
        {
            var morphPoint = previousPosition + direction * ((i + 1) * stepSize);
            vertexY = DeformAtPoint(
                originalVertex, vertex, worldVertex, staticDigSpeed, morphPoint, playerScaleX, playerBoosting,
                playerFalling, boostFactor
            );
        }

        return vertexY;
    }

    private float DeformAtPoint(Vector3 originalVertex, Vector3 vertex, Vector3 worldVertex, float staticDigSpeed,
        Vector3 playerMorphPoint, float playerScaleX, bool playerBoosting, bool playerFalling, float boostFactor)
    {
        var playerVertexAlignedPoint = new Vector3(playerMorphPoint.x, worldVertex.y, playerMorphPoint.z);
        var pointDistance = Vector3.Distance(
            playerVertexAlignedPoint,
            worldVertex);

        var baseHoleSize = .5f; //.45f
        var boostHoleSize = baseHoleSize + boostFactor * .05f;
        var holeSize = playerScaleX *
                       (playerFalling ? .7f : playerBoosting ? boostHoleSize : baseHoleSize);
        var t = Mathf.Clamp(pointDistance, 0f, holeSize);
        var distanceFactorFromHoleCenter = Mathf.Clamp(t / holeSize, 0f, 1f);
        var height = Mathf.Clamp(OutCirc(1 - distanceFactorFromHoleCenter), 0f, 1f);

        var originalHeight = originalVertex.y;

        var currentDepth = originalHeight - vertex.y;
        var progress = currentDepth / snowHeight;
        var digSpeed = Mathf.Max(0, staticDigSpeed * InExpo(1 - progress)) * height;

        var maxDepth = originalHeight - snowHeight;

        vertex.y = Mathf.Clamp(vertex.y - digSpeed * Time.fixedDeltaTime, maxDepth, originalHeight);

        return vertex.y;
    }

    public void DeformVerticies()
    {
        var velocityVector = _playerRigidbody.velocity;
        var velocity = velocityVector.magnitude;

        var playerMoving = _ballMover.Moving();
        var playerBoosting = _ballMover.Boosting();

        // Deforming force based on boost
        // var boostFactor = Mathf.Clamp(_player.BoostJuice() / 3f, 0f, 1f);

        // Deforming force based on time spent continuously on the move
        var moveTimeFactor = Mathf.Clamp(_ballMover.TimeMoving() / 6f, 0f, 1f);
        var boostFactor = Mathf.Clamp(_ballMover.BoostJuice() / 3f, 0f, 1f);

        var maxSizeReached = _playerGrower.MaxSizeReached();
        var playerFalling = false; //velocityVector.y < -4f;
        var playerPreviousPosition = _ballMover.GetPreviousPosition();

        var localToWorld = transform.localToWorldMatrix;

        var currentVerticies = _virtualMesh.vertices;
        var vertices = new Vector3[currentVerticies.Length];
        // var vertices = currentVerticies;

        var passUntil = -1;

        var playerTransform = _ballMover.transform;
        var playerPosition = playerTransform.position;
        var playerScaleX = playerTransform.localScale.x;

        var treePoints = Physics.OverlapSphere(playerPosition, playerScaleX * .6f)
            .Where(c => c.CompareTag("Tree") || c.CompareTag("WaySign"))
            .ToArray();
        if (treePoints.Length > 0) return;
        if (velocity == 0) return;

        var speedScale = .5f;

        var playerRelease = _ballMover.Releasing();
        var playerReleasing = playerRelease > 0f && _playerSize.HasSnow();

        for (var i = 0; i < currentVerticies.Length; i++)
        {
            var vertex = currentVerticies[i];
            var worldVertex = localToWorld.MultiplyPoint3x4(vertex);

            if (passUntil <= i)
            {
                float staticSpeed;
                Vector3 playerMorphPoint;

                // if (playerFalling)
                // {
                //     playerMorphPoint = playerPosition + velocityVector.normalized * -.2f;
                //     staticSpeed = playerScaleX * velocity * .4f;
                // }
                // else if (!playerMoving)
                // {
                //     playerMorphPoint = playerPosition + velocityVector.normalized * -.05f;
                //     staticSpeed = playerScaleX * velocity * .2f;
                // }
                // else if (playerBoosting)
                // {
                //     playerMorphPoint = playerPosition + velocityVector * Time.fixedDeltaTime * 4f;
                //
                //     var boostSpeed = .1f;
                //     staticSpeed = playerScaleX * velocity * boostSpeed;
                // }
                // else
                // {
                playerMorphPoint = playerPosition + velocityVector * (Time.fixedDeltaTime * snowDeformationData.interpolationVelocityMultiplier);

                if (maxSizeReached)
                {
                    var moveTimeFactorSpeed = .3f + moveTimeFactor * .2f;
                    staticSpeed = playerScaleX * velocity * moveTimeFactorSpeed;
                }
                else
                {
                    var moveTimeFactorSpeed = .3f + moveTimeFactor * .3f;
                    staticSpeed = playerScaleX * velocity * moveTimeFactorSpeed;
                }
                // }

                staticSpeed *= speedScale;

                var playerVertexAlignedPoint = new Vector3(playerMorphPoint.x, worldVertex.y, playerMorphPoint.z);
                var pointDistance = Vector3.Distance(
                    playerVertexAlignedPoint,
                    worldVertex);

                var rowDistance = playerMorphPoint.x - worldVertex.x;
                if (rowDistance < 0 && Mathf.Abs(rowDistance) > LocalCullingDistance)
                    passUntil = i + VectorRowCount - i % VectorRowCount + 1;

                if (Mathf.Abs(rowDistance) > LocalCullingDistance)
                {
                    var edgeSize = ProceduralLandscapeGenerator.GridSize / (VectorRowCount - 1);
                    var left = pointDistance - LocalCullingDistance;
                    var edges = (int) Mathf.Floor(left / edgeSize);
                    passUntil = i + edges;
                }

                if (playerReleasing && pointDistance < LocalCullingDistance && _ballMover.TouchingSnow())
                {
                    var holeSize = playerScaleX * 1.5f;
                                   var t = Mathf.Clamp(pointDistance, 0f, holeSize);
                    var distanceFactorFromHoleCenter = Mathf.Clamp(t / holeSize, 0f, 1f);


                    var speeed = OutExpo((distanceFactorFromHoleCenter < .35f ? 1 : (1 - distanceFactorFromHoleCenter))) * playerRelease * 1.5f;
                    
                    // center: 1
                    // rim: 0
                    // middle: .5
                    // output = r * releaseSpeed
                    // var digSpeed = Mathf.Max(0, Mathf.Min(.1f, staticSpeed * OutExpo((1 - distanceFactorFromHoleCenter)));
                    
                    vertex.y += speeed * Time.fixedDeltaTime;
                    // vertex.y += playerRelease * Time.fixedDeltaTime;
                    _originalVertices[i].y = Math.Max(_originalVertices[i].y, vertex.y);
                }
                else if (pointDistance < LocalCullingDistance &&
                         playerMorphPoint.y - worldVertex.y < playerScaleX * .9f)
                {
                    vertex.y = InterpolatedDeform(playerPreviousPosition, playerMorphPoint, _originalVertices[i],
                        vertex, worldVertex, staticSpeed, playerScaleX, playerBoosting, playerFalling, boostFactor);
                    // var baseHoleSize = .45f; //.55f
                    // var boostHoleSize = baseHoleSize + (boostFactor * .1f);
                    // var holeSize = playerScaleX *
                    //                (playerFalling ? .7f : playerBoosting ? boostHoleSize : baseHoleSize);
                    // var t = Mathf.Clamp(pointDistance, 0f, holeSize);
                    // var distanceFactorFromHoleCenter = Mathf.Clamp(t / holeSize, 0f, 1f);
                    // var height = Mathf.Clamp(OutCirc(1 - distanceFactorFromHoleCenter), 0f, 1f);
                    //
                    // var originalHeight = _originalVertices[i].y;
                    //
                    // var currentDepth = originalHeight - vertex.y;
                    // var progress = ((currentDepth) / snowHeight);
                    // var digSpeed = Mathf.Max(0, staticSpeed * InExpo(1 - progress)) * height;
                    //
                    // var maxDepth = originalHeight - snowHeight;
                    //
                    // vertex.y = Mathf.Clamp(vertex.y - (digSpeed * Time.fixedDeltaTime), maxDepth, originalHeight);
                }
            }

            vertices[i] = vertex;
        }

        // Debug.Log("PASS TOTAL: " + passTotal);

        if (_meshPasses.Count > snowDeformationData.renderFrameLag
        ) // Rendered mesh is a few frames behind - this enables us to precalculate the collision, and corresponding rigidbody events, while rendering a smooth output
        {
            _mesh.vertices = _meshPasses.Dequeue();

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
        }

        _meshPasses.Enqueue(vertices);

        _virtualMesh.vertices = vertices;
        _virtualMesh.RecalculateBounds();
        _virtualMesh.RecalculateNormals();
        _meshCollider.sharedMesh = _virtualMesh;
    }


    public static float OutSine(float t)
    {
        return (float) Math.Sin(t * Math.PI / 2);
    }

    public static float InExpo(float t)
    {
        return (float) Math.Pow(2, 10 * (t - 1));
    }

    public static float OutExpo(float t)
    {
        return 1 - InExpo(1 - t);
    }

    public static float InCirc(float t)
    {
        return -((float) Math.Sqrt(1 - t * t) - 1);
    }

    public static float OutCirc(float t)
    {
        return 1 - InCirc(1 - t);
    }

    public static float EaseInExpo(float x)
    {
        return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
    }

    public static float EaseOutBack(float x)
    {
        var c1 = 1.70158f;
        var c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
        // return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    public static float InBack(float t)
    {
        var s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }

    public static float OutBack(float t)
    {
        return 1 - InBack(1 - t);
    }

    public static float InOutBack(float t)
    {
        if (t < 0.5) return InBack(t * 2) / 2;
        return 1 - InBack((1 - t) * 2) / 2;
    }

    public static float OutBounce(float t)
    {
        var div = 2.75f;
        var mult = 7.5625f;

        if (t < 1 / div) return mult * t * t;

        if (t < 2 / div)
        {
            t -= 1.5f / div;
            return mult * t * t + 0.75f;
        }

        if (t < 2.5 / div)
        {
            t -= 2.25f / div;
            return mult * t * t + 0.9375f;
        }

        t -= 2.625f / div;
        return mult * t * t + 0.984375f;
    }

    public IEnumerator GeneratePlane()
    {
        var gridSize = ProceduralLandscapeGenerator.GridSize;
        var vectorsRowCount = VectorRowCount;
        var densityFactor = gridSize / (vectorsRowCount - 1);
        var offset = (gridSize + 1) / 2f;

        var width = vectorsRowCount - 1;
        var depth = vectorsRowCount - 1;

        // Defining triangles.
        var triangles = new int[width * depth * 2 * 3]; // 2 - polygon per quad, 3 - corners per polygon
        for (var d = 0; d < depth; d++)
        for (var w = 0; w < width; w++)
        {
            // quad triangles index.
            var ti = (d * width + w) * 6; // 6 - polygons per quad * corners per polygon
            // First tringle
            triangles[ti] = d * (width + 1) + w;
            triangles[ti + 1] = (d + 1) * (width + 1) + w;
            triangles[ti + 2] = (d + 1) * (width + 1) + w + 1;
            // Second triangle
            triangles[ti + 3] = d * (width + 1) + w;
            triangles[ti + 4] = (d + 1) * (width + 1) + w + 1;
            triangles[ti + 5] = d * (width + 1) + w + 1;
        }

        yield return new WaitForEndOfFrame();

        var gridRadius = ProceduralLandscapeGenerator.GridSize * .5f;

        // Defining vertices.
        var vertices = new Vector3[vectorsRowCount * vectorsRowCount];
        var i = 0;
        for (var d = 0; d < vectorsRowCount; d++)
        {
            for (var w = 0; w < vectorsRowCount; w++)
            {
                var scaleW = w * densityFactor;
                var scaleD = d * densityFactor;
                vertices[i] =
                    new Vector3(scaleW, 0, scaleD) - new Vector3(offset, 0, offset);

                i++;
            }

            yield return new WaitForEndOfFrame();
        }

        // Defining UV.
        var uv = new Vector2[vertices.Length];
        for (var i1 = 0; i1 < vertices.Length; i1++)
        {
            var vertex = vertices[i1];
            uv[i1] = new Vector2(vertex.x, vertex.z);
        }

        yield return new WaitForEndOfFrame();

        _originalVertices = vertices;

        // Creating a mesh object.
        var mesh = new Mesh();

        // Assigning vertices, triangles and UV to the mesh.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        // Assigning mesh to mesh filter to display it.
        _meshFilter.mesh = mesh;
        _mesh = mesh;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _virtualMesh = Instantiate(_mesh);
        _virtualMesh.vertices = _mesh.vertices;
        _virtualMesh.triangles = _mesh.triangles;
        _virtualMesh.uv = _mesh.uv;
        _virtualMesh.RecalculateNormals();
        _virtualMesh.RecalculateBounds();

        _meshCollider.sharedMesh = _virtualMesh;

        SetStartHeight();

        _doneGenerating = true;
    }

    private float rnd2d(float n, float m)
    {
        //random proportion -1, 1
        var e = (n * m * 31.78694f + m) % 1;
        return e * e * 137.21321f % 1;
    }

    private float voronoi2(Vector3 vtx)
    {
        var px = Mathf.Floor(vtx.x);
        var pz = Mathf.Floor(vtx.z);
        var fx = vtx.x % 1;
        var fz = vtx.z % 1;

        var res = 8.0f;
        for (var j = -1; j <= 1; j++)
        for (var i = -1; i <= 1; i++)
        {
            var rx = i - fx + rnd2d(px + i, pz + j);
            var rz = j - fz + rnd2d(px + i, pz + j);
            var d = Mathf.Sqrt(rx * rx + rz * rz);

            res += Mathf.Exp(-32.0f * d);
        }

        return -(1.0f / 32.0f) * Mathf.Log(res);
    }

    public class FractalNoise
    {
        private readonly float[] m_Exponent;
        private readonly int m_IntOctaves;
        private readonly float m_Lacunarity;
        private readonly float m_Octaves;

        public FractalNoise(float inH, float inLacunarity, float inOctaves)
        {
            m_Lacunarity = inLacunarity;
            m_Octaves = inOctaves;
            m_IntOctaves = (int) inOctaves;
            m_Exponent = new float[m_IntOctaves + 1];
            var num = 1f;
            for (var index = 0; index < m_IntOctaves + 1; ++index)
            {
                m_Exponent[index] = (float) Math.Pow(m_Lacunarity, -(double) inH);
                num *= m_Lacunarity;
            }
        }

        public float HybridMultifractal(float x, float y, float offset)
        {
            var num1 = (Mathf.PerlinNoise(x, y) + offset) * m_Exponent[0];
            var num2 = num1;
            x *= m_Lacunarity;
            y *= m_Lacunarity;
            int index;
            for (index = 1; index < m_IntOctaves; ++index)
            {
                if (num2 > 1.0)
                    num2 = 1f;
                var num3 = (Mathf.PerlinNoise(x, y) + offset) * m_Exponent[index];
                num1 += num2 * num3;
                num2 *= num3;
                x *= m_Lacunarity;
                y *= m_Lacunarity;
            }

            var num4 = m_Octaves - m_IntOctaves;
            return num1 + num4 * Mathf.PerlinNoise(x, y) * m_Exponent[index];
        }

        public float RidgedMultifractal(float x, float y, float offset, float gain)
        {
            var num1 = Mathf.Abs(Mathf.PerlinNoise(x, y));
            var num2 = offset - num1;
            var num3 = num2 * num2;
            var num4 = num3;
            for (var index = 1; index < m_IntOctaves; ++index)
            {
                x *= m_Lacunarity;
                y *= m_Lacunarity;
                var num5 = Mathf.Clamp01(num3 * gain);
                var num6 = Mathf.Abs(Mathf.PerlinNoise(x, y));
                var num7 = offset - num6;
                num3 = num7 * num7 * num5;
                num4 += num3 * m_Exponent[index];
            }

            return num4;
        }

        public float BrownianMotion(float x, float y)
        {
            var num1 = 0.0f;
            long index;
            for (index = 0L; index < (long) m_IntOctaves; ++index)
            {
                num1 = Mathf.PerlinNoise(x, y) * m_Exponent[index];
                x *= m_Lacunarity;
                y *= m_Lacunarity;
            }

            var num2 = m_Octaves - m_IntOctaves;
            return num1 + num2 * Mathf.PerlinNoise(x, y) * m_Exponent[index];
        }
    }
}