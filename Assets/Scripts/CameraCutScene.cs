using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCutScene : MonoBehaviour {
    public GameObject player;
    public Animator anim;

    float dt = 0.0f;

    // Use this for initialization
    void Start () {
        if (Global.gameState != Global.GameState.CUT_SCENE) { return; }
        player = InventorySystem.instance.player;
        anim = player.GetComponent<Animator>();

        GetComponent<OpenWorldCamera>().enabled = false;
        transform.position = player.transform.position + player.transform.up * -0.3f + player.transform.forward * 0.3f;
        transform.rotation = Quaternion.Euler(90.0f, 0, 0);

        anim.CrossFade("StandingUp", 0.0f);
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Global.gameState != Global.GameState.CUT_SCENE) { return; }
        dt += Time.deltaTime;
        //Debug.Log(dt);
        if (dt < 5.0f)
        {
            transform.Translate(-transform.up * 0.01f);
        }
        else {
            SetCameraPos();
        }

        if (dt > 9.0f) {
            Global.gameState = Global.GameState.GAME;
            anim.CrossFade("Idle 1", 0.0f);
            GetComponent<OpenWorldCamera>().enabled = true;
            this.enabled = false;
        }
	}

    void SetCameraPos() {
        transform.position = GetComponent<OpenWorldCamera>().CameraFollowObj[0].transform.position;
        transform.rotation = GetComponent<OpenWorldCamera>().CameraFollowObj[0].transform.rotation;
    }
}
