using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBox : InteractComponent
{
    public GameObject itemPrefab;

    private Vector3 itemPos = new Vector3(0.0f, 1.5f, 0.0f);

    private float[] range = { 0.0f, 0.0f, 0.0f };

    private bool isOpen = false;

    // Use this for initialization
    new void Start ()
    {
        player = InventorySystem.instance.player;
        for (int i = 0; i < range.Length; ++i)
        {
            range[i] = Random.Range(1.0f, 3.0f);
        }
	}

    public override void Interact()
    {
        if (!isOpen)
        {
            itemPos = itemPos + transform.position;
            Rigidbody itemRb;
            itemPrefab = Instantiate(itemPrefab, itemPos, itemPrefab.transform.rotation);
            itemRb = itemPrefab.GetComponent<Rigidbody>();
            itemRb.AddForce(new Vector3(range[0], range[1], range[2]), ForceMode.Impulse);
            isOpen = true;
            enabled = false;
        }
    }
}
