using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using UnityEngine.EventSystems;
using UnityEngine;

public class MagnetCamera : MonoBehaviour {

    public PostProcessingProfile MgtProfile;
    // 기존 Profile 저장
    PostProcessingProfile DgnProfile;

    //public GameObject Magnet;
    public GameObject player;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        DgnProfile = GetComponent<PostProcessingBehaviour>().profile;
	}
	
	// Update is called once per frame
	void Update () {
        // 자석이 사용 중일 떄
        if (player.GetComponent<MagnetController>().isUsing == true)
        {
            Debug.Log("제발좀");
            GetComponent<PostProcessingBehaviour>().profile = MgtProfile;
            MgtProfile.bloom.enabled = true;
            MgtProfile.colorGrading.enabled = true;
        }
        else
        {
            Debug.Log("ㅁㅁ");
            GetComponent<PostProcessingBehaviour>().profile = DgnProfile;
            MgtProfile.bloom.enabled = false;
            MgtProfile.colorGrading.enabled = false;

        }
    }
}
