using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TreePlanter : MonoBehaviour
{
    public GameObject seedTemplate;
    private PlayerModeController _playerModeController;
    private PlayerInventory _playerInventory;

    void Awake()
    {
        _playerInventory = GetComponent<PlayerInventory>();
        _playerModeController = GetComponent<PlayerModeController>();
    }

    public void OnUse(InputValue value)
    {
        if (value.isPressed)
        {
            var islandGameObject = GetNearbyIslandOrNull();
            if (islandGameObject)
            {
                var island = islandGameObject.GetComponentInParent<Island>();
                
                if (CanUse(island))
                {
                    Use(island);
                }
            }
        }
    }

    public bool CanUse(Island island)
    {
        return _playerInventory.GetCones() > 0 &&
               !_playerModeController.HogInAir() &&
               island.GetComponentInParent<Island>().CanGrowTrees();
    }

    public void Use(Island island)
    {
        var taken = _playerInventory.TryGetCones(1);
        if (taken == 1)
        {
            var seed = Instantiate(seedTemplate);
            seed.transform.position = _playerModeController.GetPlayerFeetPosition();
            island.PlantedTree();
        }
    }

    private GameObject GetNearbyIslandOrNull()
    {
        var islandColliderOrNull = Physics.OverlapSphere(_playerModeController.GetPlayerFeetPosition(), 2f)
            .FirstOrDefault(hit => hit.CompareTag("Island"));

        if (islandColliderOrNull != null)
        {
            return islandColliderOrNull.gameObject;
        }

        return null;
    }
}