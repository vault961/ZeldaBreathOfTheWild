using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.ScrollSnaps;


public class ButtonMoveEvent : Button {
    
    DirectionalScrollSnap scrollSnap;       //스크롤링 함수를 쓰기위해 연결

    //public ZeldaCombat _zeldaCombat;

    protected override void Start()
    {
        scrollSnap = GetComponentInParent<DirectionalScrollSnap>();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        ItemSlot slot = GetComponent<ItemSlot>();

        int index;
        scrollSnap.GetSnapIndexOfChild(transform.parent.GetComponent<RectTransform>(), out index);  //현재 내가 속해있는 그룹의 부모 RectTransform 싹 돌고 나서 현재 위치의 인덱스를 반환
        //Debug.Log("SlotGroup Index Before: " + index);


        UIManager.instance.inventoryUI.SetItemToolTip(slot._item);
        UIManager.instance.inventoryUI.SetSelectedItem(gameObject);
        

        //_zeldaCombat.CreateShield(index);

        if (index == scrollSnap.closestSnapPositionIndex)
            return;

        //현재 위치해 있는 내 위치가 이동할려는 인덱스보다 작으면
        if (index > scrollSnap.closestSnapPositionIndex)
        {
            scrollSnap.OnForward(); //앞으로 가라
        }
        else
        {
            scrollSnap.OnBack();    //뒤로 가라(내가 위치한 인덱스가 이동할려는 인덱스보다 크면)
        }
        //Debug.Log("SlotGroup Index After: " + index);
    }
}
