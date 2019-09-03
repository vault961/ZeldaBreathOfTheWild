using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetController : MonoBehaviour {

    public bool getMagnet;
    public bool isUsing = false;
    private GameObject player;

    public GameObject Crosshair;
    public GameObject magnet;
    public GameObject Line_R;
    public GameObject Line_L;

    public List<GameObject> IronObject = new List<GameObject>();
    public List<GameObject> IronDoor = new List<GameObject>();

    public ParticleSystem sparkParticle;

    // IronObject 이동 Property
    Rigidbody hitRigidbody;
    public GameObject magnetPivot;
    public float moveUpSpeed;
    public float pushbackSpeed;
    public float aroundSpeed;
    public float moveDoorSpeed;

    public Collider hitObject = null;

    public CameraFollowMagneting cameraFollowMagneting;

    private void Start()
    {
        player = InventorySystem.instance.player;

        // Magnet 정지
        magnet.SetActive(false);

        Crosshair.SetActive(false);

        sparkParticle.Play(false);

        //magnet.GetComponentInChildren<LineRenderer>().enabled = false;
        Line_R.GetComponent<LineRenderer>().enabled = false;
        Line_L.GetComponent<LineRenderer>().enabled = false;

        cameraFollowMagneting = GetComponent<Player>().cameraFollowMagneting.GetComponent<CameraFollowMagneting>();

        moveUpSpeed = 100.0f;
        aroundSpeed = 150.0f;
        pushbackSpeed = 130.0f;
        moveDoorSpeed = 20.0f;
    }

    // Update is called once per frame
    void Update () {

        if (getMagnet)
        {
            ChangeIronState();

            if (Player.playerState == State.STATE_MAGNET)
            {
                OnMagnet();
            }
            else if (Player.playerState == State.STATE_MAGNETING)
            {
                HitObjectCtrl();
            }

            if (Input.GetButtonDown("LButton"))
            {
                isUsing = true;

                Player.playerState = State.STATE_MAGNET;
                // Crosshair active
                Crosshair.SetActive(true);
                // magnet active
                magnet.SetActive(true);
            }

            if (Input.GetButtonDown("BButton"))  // 아이템 버튼 비활성화
            {
                Player.playerState = State.STATE_IDLE;
                isUsing = false;
                GetComponent<Animator>().SetBool("Magneting", isUsing);

                // Crosshair inactive
                Crosshair.SetActive(false);

                // magnet inactive
                magnet.SetActive(false);

                // Line Renderer
                Line_R.GetComponent<LineRenderer>().enabled = false;
                Line_R.GetComponent<magneticTrail>().dest = null;

                Line_L.GetComponent<LineRenderer>().enabled = false;
                Line_L.GetComponent<magneticTrail>().dest = null;

                //Joint
                //magnet.GetComponent<HingeJoint>().connectedBody = null;

                // particle 재생
                if (hitObject != null)
                {
                    sparkParticle.transform.position = hitObject.transform.position;
                    sparkParticle.Play();
                }

                hitObject = null;
                cameraFollowMagneting.objectTr = null;
            }
        }
    }

    void ChangeIronState()
    {
        if(isUsing)
        {
            // IronObject
            foreach (var IronObj in IronObject)
            {
                if (hitObject == null)
                    IronObj.GetComponentInChildren<IronController>().CurState = IronState.Inactive;
            }
            // IronDoor
            foreach (var IronObj in IronDoor)
            {
                if (hitObject == null)
                    IronObj.GetComponentInChildren<IronController>().CurState = IronState.Inactive;
            }
        }
        else // 전체 비활성화
        {
            // IronObject
            foreach (var IronObj in IronObject)
            {
                IronObj.GetComponentInChildren<IronController>().CurState = IronState.None;
            }
            // IronDoor
            foreach (var IronObj in IronDoor)
            {
                IronObj.GetComponentInChildren<IronController>().CurState = IronState.None;
            }
        }
    }

    bool CheckRaycast()
    {
        bool isHit = false;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f))
        {
            if (hit.collider.CompareTag("IRON") || hit.collider.CompareTag("IRONDOOR"))
            {
                hit.collider.GetComponent<IronController>().CurState = IronState.Active;

                // IronObject를 선택
                if (Input.GetButtonDown("AButton"))
                {
                    // hitObject에 연결
                    hitObject = hit.collider;
                    isHit = true;
                    cameraFollowMagneting.objectTr = hit.transform;
                }
                else
                    hitObject = null;
            }
        }
        return isHit;
    }

    void OnMagnet()
    {
        if (CheckRaycast())
        {
            isUsing = true;
            Player.playerState = State.STATE_MAGNETING;
            GetComponent<Animator>().SetBool("Magneting", isUsing);

            hitObject.GetComponent<IronController>().CurState = IronState.Active;

            //LineRenderer
            Line_R.GetComponent<magneticTrail>().dest = hitObject.transform;
            Line_R.GetComponent<LineRenderer>().enabled = true;

            Line_L.GetComponent<magneticTrail>().dest = hitObject.transform;
            Line_L.GetComponent<LineRenderer>().enabled = true;

            //Joint
            //magnet.GetComponent<HingeJoint>().connectedBody = hitObject.GetComponent<Rigidbody>();
        }
    }

    void HitObjectCtrl()
    {
        // Hit된 Object의 Transfom을 받아온다.
        hitRigidbody = hitObject.GetComponentInChildren<Rigidbody>();

        //float dist;
        //Vector3 fromMgt;    // 좌우 이동시, hit Object를 뷰포트의 가운데로 끌어오기 위한 vector3 

        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");
        float inputZ = Input.GetAxis("MagnetZAxis");
        float inputVertical = Input.GetAxis("Vertical");

        if (hitRigidbody.CompareTag("IRON"))
        {
            // 상하 이동
            hitRigidbody.AddForce(0, -inputY * moveUpSpeed, 0);

            // 좌우 이동       
            hitRigidbody.transform.RotateAround(magnetPivot.transform.position,
                                                    hitRigidbody.transform.up, inputX * aroundSpeed * Time.deltaTime);
            // 안팎 이동
            hitRigidbody.AddForce( player.transform.forward.z * inputZ * pushbackSpeed, 0, 0);

        }
        else if (hitRigidbody.CompareTag("IRONDOOR"))
        {
            IronDoor[0].GetComponent<Transform>().parent.Rotate(new Vector3(0, -inputVertical * Time.deltaTime * moveDoorSpeed, 0f), Space.World);
            IronDoor[1].GetComponent<Transform>().parent.Rotate(new Vector3(0, inputVertical * Time.deltaTime * moveDoorSpeed, 0f), Space.World);
        }
    }

}
