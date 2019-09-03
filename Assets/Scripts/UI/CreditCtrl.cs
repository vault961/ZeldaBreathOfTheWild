using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditCtrl : MonoBehaviour {

    public GameObject targetPoint;
    private float speed = 10.0f;
    private bool isStop = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Space))
        {
            speed = 50.0f;
        }
        else
        {
            speed = 10.0f;
        }
        if (!isStop)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            if (transform.position.y >= targetPoint.transform.position.y)
            {
                StopCredit();
            }
        }
    }

    void StopCredit()
    {
        isStop = true;
        StartCoroutine(Credit());
    }

    IEnumerator Credit()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Zelda_Start");
    }
}
