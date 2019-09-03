using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWorldMovement : MonoBehaviour {

    public float inputX;
    public float inputZ;
    public Vector3 desiredMoveDirection;
    public bool blockRotationPlayer;
    public float desiredRotationSpeed;
    public Animator anim;
    public float speed;
    public float moveSpeed;
    public float allowPlayerRotation;
    public Camera cam;
    public CharacterController controller;
    public bool isGrounded;
    private float verticalVel;
    private Vector3 moveVector;

    //private float prevInputX = 0f;
    //private float prevInputZ = 0f;
    public bool jumping = false;
    private bool running = false;
    public float stamina = 10.0f;
    private float maxStamina;
    public bool canSlide = false;
    public bool canMove = true;

    SA.FreeClimb freeClimb;
    SlopeController slopeController;

    public LayerMask[] layerMasks;

    // Use this for initialization
    void Awake()
    {
        anim = this.GetComponent<Animator>();
        cam = Camera.main;
        controller = this.GetComponent<CharacterController>();
        
        maxStamina = stamina;

        freeClimb = GetComponent<SA.FreeClimb>();
        slopeController = GetComponent<SlopeController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(canMove);
        //Gravity();
        if (Global.gameState != Global.GameState.GAME || GetComponent<ZeldaCombat>().isDead) { return; }

        switch (Player.playerState)
        {
            case State.STATE_IDLE:
                Gravity();
                if (canMove) {
                    InputMagnitude();
                    Sprint();
                    Jump();
                }
                break;
            case State.STATE_CLIMBING:
                if (canMove) {
                    freeClimb.Tick(Time.deltaTime);
                }
                break;
            case State.STATE_COMBAT:
                if (anim.GetBool("StopCombat")) {
                    Gravity();
                    if (canMove)
                    {
                        InputMagnitude();
                        Jump();
                    }
                }
                break;
            case State.STATE_TARGET:
                if (anim.GetBool("StopCombat"))
                {
                    Gravity();
                    if (canMove)
                    {
                        InputMagnitude();
                        //Jump();
                    }
                }
                break;
            case State.STATE_THROW:
                Gravity();
                if (canMove)
                {
                    InputMagnitude();
                    //Jump();
                }
                break;
            case State.STATE_MAGNET:
                Gravity();
                if (canMove)
                {
                    InputMagnitude();
                    Jump();
                }
                break;
            case State.STATE_MAGNETING:
                Gravity();
                if (canMove)
                {
                    InputMagnitude();
                }
                break;
            default:
                break;
        }

    }

    void PlayerMoveAndRotation()    // 캐릭터 이동과 회전
    {
        if (Player.playerState == State.STATE_TARGET) { PlayerMoveOnTarget(); return; }
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * inputZ + right * inputX;
        desiredMoveDirection = desiredMoveDirection.normalized;

        if (!blockRotationPlayer) {     // 막혀있지 않다면 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
            controller.Move(transform.forward * Time.deltaTime * speed * moveSpeed * 2f);
        }
        else {
            //transform.Translate(transform.forward * Time.deltaTime * speed * moveSpeed, Space.World);

            controller.Move(desiredMoveDirection * Time.deltaTime * speed * moveSpeed * 2f * 0.3f);
        }
    }

    void PlayerMoveOnTarget() {
        Vector3 targetDir = GetComponent<ZeldaCombat>().target.transform.position - transform.position;
        targetDir.y = 0;
        //Debug.Log(targetDir);
        transform.rotation = Quaternion.LookRotation(targetDir.normalized);

        var forward = transform.forward;
        var right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * inputZ + right * inputX;
        
        //transform.Translate(transform.forward * Time.deltaTime * speed * moveSpeed, Space.World);
        controller.Move(desiredMoveDirection * Time.deltaTime * speed * moveSpeed * 1.5f);
    }

    void InputMagnitude()   // 입력
    {
        // 축 입력
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        anim.SetFloat("InputZ", inputZ, 0.0f, Time.deltaTime * 2f);
        anim.SetFloat("InputX", inputX, 0.0f, Time.deltaTime * 2f);

        speed = new Vector2(inputX, inputZ).magnitude;

        if (speed > allowPlayerRotation) {          // 이동할 때
            anim.SetBool("Move", true);
            anim.SetFloat("InputMagnitude", speed, 0.0f, Time.deltaTime);
            blockRotationPlayer = (Player.playerState == State.STATE_THROW || Player.playerState == State.STATE_MAGNETING) ? true : false;
            PlayerMoveAndRotation();
            canSlide = false;
            anim.SetBool("Combat", false);   // 움직이면 전투모션 해제
        }
        else if (speed <= allowPlayerRotation) {    // 멈출 때
            anim.SetFloat("InputMagnitude", speed, 0.0f, Time.deltaTime);
            anim.SetBool("Move", false);
            canSlide = true;
        }

    }

    void Jump() {
        // 땅에 있을 때.
        //isGrounded = CheckGround();
        //Gravity();
        // 점프
        if (Input.GetButtonDown("XButton"))
        {
            
            if (inputZ > 0 && Player.playerState != State.STATE_CLIMBING)
            {
                if (freeClimb.CheckForClimb(layerMasks[0]))
                {
                    //Debug.Log("c");
                    //StartClimb();
                    if (GetComponent<ZeldaCombat>().hasWeapon)
                    {
                        Invoke("StartClimb", 0.6f);
                        Player.playerState = State.STATE_CLIMBING;
                    }
                    else {
                        StartClimb();
                    }
                }
                else if (slopeController.groundAngle >= slopeController.maxGroundAngle) {
                    if (freeClimb.CheckForClimb(layerMasks[1]))
                    {
                        if (GetComponent<ZeldaCombat>().hasWeapon)
                        {
                            Invoke("StartClimb", 0.6f);
                            Player.playerState = State.STATE_CLIMBING;
                        }
                        else
                        {
                            StartClimb();
                        }
                    }
                }
            }
            if (jumping == false && Player.playerState != State.STATE_CLIMBING)
            {
                //Debug.Log("j");
                anim.SetBool("Jump", true);
                jumping = true;
                verticalVel = 0.17f;
                moveVector = new Vector3(0, verticalVel, 0);
                controller.Move(moveVector);
            }
        }

        //transform.Translate(moveVector);
    }

    void Sprint() {     // 달리기 함수

        if (Input.GetButtonDown("BButton") && stamina > 0) {
            if (!running) {
                running = true;
                moveSpeed *= 2f;
                anim.SetTrigger("Sprint");
            }
        }
        if (Input.GetButtonUp("BButton") || stamina <= 0) {
            if (running)
            {
                running = false;
                moveSpeed *= 0.5f;
                anim.SetTrigger("Sprint");
            }
        }
        if (running) { stamina -= Time.deltaTime * 3f; }
        else if(stamina < maxStamina){ stamina += Time.deltaTime * 2f; }
        if (stamina > maxStamina) { stamina = maxStamina; }
    }

    public void StartClimb() {     // 벽타기 함수
        Player.playerState = State.STATE_CLIMBING;
        canMove = true;
        anim.SetBool("ClimbingEnd", false);
        freeClimb.a_hook.enabled = true;
        inputX = 0.0f;
        inputZ = 0.0f;
        speed = 0.0f;
        verticalVel = 0f;
        desiredMoveDirection = Vector3.zero;
    }

    bool CheckGround() {
        RaycastHit hit;
        //Debug.DrawRay(transform.position, (-transform.up + -transform.forward) * 0.3f, Color.red);
        //Debug.DrawRay(transform.position, (-transform.up + transform.forward) * 0.3f, Color.red);
        //Debug.DrawRay(transform.position, -transform.up * 0.3f, Color.magenta);
        if (Physics.Raycast(transform.position, -transform.up, out hit, 0.3f)) {
            return true;
        }
        if (Physics.Raycast(transform.position, (-transform.up + -transform.forward), out hit, 0.3f)) {
            return true;
        }
        if (Physics.Raycast(transform.position, (-transform.up + transform.forward), out hit, 0.3f)) {
            return true;
        }
        if (Physics.Raycast(transform.position, (-transform.up + transform.right), out hit, 0.3f)) {
            return true;
        }
        if (Physics.Raycast(transform.position, (-transform.up + -transform.right), out hit, 0.3f)) {
            return true;
        }
        return false;
    }

    void Gravity() {
        isGrounded = controller.isGrounded;
        //isGrounded = CheckGround();
        if (!isGrounded) { isGrounded = CheckGround(); }
        //Debug.Log(isGrounded + "   " + verticalVel);
        if (isGrounded)
        {
            if (verticalVel < -0.6f) {      // 높은 곳에서 떨어졌을 때
                anim.SetBool("HardLanding", true);
                GetComponent<Player>().hardLandingFX.GetComponent<ParticleSystem>().Play();
                canMove = false;
                //if (verticalVel < -1.0f) { GetComponent<ZeldaCombat>().GetDamage(Mathf.Abs(verticalVel * 50.0f)); }
            }
            jumping = false;
            verticalVel = 0;
            anim.SetBool("Jump", false);
            anim.SetBool("Air", false);
        }
        else
        {
            if (verticalVel < -0.15f) { anim.SetBool("Air", true); }
            verticalVel -= 0.015f;
        }
        moveVector = new Vector3(0, verticalVel, 0);
        controller.Move(moveVector);
    }

    void EndHardLanding() { 
        canMove = true;
        anim.SetBool("HardLanding", false);
        
    }
}
