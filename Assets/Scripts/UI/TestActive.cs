using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TestActive : MonoBehaviour {

    //public GameObject _gameObject;
    //bool active;
    EventSystem _eventSystem;

    private void OnEnable()
    {
        _eventSystem = EventSystem.current;
    }
    
    // Update is called once per frame
    void Update ()
    {
        //if(Input.GetMouseButtonDown(1))
        //{
        //    RaycastHit hit;
        //
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //
        //    if(Physics.Raycast(ray, out hit))
        //    {
        //        if(hit.collider != null)
        //        {
        //            GameObject _gameObject = hit.collider.gameObject;
        //            _eventSystem.SetSelectedGameObject(_gameObject);
        //
        //        }
        //    }
        //}
    }

  
}
