using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvenPlayerCamera : MonoBehaviour {

    private Transform _transform;
    Vector3 _cameraPos = new Vector3(0.0f, 0.9f, 5.0f);

	// Use this for initialization
	void Start ()
    {
        _transform = InventorySystem.instance.player.GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = InventorySystem.instance.player.transform.position + _cameraPos;
	}
}
