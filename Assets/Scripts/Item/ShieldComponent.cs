using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldComponent : InteractComponent{

    public ITEM.Item shieldItem;
    private bool isTrigger = false;

    private void Awake()
    {
        shieldItem = new ITEM.Item(itemName);
    }

    public override void Interact()
    {
        InventorySystem.instance.AddItem(shieldItem);
        player.GetComponent<ZeldaCombat>().PickItem((ItemType)shieldItem.itemData.Type);

        if (player.GetComponent<ZeldaCombat>().hasWeapon == false) {
            player.GetComponent<ZeldaCombat>().CreateWeapon(InventorySystem.instance.itemLists[(int)ItemType.TYPE_WEAPON].Count);
        }
        Destroy(this.gameObject, 0.0f);
   
    }
}
