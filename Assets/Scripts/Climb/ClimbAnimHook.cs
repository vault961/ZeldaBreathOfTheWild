using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class ClimbAnimHook : MonoBehaviour
    {
        Animator anim;
        IKSnapshot ikBase;
        IKSnapshot current = new IKSnapshot();
        IKSnapshot next = new IKSnapshot();
        IKGoals goals = new IKGoals();

        public float w_rh;
        public float w_lh;
        public float w_rf;
        public float w_lf;

        Vector3 rh, lh, rf, lf;
        Transform h;

        bool isMirror;
        bool isLeft;
        Vector3 prevMoveDir;

        float delta;
        public float lerpSpeed = 2.0f;

        // Use this for initialization
        void Start()
        {

        }

        public void Init(FreeClimb c, Transform helper) {
            anim = c.anim;
            ikBase = c.baseIKSnapshot;
            h = helper;
        }

        public void CreatePosition(Vector3 origin, Vector3 moveDir, bool isMid) {
            delta = Time.deltaTime;
            HandleAnim(moveDir, isMid);

            //if (!isMid)
            //{
            //    UpdateGoals(moveDir);
            //    prevMoveDir = moveDir;
            //}
            //else {
            //    UpdateGoals(prevMoveDir);
            //}
            UpdateGoals(moveDir);
            prevMoveDir = moveDir;


            IKSnapshot ik = CreateSnapShot(origin);
            CopySnapshot(ref current, ik);

            SetIKPosition(isMid, goals.lf, current.lf, AvatarIKGoal.LeftFoot);
            SetIKPosition(isMid, goals.lh, current.lh, AvatarIKGoal.LeftHand);
            SetIKPosition(isMid, goals.rf, current.rf, AvatarIKGoal.RightFoot);
            SetIKPosition(isMid, goals.rh, current.rh, AvatarIKGoal.RightHand);

            UpdateIKWeight(AvatarIKGoal.LeftFoot, 1);
            UpdateIKWeight(AvatarIKGoal.LeftHand, 1);
            UpdateIKWeight(AvatarIKGoal.RightFoot, 1);
            UpdateIKWeight(AvatarIKGoal.RightHand, 1);
        }

        void UpdateGoals(Vector3 moveDir)
        {
            isLeft = (moveDir.x <= 0);

            if (moveDir.x != 0)
            {
                goals.lf = isLeft;
                goals.lh = isLeft;
                goals.rf = !isLeft;
                goals.rh = !isLeft;
            }
            else {
                bool isEnabled = isMirror;
                if (moveDir.y < 0) {
                    isEnabled = !isEnabled;
                }

                goals.lf = isEnabled;
                goals.lh = isEnabled;
                goals.rf = !isEnabled;
                goals.rh = !isEnabled;
            }
        }

        void HandleAnim(Vector3 moveDir, bool isMid) {
            if (isMid)
            {
                if (moveDir.y != 0)     // 위아래 움직일때
                {
                    if (moveDir.x == 0)
                    {
                        isMirror = !isMirror;
                        anim.SetBool("mirror", isMirror);
                    }
                    else
                    {
                        if (moveDir.y < 0)
                        {
                            isMirror = (moveDir.x > 0);
                            anim.SetBool("mirror", isMirror);
                        }
                        else
                        {
                            isMirror = (moveDir.x < 0);
                            anim.SetBool("mirror", isMirror);
                        }
                    }

                    anim.CrossFade("Climbing Up Wall", 0.2f);
                }
            }
            else {
                anim.CrossFade("Climbing Idle", 0.2f);
            }
        }

        public IKSnapshot CreateSnapShot(Vector3 o) {
            IKSnapshot r = new IKSnapshot();

            Vector3 _lh = localToWorld(ikBase.lh);
            r.lh = GetPosActual(_lh, AvatarIKGoal.LeftHand);

            Vector3 _rh = localToWorld(ikBase.rh);
            r.rh = GetPosActual(_rh, AvatarIKGoal.RightHand);
            //r.rh = localToWorld(ikBase.rh);

            Vector3 _lf = localToWorld(ikBase.lf);
            r.lf = GetPosActual(_lf, AvatarIKGoal.LeftFoot);
            //r.lf = localToWorld(ikBase.lf);

            Vector3 _rf = localToWorld(ikBase.rf);
            r.rf = GetPosActual(_rf, AvatarIKGoal.RightFoot);
            return r;
        }

        public float wallOffset = 0.1f;

        Vector3 GetPosActual(Vector3 o, AvatarIKGoal goal) {
            Vector3 r = o;
            Vector3 origin = o;
            Vector3 dir = h.forward;
            origin += -(dir * 0.2f);
            RaycastHit hit;
            bool isHit = false;
            if (Physics.Raycast(origin, dir, out hit, 1.5f)) {
                Vector3 _r = hit.point + (hit.normal * wallOffset);
                r = _r;
                isHit = true;

                if (goal == AvatarIKGoal.RightFoot)
                {
                    if (hit.point.y > transform.position.y - 0.1f){
                        isHit = false;
                    }

                }
            }

            if (!isHit) {
                switch (goal)   
                {
                    case AvatarIKGoal.LeftFoot:
                        r = localToWorld(ikBase.lf);
                        break;
                    case AvatarIKGoal.RightFoot:
                        r = localToWorld(ikBase.rf);
                        break;
                    case AvatarIKGoal.LeftHand:
                        r = localToWorld(ikBase.lh);
                        break;
                    case AvatarIKGoal.RightHand:
                        r = localToWorld(ikBase.rh);
                        break;
                    default:
                        break;
                }
            }

            return r;
        }

        Vector3 localToWorld(Vector3 p) {
            Vector3 r = h.position;
            r += h.right * p.x;
            r += h.forward * p.z;
            r += h.up * p.y;
            return r;
        }

        public void CopySnapshot(ref IKSnapshot to, IKSnapshot from) {
            to.rh = from.rh;
            to.lh = from.lh;
            to.rf = from.rf;
            to.lf = from.lf;
        }

        void SetIKPosition(bool isMid, bool isTrue, Vector3 pos, AvatarIKGoal goal) {
            if (isMid)
            {
                Vector3 p = GetPosActual(pos, goal);
                if (isTrue)
                {
                    UpdateIKPosition(goal, p);
                }
                else {
                    if (goal == AvatarIKGoal.LeftFoot) {
                        if (p.y > transform.position.y - 0.25f) {
                            UpdateIKPosition(goal, p);
                        }
                    }
                }
            }
            else {
                if (!isTrue) {
                    Vector3 p = GetPosActual(pos, goal);
                    UpdateIKPosition(goal, p);
                }
            }
        }

        public void UpdateIKPosition(AvatarIKGoal goal, Vector3 pos) {
            switch (goal) {
                case AvatarIKGoal.LeftFoot:
                    lf = pos;
                    break;
                case AvatarIKGoal.RightFoot:
                    rf = pos;
                    break;
                case AvatarIKGoal.LeftHand:
                    lh = pos;
                    break;
                case AvatarIKGoal.RightHand:
                    rh = pos;
                    break;
            }
        }

        public void UpdateIKWeight(AvatarIKGoal goal, float w)
        {
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    w_lf = w;
                    break;
                case AvatarIKGoal.RightFoot:
                    w_rf = w;
                    break;
                case AvatarIKGoal.LeftHand:
                    w_lh = w;
                    break;
                case AvatarIKGoal.RightHand:
                    w_rh = w;
                    break;
            }
        }

        public void OnAnimatorIK() {
            delta = Time.deltaTime;

            SetIKPos(AvatarIKGoal.LeftFoot, lf, w_lf);
            SetIKPos(AvatarIKGoal.LeftHand, lh, w_lh);
            SetIKPos(AvatarIKGoal.RightFoot, rf, w_rf);
            SetIKPos(AvatarIKGoal.RightHand, rh, w_rh);
        }

        void SetIKPos(AvatarIKGoal goal, Vector3 tp, float w) {
            IKStates iKState = GetIKState(goal);
            if (iKState == null) {
                iKState = new IKStates();
                iKState.goal = goal;
                ikStates.Add(iKState);
            }

            if (w == 0) {
                iKState.isSet = false;
            }

            if (!iKState.isSet) {
                iKState.position = GoalToBodyBones(goal).position;
                iKState.isSet = true;
            }

            iKState.positionWeight = w;
            iKState.position = Vector3.Lerp(iKState.position, tp, delta * lerpSpeed);


            anim.SetIKPositionWeight(goal, iKState.positionWeight);
            anim.SetIKPosition(goal, iKState.position);
        }

        Transform GoalToBodyBones(AvatarIKGoal goal) {
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    return anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                case AvatarIKGoal.RightFoot:
                    return anim.GetBoneTransform(HumanBodyBones.RightFoot);
                case AvatarIKGoal.LeftHand:
                    return anim.GetBoneTransform(HumanBodyBones.LeftHand);
                default:
                case AvatarIKGoal.RightHand:
                    return anim.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        IKStates GetIKState(AvatarIKGoal goal) {
            IKStates r = null;
            foreach (IKStates i in ikStates)
            {
                if (i.goal == goal) {
                    r = i;
                    break;
                }
            }

            return r;
        }

        List<IKStates> ikStates = new List<IKStates>();

        class IKStates {
            public AvatarIKGoal goal;
            public Vector3 position;
            public float positionWeight;
            public bool isSet = false;
        }
    }


    public class IKGoals {
        public bool rh;
        public bool lh;
        public bool rf;
        public bool lf;
    }
}
