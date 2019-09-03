using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.ScrollSnaps;
using UnityEngine.UI;
using UnityEngine.Sprites;
using ITEM;
using UnityEngine.EventSystems;

public class SelectItem : MonoBehaviour {

    public DirectionalScrollSnap _scrollSnap;

    public InventorySystem _inventorySystem;
    private List<string> _imageName = new List<string>();

    List<ItemSlotGroup> slotGroupList = new List<ItemSlotGroup>();
    private List<Item> itemList = new List<Item>();

    // public EventSystem _eventSystem;
    private void Awake()
    {
        foreach (Transform child in transform)
        {
            slotGroupList.Add(child.GetComponent<ItemSlotGroup>());
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < 4; i++)
        {
            SlotGroupUpdate(i);
        }
    }

    public void SlotGroupUpdate(int InvenType)
    {
        ItemSlotGroup slotGroup = slotGroupList[InvenType];
        itemList = _inventorySystem.itemLists[InvenType];
        for (int i = 0; i < slotGroup.slots.Count; i++)
        {
            ItemSlot slot = slotGroup.slots[i].GetComponent<ItemSlot>();
            slot._index = i + 1;
            if (itemList.Count == 0 || i > itemList.Count - 1)
            {
                //slot.SetItemData();
                return;
            } 

            foreach (var item in itemList)
            {
                slot.SetItemData(item);

                //if(item)
            }

        }
    }
}
