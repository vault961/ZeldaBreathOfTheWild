using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour {

    public GameObject _topPanel;
    public GameObject _middlePanel;
    public GameObject _bottomPanel;

    public GameObject selectedItemSlot = null;

    public ToolTipPanel toolTipPanel;
    public GameObject _tooltipOBJ;
    //public ZeldaCombat zeldaCombat;
    EventSystem _eventSystem;

    public GameObject _selectPanel;

    public void SetItemToolTip(ITEM.Item item)
    {
        if(item != null)
            toolTipPanel.SetTooltipData(item);
        
    }

    private void OnEnable()
    {
        if(selectedItemSlot == null)
        {
            selectedItemSlot = EventSystem.current.firstSelectedGameObject;
            _tooltipOBJ.SetActive(false);
        }
        else
        {
            SetItemToolTip(selectedItemSlot.GetComponent<ItemSlot>()._item);
           
        }
        
    }

    public void SetSelectedItem(GameObject selected)
    {
        selectedItemSlot = selected;
        _tooltipOBJ.SetActive(true);
        
        //if (selectedItemSlot != null && _eventSystem)
        //{
        //    _selectPanel.SetActive(true);
        //}
        //else
        //    _selectPanel.SetActive(false);
    }

    private void Start()
    {
        _eventSystem = EventSystem.current;
    }
    private void Update()
    {

        //Debug.Log("현재 위치: " + _eventSystem);
    }

}
