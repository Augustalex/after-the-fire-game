using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameInventory : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public GameObject inventorySlotTemplate;

    private const int MaxSlots = 8;

    private readonly List<GameObject> _items = new List<GameObject>();

    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();

        playerInventory.Updated += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        foreach (var item in _items)
        {
            Destroy(item);
        }

        _items.Clear();
        AddToInventory(IconLibrary.Instance.beetleIcon, playerInventory.GetWorms());

        if (playerInventory.GetCones() > 0)
        {
            AddToInventory(IconLibrary.Instance.pineConeIcon, playerInventory.GetCones());
        }
        
        if (playerInventory.GetLogs() > 0)
        {
            AddToInventory(IconLibrary.Instance.logIcon, playerInventory.GetLogs());
        }

        var leftToAdd = MaxSlots - _items.Count;
        for (int i = 0; i < leftToAdd; i++)
        {
            AddEmptySlot();
        }
    }

    private void AddEmptySlot()
    {
        var slot = CreateSlot();
        slot.emptySlot = true;
    }

    private void AddToInventory(Texture icon, int count)
    {
        var slot = CreateSlot();
        slot.icon = icon;
        slot.count = count;
    }

    private InventorySlot CreateSlot()
    {
        var newItem = Instantiate(inventorySlotTemplate, Vector3.zero, Quaternion.identity, transform);
        var slot = newItem.GetComponent<InventorySlot>();

        _items.Add(newItem);

        return slot;
    }
}