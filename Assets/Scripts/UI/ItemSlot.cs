using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour {

    public Image itemIcon;
    public int _index;
    public ITEM.Item _item = null;

    private Sprite sprite;
    EventSystem _eventSystem;
    
    private ZeldaCombat _zeldaCombat;

    private void OnEnable()
    {
        _eventSystem = EventSystem.current;
    }
    

    public void SetItemData(ITEM.Item item = null)
    {
        _item = item;
        SpriteAtlas atlas = UIManager.instance.GetItemIconAtlas();
        if (item != null)
        {
            sprite = atlas.GetSprite(item.itemData.Name);
            item.count++;
        }
        else
        {
            sprite = atlas.GetSprite("Transparency");
        }
        itemIcon.overrideSprite = sprite;

    }

    public void selectWItem()
    {
        _zeldaCombat = InventorySystem.instance.player.GetComponent<ZeldaCombat>();
        _zeldaCombat.CreateWeapon(_index);
    }

    public void selectSItem()
    {
        _zeldaCombat = InventorySystem.instance.player.GetComponent<ZeldaCombat>();
        _zeldaCombat.CreateShield(_index);
    }

}