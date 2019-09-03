using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {
    public static InventorySystem instance = null;

    public GameObject player;

    //public static InventorySystem Call() {
    //    if(_instance == null){
    //        _instance = this;
    //    }
    //    return _instance;
    //}

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (instance == null) {
            instance = this;
            //Debug.Log(instance);
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

		for(int i = (int)ItemType.TYPE_WEAPON; i <= (int)ItemType.TYPE_CONSUME; ++i)
		{
			itemLists.Add(new List<ITEM.Item>());
		}
		DontDestroyOnLoad(gameObject);
    }
    
	public List<List<ITEM.Item>> itemLists = new List<List<ITEM.Item>>();

    public void AddItem(ITEM.Item item) {
        if(item.itemData.Type == (int)ItemType.TYPE_CONSUME)
		{
			var consumeInven = itemLists[(int)ItemType.TYPE_CONSUME];
			bool isFind = false;
			foreach (var c in consumeInven)
			{
				if (c.itemData.Name == item.itemData.Name)
				{
					c.count++;
					isFind = true;
				}
			}
			if (!isFind) { itemLists[item.itemData.Type].Add(item);  }
		}
		else
		{
			itemLists[item.itemData.Type].Add(item);
		}
    }

    public void RemoveItem(ItemType type, int index, int count = 1) {
		if (type == ItemType.TYPE_CONSUME)
		{
            itemLists[(int)type][index].count--;                        //먹는거 카운트 줄여준다

            if (itemLists[(int)type][index].count == 0)                 //0이 되면 삭제 맞나?
                itemLists[(int)type].RemoveAt(index);
        }
		else
		{
			itemLists[(int)type].RemoveAt(index);
		}
    }

	public ITEM.Item GetItemFromInventory(ItemType type, int index)
	{
		return itemLists[(int)type][index];
	}
}
