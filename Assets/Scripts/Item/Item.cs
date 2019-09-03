using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
    TYPE_WEAPON,
    TYPE_SHIELD,
    TYPE_STUFF,
    TYPE_CONSUME,
}

public enum WeaponType
{
    SWORD,
    AXE,
    STICK,
}

namespace ITEM
{
	[System.Serializable]
	public class Item
    {

        public Sprite DefaultImg;
        public int count = 1;
        public int maxCount;

		//public string itemName;
		public ItemData itemData;

		//public ItemType		itemType;
		//public int			Range;
		//public float		Damage;
		//public float		Cure;
		//public float		HpUp;
		//public float		HpDown;
		//public float		DefenceUp;
		//public string		Info;

		
        
        public Item(string name) {
            itemData = ItemDataManager.instance.GetObjectState(name);
		}   // 테이블에서 이름을 찾아 데이터를 입력

		//public void Awake() { }

		public virtual void Use() { }
        public virtual void OnTriggerEnter(Collider col) { }
    }

    [System.Serializable]
    public class WeaponItem : Item {

        public WeaponItem(string _n) : base(_n) { //itemType = ItemType.TYPE_WEAPON;
		}
        public override void Use() { }
    }

    [System.Serializable]
    public class ShieldItem : Item {

        public ShieldItem(string _n) : base(_n) { //itemType = ItemType.TYPE_SHIELD; 
		}
        public override void Use() { }
    }

    [System.Serializable]
    public class ConsumeItem : Item {

        public ConsumeItem(string _n) : base(_n) { //itemType = ItemType.TYPE_CONSUME;
		}
        public override void Use() { }
    }

}
