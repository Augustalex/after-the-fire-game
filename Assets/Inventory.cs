using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public GameObject inventorySlotTemplate;

    private const float RowOffset = 25f;
    private const float ColumnOffset = 25f;
    private const int MaxSlots = 8;
    private const int ColumnCount = 2;

    private readonly List<GameObject> _items = new List<GameObject>();
    private RectTransform _rect;

    void Start()
    {
        _rect = GetComponent<RectTransform>();
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    public void Refresh()
    {
        foreach (var item in _items)
        {
            Destroy(item);
        }

        _items.Clear();
        if (playerInventory.GetCones() > 0)
        {
            AddToInventory(IconLibrary.Instance.pineConeIcon, playerInventory.GetCones());
        }

        if (playerInventory.GetWorms() > 0)
        {
            AddToInventory(IconLibrary.Instance.beetleIcon, playerInventory.GetWorms());
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
        var index = _items.Count();
        var newItem = Instantiate(inventorySlotTemplate, Vector3.zero, Quaternion.identity, transform);
        var rect = newItem.GetComponent<RectTransform>();
        //
        // var rowCount = (float) MaxSlots / (float) ColumnCount;
        // var newHeight = (_rect.rect.height / rowCount) - RowOffset * 2f;
        // rect.sizeDelta = new Vector2(-newHeight, -newHeight);
        //
        // var x = ((newHeight + ColumnOffset) * (index % ColumnCount));
        // var y = (newHeight * index + RowOffset * index);
        // Debug.Log($"x: {x}, y: {y}");
        // rect.anchoredPosition = new Vector2(x, y);

        var slot = newItem.GetComponent<InventorySlot>();

        _items.Add(newItem);

        return slot;
    }
}