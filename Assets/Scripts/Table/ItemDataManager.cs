using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemDataManager : MonoBehaviour {

	public static ItemDataManager instance;

	ItemTableData tableData;

	void Awake()
	{
		if(instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			tableData = ItemTableData.LoadTableData();
		}
	}

	public ItemData GetObjectState(string objectName)
	{
        if (tableData.itemAttributes.ContainsKey(objectName) == false)
        {
            return null;
        }
		ItemData itemAttribute = tableData.itemAttributes[objectName];
		return itemAttribute;
	}
}
