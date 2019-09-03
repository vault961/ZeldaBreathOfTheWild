using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class FreeClimb : MonoBehaviour
    {
        public Animator anim;
        public bool isClimbing;

        public bool inPosition;
        public bool isLerping;
        float t;
        float et;
        Vector3 startPos;
        Vector3 targetPos;
        Quaternion startRot;
        Quaternion targetRot;
        public float positionOffset;
        public float offsetFromWall = 0.3f;
        public float speed_multiplier = 0.2f;
        public float climbSpeed = 3;
        public float rotateSpeed = 5;
        public float inAngleDis = 1;

        Transform helper;
        //float delta;

        public float horizontal;
        public float vertical;
        public bool isMid;
        public bool isTop = false;

        public IKSnapshot baseIKSnapshot;
        public ClimbAnimHook a_hook;

        public LayerMask layerMask;

        public OpenWorldMovement owm;
        public SlopeController sc;

        // Use this for initialization
        void Start()
        {
            owm = GetComponent<OpenWorldMovement>();
            sc = GetComponent<SlopeController>();
            Init();
        }

        public void Init() {
            helper = new GameObject().transform;
            helper.name = "Climb helper";
            anim = owm.anim;
            a_hook.Init(this, helper);
            //CheckForClimb();

            DontDestroyOnLoad(helper);
        }

        public bool CheckForClimb(int lm) {
            layerMask = lm;
            //Vector3 upv = transform.localRotation * Vector3.up * 0.5f;
            Vector3 origin = transform.position;
            origin.y += 0.82f;
            Vector3 dir = transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, 0.5f, layerMask)) {
                //Debug.Log("c");
                //Player.playerState = State.STATE_CLIMBING;
                if (GetComponent<ZeldaCombat>().hasWeapon)
                {
                    anim.SetTrigger("UnEquip");
                }
                owm.canMove = false;
                helper.transform.position = PosWithOffset(origin, hit.point);

                InitForClimb(hit);
                return true;
            }

            return false;
        }

        void InitForClimb(RaycastHit hit) {
            //anim.SetTrigger("UnEquip");
            Physics.gravity = Vector3.zero;
            isClimbing = true;
            helper.transform.rotation = Quaternion.LookRotation(-hit.normal);
            startPos = transform.position;
            targetPos = hit.point + (hit.normal * offsetFromWall);
            t = 0;
            inPosition = false;
            //anim.CrossFade("Climbing Idle", 2);
        }


        void Update()
        {
            if (Global.gameState != Global.GameState.GAME) { return; }

            if (Player.playerState != State.STATE_CLIMBING)
            {
                helper.position = transform.position + (Vector3.up * 2f);
            }
        }

        public void Tick(float delta) {
            
            if (!inPosition) {
                //Debug.Log("p");
                GetInPosition(delta);
                return;
            }

            StopClimbOnTop();
            LookForGround();
            if (!isLerping)
            {
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
                //float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);

                Vector3 h = helper.right * horizontal;
                Vector3 v = helper.up * vertical;
                Vector3 moveDir = (h + v).normalized;

                if (isMid)
                {
                    //if (moveDir == Vector3.zero)
                    //{
                    //    a_hook.CreatePosition(targetPos, Vector3.up, true);
                    //    return;
                    //}
                }
                else
                {
                    bool canMove = CanMove(moveDir);
                    if (!canMove || moveDir == Vector3.zero)
                    {
                        a_hook.CreatePosition(targetPos, Vector3.up, true);
                        return;
                    }
                }
                isMid = !isMid;

                t = 0;
                isLerping = true;
                startPos = transform.position;

                Vector3 tp = helper.position - transform.position;
                //float d = Vector3.Distance(helper.position, startPos) / 2;
                tp *= positionOffset;
                tp += transform.position;
                //tp.y += 1f; ////
                targetPos = (isMid) ? tp : helper.position;
                a_hook.CreatePosition(targetPos, moveDir, isMid);

            }
            else {
                t += delta * climbSpeed;
                if (t > 1) {
                    t = 1;
                    isLerping = false;
                }

                if (!isTop) {
                    Vector3 cp = Vector3.Lerp(startPos, targetPos, t);
                    transform.position = cp;
                    transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed);
                }

                //LookForGround();
                StopClimbOnGround();
                
            }
            
        }

        bool CanMove(Vector3 moveDir) {
            //Debug.Log(moveDir);
            Vector3 origin = transform.position;
            float dis = positionOffset;
            Vector3 dir = moveDir;
            Debug.DrawRay(origin, dir * dis, Color.red);
            RaycastHit hit;     // 가려는 방향으로 캐스트
            if (Physics.Raycast(origin, dir, out hit, dis)) {
                //return false;
            }

            origin += moveDir * dis;
            dir = helper.forward;
            float dis2 = inAngleDis;
            Debug.DrawRay(origin, dir * dis2, Color.blue);      // 벽을 향해 캐스트
            if (Physics.Raycast(origin, dir, out hit, dis, layerMask))
            {
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }
            else {
                //Debug.Log(moveDir);
                if (moveDir.y >= 0.65f)     // 위 또는 대각선 위로 향할때
                {
                    isTop = true;
                }
            }

            origin = origin + (dir * dis2);
            dir = -moveDir;
            if (Physics.Raycast(origin, dir, out hit, inAngleDis, layerMask)) {
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }

            //return false;

            origin += dir * dis2;
            dir = -Vector3.up;
            Debug.DrawRay(origin, dir, Color.green);
            if (Physics.Raycast(origin, dir, out hit, dis2, layerMask)) {
                float angle = Vector3.Angle(-helper.forward, hit.normal);
                if (angle < 40) {
                    helper.position = PosWithOffset(origin, hit.point);
                    helper.rotation = Quaternion.LookRotation(-hit.normal);
                    return true;
                }
            }

            return false;
        }

        void GetInPosition(float delta) {
            t += delta * 3;

            if (t > 1) {
                t = 1;
                inPosition = true;
                horizontal = 0;
                vertical = 0;
                // enable the ik
                a_hook.CreatePosition(targetPos, Vector3.up, false);
            }
            //Debug.Log("l5");
            //a_hook.CreatePosition(targetPos, Vector3.up, false);
            Vector3 tp = Vector3.Lerp(startPos, targetPos, t);
            transform.position = tp;
            transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed);
        }

        Vector3 PosWithOffset(Vector3 origin, Vector3 target) {
            Vector3 direction = origin - target;
            direction.Normalize();
            Vector3 offset = direction * offsetFromWall;
            //Vector3 upv = transform.localRotation * Vector3.up * 0.5f;
            return target + offset;

        }

        void LookForGround() {
            if (sc.groundAngle > sc.maxGroundAngle) { return; }
            Vector3 origin = transform.position;
            Vector3 direction = -Vector3.up;

            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, 1.0f)) {        // 일반 벽에서의 벽타기 종료
                if (vertical < 0)
                {
                    StopClimb();
                    owm.anim.SetTrigger("ClimbingEndDown");
                }
            }
            
        }

        void StopClimb() {
            vertical = 0;
            isClimbing = false;
            isLerping = false;
            owm.jumping = false;
            owm.anim.SetBool("Jump", false);
            Player.playerState = State.STATE_IDLE;
            a_hook.enabled = false;
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
        }

        void StopClimbOnGround() {
            if (layerMask != owm.layerMasks[1]) { return; }
            if (Player.playerState == State.STATE_CLIMBING) {
                if (sc.groundAngle <= sc.maxGroundAngle) {

                    owm.anim.SetBool("ClimbingEnd", true);

                    Vector3 dest = transform.position;
                    dest += transform.forward * 0.5f;
                    dest += Vector3.up * 0.4f;

                    owm.canMove = false;
                    a_hook.enabled = false;
                    StartCoroutine(OnGroundCoroutine(2.0f, dest));
                    //owm.canMove = false;
                }
            }
        }

        void StopClimbOnTop() {
            if (isTop) {
                owm.anim.SetTrigger("ClimbingTop");
                Vector3 dest = transform.position;
                dest += transform.forward * 0.5f;
                dest += Vector3.up * 0.2f;

                owm.canMove = false;
                a_hook.enabled = false;
                StartCoroutine(OnTopCoroutine(2.0f, dest));
                
            }
        }

        IEnumerator OnTopCoroutine(float duration, Vector3 dest) {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();

            Vector3 start = transform.position;
            float elapsed = 0.0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, dest, elapsed/duration);
                yield return wait;
            }

            transform.position = dest;
            StopClimb();
            owm.anim.SetTrigger("ClimbTopEnd");
            isTop = false;
            owm.canMove = true;
        }

        IEnumerator OnGroundCoroutine(float duration, Vector3 dest) {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();

            Vector3 start = transform.position;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, dest, elapsed / duration);
                yield return wait;
            }

            transform.position = dest;
            StopClimb();
            owm.canMove = true;
        }

    }

    [System.Serializable]
    public class IKSnapshot {
        public Vector3 rh, lh, lf, rf;

    }
}
