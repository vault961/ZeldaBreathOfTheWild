using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinmapPos : MonoBehaviour {

    Vector3 Pos = new Vector3(0.0f, 20.0f, 0.0f);
	// Update is called once per frame
	void Update () {
        transform.position = InventorySystem.instance.player.transform.position + Pos;
	}
}
