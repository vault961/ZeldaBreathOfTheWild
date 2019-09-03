using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowMagneting : MonoBehaviour {
    public Transform playerTr;
    public Transform objectTr;
	// Use this for initialization
	void Start () {
        playerTr = GetComponentInParent<Transform>().parent;
	}
	
	// Update is called once per frame
	void Update () {
        if (objectTr == null) { return; }
        //Vector3 m = (playerTr.position + objectTr.position) * 0.5f;
        //Debug.Log(playerTr.position + " " + objectTr.position + " " + m);
        Vector3 pos = Vector3.Lerp(playerTr.position, objectTr.position, 0.7f);
        pos.x = playerTr.position.x;
        pos.z = playerTr.position.z;
        pos.y += 1f;
        transform.position = pos;

        float dis = (playerTr.position - objectTr.position).magnitude;
        if (dis > 6f) { dis = 6f; }
        Camera.main.GetComponent<CameraCollision>().maxDistance = dis;

    }
}
