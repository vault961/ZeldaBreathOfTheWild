using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleDropComponent : MonoBehaviour {

    public List<GameObject> apples = new List<GameObject>();
    private int shakingCount = 0;
    private List<int> maxCounts = new List<int>();
    private void Awake()
    {
        for (int i = 0; i < apples.Count; ++i)
        {
            maxCounts.Add(i + 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            shakingCount++;

            for (int i = 0; i < apples.Count; i++)
            {
                if (shakingCount >= maxCounts[i])
                {
                    Rigidbody appleRb = apples[i].GetComponent<Rigidbody>();
                    appleRb.isKinematic = false;
                    apples.Remove(apples[i]);
                }
            }
        }
    }

    public void Update()
    {
        if (apples.Count == 0)
            this.enabled = false;
    }
}
