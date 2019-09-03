using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magneticTrail : MonoBehaviour {

    private LineRenderer lineRenderer;
    private float counter;
    private float dist;
    public int offset;

    private Transform origin;
    public Transform dest;

	// Use this for initialization
	void Start () {

        origin = this.transform;
        lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.positionCount = offset + 1;
	}
	
	// Update is called once per frame
	void Update () {

        if (lineRenderer.enabled == true)
        {
            lineRenderer.SetPosition(0, origin.position);
            
            //dist = (origin.position - dest.position).sqrMagnitude;
            //
            //for(int i=0; i < offset; i++)
            //{
            //    lineRenderer.SetPosition(i, new Vector3(origin.position.x, origin.position.y * Mathf.Sin(Time.deltaTime), origin.position.z));
            //}

            // 마지막 도착지점
            lineRenderer.SetPosition(1, dest.position);

        }

	}
}
