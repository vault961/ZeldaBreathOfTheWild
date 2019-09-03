using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianHeadCtrl : MonoBehaviour {

    public Transform playerTr;
    public float headSpinSpeed = 15.0f;
    
	void Update () {
        if(playerTr != null)
        {
            HeadSpin();
        }
	}

    public void HeadSpin()
    {
        Vector3 dir = (playerTr.position - transform.position);

        //transform.LookAt(new Vector3(dir.x, transform.position.y, dir.z));

        transform.forward = dir;
        transform.Rotate(Vector3.forward, -90f);

        //transform.rotation =
        // Quaternion.Lerp(
        //transform.rotation,
        //Quaternion.LookRotation(dir),
        //headSpinSpeed * Time.deltaTime
        //);
        //transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, 0);

        Debug.DrawRay(transform.position, dir, Color.red);
        Debug.DrawRay(transform.position, transform.forward, Color.blue);   
    }
}
