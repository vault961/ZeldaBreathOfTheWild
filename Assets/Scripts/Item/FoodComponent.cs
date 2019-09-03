using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodComponent : InteractComponent {

    public ITEM.Item consumeItem;

    private void Awake()
    {
        consumeItem = new ITEM.Item(itemName);
    }

    public override void Interact()
    {
        InventorySystem.instance.AddItem(consumeItem);
        player.GetComponent<ZeldaCombat>().PickItem((ItemType)consumeItem.itemData.Type);

        Destroy(this.gameObject, 0.0f);
    }
}
