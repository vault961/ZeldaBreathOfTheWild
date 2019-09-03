using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipPanel : MonoBehaviour {

    public Text nameText;
    public Text infoText;
    public Text defenText;
    public Text damegeText;
    public Text hpUpText;

    void SetNameText(string name)
    {
        nameText.text = name;
    }

    void SetInfoText(string info)
    {
        infoText.text = info;
    }

    void SetDefenText(string state)
    {
        defenText.text = state;
    }

    void SetDamegeText(string state)
    {
        damegeText.text = state.ToString();
    }

    void SetHpUpText(string state)
    {
        hpUpText.text = state.ToString();
    }

    public void SetTooltipData(ITEM.Item item)
    {
        //if(item != null)
        //{
        //    SetNameText(item.itemData.Name);
        //    SetInfoText(item.itemData.Info);
        //    SetDefenText(item.itemData.DefenseUp.ToString());
        //    SetDamegeText(item.itemData.Damage.ToString());
        //    SetHpUpText(item.itemData.HpUp.ToString());
        //}
        //else if (item == null)
        //{
        //    nameText.enabled = false;
        //}
        SetNameText(item.itemData.Name);
        SetInfoText(item.itemData.Info);
        SetDefenText(item.itemData.DefenseUp.ToString());
        SetDamegeText(item.itemData.Damage.ToString());
        SetHpUpText(item.itemData.HpUp.ToString());
    }
}
