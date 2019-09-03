using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokoblinAI : MonoBehaviour {

    private Transform playerTr;
    private BokoblinSense sense;
    private BokoblinState state;
    private BokoblinAttack attack;
    private MoveAgent agent;
    private BokoblinAnimationCtrl ani;
    private EnemyUICtrl uiCtrl;
    private Rigidbody[] ragdolls;

    private Selector root;

    public float turningSpeed = 3.0f;

    void Awake () {
        playerTr = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        sense = GetComponent<BokoblinSense>();
        state = GetComponent<BokoblinState>();
        attack = GetComponent<BokoblinAttack>();
        agent = GetComponent<MoveAgent>();
        ani = GetComponent<BokoblinAnimationCtrl>();
        uiCtrl = GetComponent<EnemyUICtrl>();
        ragdolls = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody bone in ragdolls) // 시작시 래그돌과 콜라이더를 꺼준다
        {
            if(bone.GetComponent<Rigidbody>() != GetComponent<Rigidbody>())
            {
                if (!bone.CompareTag("Weapon"))
                {
                    bone.isKinematic = true;
                    bone.GetComponent<Collider>().enabled = false;
                }
            }
        }
        
        (root = Selector.Make()).
            AddChild(ActionNode.Make(CheckState)).
            // 전투
            AddChild(Sequence.Make().
                    AddChild(ActionNode.Make(IsAlert)).
                    AddChild(ActionNode.Make(CheckMoveToFoe)).
                    AddChild(ActionNode.Make(AttackFoe)).
                    AddChild(ActionNode.Make(CheckFoeDead))
                    ).
            // 의심
            AddChild(Selector.Make().
                    AddChild(ActionNode.Make(IsAlert)).
                    AddChild(ActionNode.Make(SenseFoe))
                    ).
            // 유휴
            AddChild(Selector.Make().
                    AddChild(ActionNode.Make(IsAlert)).
                    AddChild(ActionNode.Make(Idle))
                    );
    }

    private void OnEnable()
    {
        StartCoroutine(BokbolinAI());
        if(uiCtrl.ui != null)
        {
            uiCtrl.ui.enabled = true;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (uiCtrl.ui != null)
        {
            uiCtrl.ui.enabled = false;
        }
    }

    private void OnDestroy()
    {
        uiCtrl.DestoryUI();
    }

    IEnumerator BokbolinAI()
    {
        while (!state.isDead)
        {
            yield return new WaitForSeconds(0.03f);
            root.Run();
        }
    }

    #region 공통
    bool CheckState() // 상태 체크
    {
        if (state.isDead || state.isAttacking || state.isKnockDown)
            return true;
        else
            return false;
    }
    bool IsAlert() // 얼럿 상태인지 체크
    {
        return state.isAlert;
    }
    #endregion

    #region Combat(전투)상태 트리
    bool CheckMoveToFoe()  // 적과의 거리 체크
    {
        if (state.isDamaging || state.isKnockDown) // 공격 받으면 잠시 멈칫한다
        {
            agent.Stop();
            return false;
        }
        else
        { 
            agent.traceTarget = playerTr.position;  // 플레이어를 쫓아간다
            Turn();

            // 일정거리 이상 다가가면 멈춘다
            float dist = (playerTr.position - transform.position).sqrMagnitude;
            if (dist <= agent.attackDistance * agent.attackDistance)
            {
                ani.OffWalk();
                ani.OnWaitB();
                return true;
            }
            // 아니라면 계속 쫓아감
            else
            {
                ani.OffWaitB();
                ani.OnWalk();
                return false;
            }
        }
    }

    bool AttackFoe()  // 적 공격
    {
        if(attack.AttackTimer()) // 일정 주기마다 공격 (AttackTimer() 사용)
        {
            agent.Stop();
            state.isAttacking = true;

            int ran = Random.Range(1, 6); // 1 ~ 5 까지의 공격 모션을 사용
            ani.OnAttack();
            ani.AttackIdx(ran);
        }
        return true;
    }

    bool CheckFoeDead() // 적이 죽었는지 확인
    {
        return false;
    }
    #endregion

    #region Suspicion(의심)상태 트리
    bool SenseFoe() // 적 감지
    {
        if(sense.GetSuspicion() > 0.0f) // 의심도가 0 이상일시 
        {
            uiCtrl.ui.OnSuspicion(sense.GetSuspicion());

        }

        if (sense.GetSense()) // 감지 했는지 판단
        {
            state.currentState = BokoblinState.State.SUSPICION;
            ani.OnDoubt();
            Turn();

            if (sense.GetSuspicion() >= 1.0f) // 의심도가 꽉 차면 얼럿상태로 전환
            {
                StartCoroutine(Alert()); 
            }

            return true;
        }

        else if(sense.GetSuspicion() <= 0.0f) // 의심도가 0이 되면 ui 꺼줌
        {
            uiCtrl.ui.OffSuspicion();
            ani.OffDoubt();
            state.currentState = BokoblinState.State.IDLE;
        }
        return false;

    }

    public IEnumerator Alert() // 얼럿 코루틴
    {
        uiCtrl.ui.OffSuspicion();   // 의심도(? 물음표) UI를 꺼준다
        uiCtrl.ui.OnAlert();        // 얼럿(! 느낌표) UI 켜준다
        ani.OnFind();               // 발견 애니메이션 실행

        state.isAlert = true;       // 얼럿 켜줌
        state.currentState = BokoblinState.State.COMBAT;    // 전투 상태로 스테이트 변경
        yield return new WaitForSeconds(0.3f);              // 0.3초뒤 주위 보코블린의 얼럿도 키워줌

        // viewRange 범위 안에 있는 보코블린들에게 얼럿 전달
        Collider[] colls =
            Physics.OverlapSphere(
                transform.position,
                sense.viewRange
                );

        foreach(Collider coll in colls)
        {
            if(coll.CompareTag("Bokoblin")  // 만약 범위안에 보코블린이 있고 얼럿 상태가 아니라면
                && !coll.GetComponent<BokoblinState>().isAlert)
            {
                coll.GetComponent<BokoblinAI>().StartCoroutine(
                    coll.GetComponent<BokoblinAI>().Alert());       // 얼럿 코루틴을 실행
            }
        }

        yield return new WaitForSeconds(1.0f); // 0.1로 뒤 얼럿 UI를 꺼줌

        uiCtrl.ui.OffAlert();   // 얼럿 UI 꺼줌
        yield break;
    }

    public void Turn() // 적을 감지하면 돌기 
    {
        Vector3 dir = (playerTr.position - transform.position);

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                turningSpeed * Time.deltaTime
                );
    }
    #endregion

    #region Idle(유휴)상태 트리
    bool Idle()
    {
        if (state.currentState == BokoblinState.State.IDLE)
        {
            ani.OnWait();
        }
        return false;
    }
    #endregion
}