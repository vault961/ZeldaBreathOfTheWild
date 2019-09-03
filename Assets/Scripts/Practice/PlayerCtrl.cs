using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 링크를 움직이려고 만든 테스트용 스크립트인것
public class PlayerCtrl : MonoBehaviour {

    private float h = 0.0f;
    private float v = 0.0f;
    public float moveSpeed = 8.0f;
    Vector3 movement = Vector3.zero;

    float rotationX = 0.0f;

    void Update () {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        movement = (Vector3.forward * v) + (Vector3.right * h);
        movement = movement.normalized * moveSpeed * Time.deltaTime;
        //transform.position += movement;
        transform.Translate(movement, Space.Self);

        rotationX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, rotationX * Time.deltaTime * 100f);
    }
}
