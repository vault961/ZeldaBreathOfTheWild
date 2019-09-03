using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour {

    private Transform pt;

    private void Start()
    {
        pt = InventorySystem.instance.player.GetComponent<Transform>();
    }
    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, -pt.eulerAngles.y);
    }
}
