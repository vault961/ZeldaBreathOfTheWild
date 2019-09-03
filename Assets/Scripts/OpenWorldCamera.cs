using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using UnityEngine;

public class OpenWorldCamera : MonoBehaviour {

    public float CameraMoveSpeed = 120.0f;
    public GameObject[] CameraFollowObj;
    Vector3 followPos;
    public float clampMinAngle = -40.0f;
    public float clampMaxAngle = 89.0f;
    public float inputSensitivity = 150.0f;
    //public GameObject CameraObj;
    //public GameObject PlayerObj;
    public float camDistanceXToPlayer;
    public float camDistanceYToPlayer;
    public float camDistanceZToPlayer;

    public float finalInputX;
    public float finalInputZ;
    public float smoothX;
    public float smoothY;
    private float rotY;
    public float rotX;

    public GameObject player;
    public float targetDis;
    //public Vector3 offsetPos;
    public Transform MagnetTr;

    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Use this for initialization
    void Start () {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        //offsetPos = (Vector3.forward * 1f) + (Vector3.up * 0.5f);

        MagnetTr = CameraFollowObj[2].transform;

        DontDestroyOnLoad(CameraFollowObj[2]);
        DontDestroyOnLoad(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
        if (Global.gameState != Global.GameState.GAME) { return; }

        float inputX = Input.GetAxis("Mouse X");
        float inputZ = Input.GetAxis("Mouse Y");
        
        finalInputX = inputX;
        finalInputZ = inputZ;

        // 자석 사용중일 때에는 카메라 상하회전이 안됨
        if (Player.playerState != State.STATE_MAGNETING) {
            rotX += finalInputZ * inputSensitivity * Time.deltaTime;
        }
        rotY += finalInputX * inputSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, clampMinAngle, clampMaxAngle);


        //Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0f);
        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0f);
        transform.rotation = localRotation;

        // 던지기, 망원경, 자석 사용중일 때에는 카메라 좌우회전이 캐릭터에 적용
        if (Player.playerState == State.STATE_THROW || GetComponentInChildren<CameraZoom>().isZoomed || Player.playerState == State.STATE_MAGNETING)
        {
            localRotation = Quaternion.Euler(0, rotY, 0f);
            player.transform.rotation = localRotation;
        }
        
        MagnetTr.transform.RotateAround(player.transform.position, Vector3.up, finalInputX * inputSensitivity * Time.deltaTime);
    }

    void LateUpdate() {
        
        CameraUpdater();
    }

    void CameraUpdater() {
        //Transform target = (Player.playerState == State.STATE_THROW) ? CameraFollowObj[1].transform : CameraFollowObj[0].transform;
        Vector3 target = CameraFollowObj[0].transform.position;

        switch (Player.playerState)
        {
            case State.STATE_THROW:
                target = CameraFollowObj[1].transform.position;
                break;
            case State.STATE_MAGNET:
                //CameraMoveSpeed = 5.0f;
                //target = CameraFollowObj[0].transform.position + offsetPos;
                target = CameraFollowObj[2].transform.position;
                break;
            case State.STATE_MAGNETING:
                target = CameraFollowObj[3].transform.position;
                break;
            case State.STATE_TARGET:
                Vector3 playerPos = player.transform.position;
                Vector3 targetPos = player.GetComponent<ZeldaCombat>().target.transform.position;
                target = Vector3.Lerp(playerPos, targetPos, 0.5f);
                target.y += 1.5f;
                targetDis = Vector3.Distance(playerPos, targetPos) + 3.0f;
                break;
            default:
                //target = CameraFollowObj[0].transform;
                break;
        }

        float step = CameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

    }

    
}
