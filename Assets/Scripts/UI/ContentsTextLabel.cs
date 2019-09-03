using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContentsTextLabel : MonoBehaviour {

    Text _ContentsText;
    public List<GameObject> _itemSlot;

    // Use this for initialization
    void Awake ()
    {
        _ContentsText = GetComponent<Text>();
        //_itemSlot = getc<ItemSlotGroup>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        //if(_eventSystem == null)
        //{
        //    _ContentsText.text = " ";
        //}
        //else
        //    
        //itemContentsText();
        //Debug.Log(_eventSystem);
    }

    public void itemContentsText()
    {

    }
}
