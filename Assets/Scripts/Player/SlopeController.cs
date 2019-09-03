using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeController : MonoBehaviour {

    private Vector3 footOffset = new Vector3(0, -0.7f, 0);

    public float maxGroundAngle;
    public float groundAngle;
    RaycastHit hitInfo;
    private Vector3 lookGround;

    public LayerMask ground;

    private OpenWorldMovement openWorldMovement;
    private SA.FreeClimb freeClimb;

    // Use this for initialization
    void Start () {
        openWorldMovement = this.GetComponent<OpenWorldMovement>();

        freeClimb = GetComponent<SA.FreeClimb>();
    }
	
	// Update is called once per frame
	void Update () {
        
        CheckGround();
        CalculateGroundAngle();
        Sliding();
        //Debug.Log(openWorldMovement.canMove);
    }

    void CheckGround() {
        //Debug.DrawRay(transform.position, -Vector3.up, Color.magenta);
        if (Player.playerState == State.STATE_IDLE) {
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 2f, ground)) {
                openWorldMovement.isGrounded = true;
            }
        } else if (Player.playerState == State.STATE_CLIMBING) {
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 2f, ground))
            {
                openWorldMovement.isGrounded = true;
            }
        }

    }

    void CalculateGroundAngle() {
        //if (openWorldMovement.jumping) {
        //    groundAngle = 0.0f;
        //    return;
        //}

        groundAngle = Vector3.Angle(hitInfo.normal, Vector3.up);
        //Debug.Log(hitInfo.normal);
        //Debug.Log(groundAngle);
    }

    public void Sliding() {
        if (openWorldMovement.jumping || Player.playerState == State.STATE_CLIMBING) { return; }
        if (groundAngle >= maxGroundAngle)
        {
            if (openWorldMovement.canSlide == false) { openWorldMovement.anim.SetBool("Sliding", false); return; }

            if (openWorldMovement.speed <= openWorldMovement.allowPlayerRotation)   // 미끄러짐
            {
                Vector3 slideDir = Vector3.Reflect(hitInfo.normal, transform.up);
                transform.Translate(slideDir * Time.deltaTime * 2f, Space.World);
                //openWorldMovement.controller.Move(slideDir * Time.deltaTime * 2f);
                Vector3 rot = new Vector3(slideDir.x, 0, slideDir.z);
                transform.rotation = Quaternion.LookRotation(rot);
                openWorldMovement.anim.SetBool("Sliding", true);
            }
            else
            {          //  벽타기
                //if (freeClimb.CheckForClimb(openWorldMovement.layerMasks[1]))
                //{
                //    if (GetComponent<ZeldaCombat>().hasWeapon)
                //    {
                //        Invoke("StartClimb", 0.6f);
                //        Player.playerState = State.STATE_CLIMBING;
                //    }
                //    else
                //    {
                //        openWorldMovement.StartClimb();
                //    }
                //}
            }


        }
        else { openWorldMovement.anim.SetBool("Sliding", false); }

    }
}
