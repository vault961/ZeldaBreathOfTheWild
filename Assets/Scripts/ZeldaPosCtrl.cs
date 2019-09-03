using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZeldaPosCtrl : MonoBehaviour {
    GameObject player;
    GameObject[] magnetCam;
    Vector3 magnetCamPos = new Vector3(0.34f, 1.73f, 0.0f);

	// Use this for initialization
	void Start () {
        player = InventorySystem.instance.player;
        magnetCam = GameObject.FindGameObjectsWithTag("MagnetCAM");

        if (SceneManager.GetActiveScene().name == "Zelda_Play" &&
            player.GetComponent<Player>().currentPos != Vector3.zero)
        {
            transform.position = player.GetComponent<Player>().currentPos;
        }
        player.transform.position = transform.position;
        foreach (var cam in magnetCam)
        {
            cam.transform.position = transform.position + magnetCamPos;
        }

        player.GetComponent<Player>().Reset();
	}

}
