using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MovingImage : MonoBehaviour {
	public GameObject Dest;
    public EventSystem eSystem;

	public float speed;

	private void Start()
	{
		speed = 10;
	}

	public void Update()
	{
        if (transform.position.x < Dest.transform.position.x)
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
            eSystem.GetComponent<StandaloneInputModule>().enabled = false;
        }
        else
        {
            eSystem.GetComponent<StandaloneInputModule>().enabled = true;
        }
	}
}
