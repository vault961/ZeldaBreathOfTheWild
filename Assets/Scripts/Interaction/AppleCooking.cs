using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleCooking : MonoBehaviour {
    
    public GameObject BakedAppleFrefab;

    WaitForSeconds cookingTime = new WaitForSeconds(3.0f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fire"))
        {
            Debug.Log("불이다");
            StartCoroutine(CookingApple());
        }
    }

    IEnumerator CookingApple()
    {
        yield return cookingTime;

        Instantiate(BakedAppleFrefab, transform);
        //Destroy(gameObject, 0.0f);
    }
}
