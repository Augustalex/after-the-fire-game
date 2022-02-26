using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildForrestGenerator : MonoBehaviour
{
    public List<GameObject> treeTemplates = new List<GameObject>();

    private HashSet<Vector3> _treeExistsByPosition = new HashSet<Vector3>();

    public void GenerateForrestOnPlane()
    {
        var position = transform.position;
        var planeRadius = ProceduralLandscapeGenerator.GridSize * 2f;

        var count = Random.Range(4, 12);
        for (var i = 0; i < count; i++)
        {
            var tryingCount = 0;
            var trying = true;
            while (trying)
            {
                var randomPosition = AlignToSubGrid(new Vector3(
                    Random.Range(position.x - planeRadius, position.x + planeRadius),
                    6f,
                    Random.Range(position.z - planeRadius, position.z + planeRadius)
                ));
                if (_treeExistsByPosition.Contains(AlignToGrid(randomPosition))) continue;

                if (Physics.Raycast(new Ray(randomPosition, Vector3.down), out var hitInfo, 10f,
                    (1 << LayerMask.NameToLayer("Island") | (1 << LayerMask.NameToLayer("Ice")))))
                {
                    if (hitInfo.collider.CompareTag("Island"))
                    {
                        var leveledPosition = new Vector3(
                            randomPosition.x,
                            hitInfo.point.y,
                            randomPosition.z
                        );

                        var randomRotation = Quaternion.Euler(0f, Random.Range(0f, 359f), 0f);
                        GameObject treeTemplate = treeTemplates[Random.Range(0, treeTemplates.Count)];
                        Instantiate(treeTemplate, leveledPosition, randomRotation, transform);
                        _treeExistsByPosition.Add(AlignToGrid(leveledPosition));
                    }
                }

                tryingCount += 1;
                if (tryingCount > 4) trying = false;
            }
        }
    }


    private Vector3 AlignToGrid(Vector3 position)
    {
        var gridSize = 3.2f;

        return new Vector3(
            position.x - (position.x % gridSize),
            0f,
            position.z - (position.z % gridSize));
    }

    private Vector3 AlignToSubGrid(Vector3 position)
    {
        var gridSize = 1.5f;
        var randomOffset = Random.insideUnitCircle;

        return new Vector3(
            position.x - (position.x % gridSize),
            position.y,
            position.z - (position.z % gridSize)) + new Vector3(randomOffset.x, 0, randomOffset.y);
    }
}