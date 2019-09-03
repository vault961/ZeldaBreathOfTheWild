using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData
{
	public string	Name;
	//public int		Index;
	public int		Type;
	public int		Range;
	public int		Damage;
	public float	Cure;
	public float	HpUp;
	public float	HpDown;
	public float	DefenseUp;
	public string	Info;
}

[Serializable]
public class ItemTableData {

	const string tableDataPath = "Assets/Data/zeldaItem.json";
	public List<ItemData> objectStates = new List<ItemData>();
	public Dictionary<string, ItemData> itemAttributes = new Dictionary<string, ItemData>();

	public static ItemTableData LoadTableData()
	{
		TextAsset asset = Resources.Load("Texts/zeldaItem") as TextAsset;
		string jsonString = asset.text;
		ItemTableData tableData = JsonUtility.FromJson<ItemTableData>(jsonString);
		tableData.GenerateStates();
		return tableData;
	}

	public void GenerateStates()
	{
		foreach(var ItemAttribute in objectStates)
		{
			itemAttributes.Add(ItemAttribute.Name, ItemAttribute);
		}
	}
}
