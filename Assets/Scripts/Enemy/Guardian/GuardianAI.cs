using UnityEngine;

public class GuardianAI : MonoBehaviour {

    private Transform playerTr;
    private GuardianState state;
    private GuardianAnimationCtrl ani;
    private GuardianHeadCtrl headCtrl;
    private GuardianAttack attack;

    private Selector root;

    private int playerLayer;
    private int enemyLayer;
    private int layerMask;

    private float viewRange = 10.0f;     // 가디언 시야 범위
    private float turningSpeed = 3.0f;   // 회전 속도
    private float minDistance = 3.0f;    // 최소 거리
    private float maxDistance = 10.0f;   // 최대 거리

	void Awake () {
        state = GetComponent<GuardianState>();
        ani = GetComponent<GuardianAnimationCtrl>();
        headCtrl = GetComponentInChildren<GuardianHeadCtrl>();
        attack = GetComponent<GuardianAttack>();

        playerLayer = LayerMask.NameToLayer("Player");
        layerMask = 1 << playerLayer;

        (root = Selector.Make()).
            AddChild(ActionNode.Make(CheckState)).
            AddChild(Sequence.Make().
                    AddChild(ActionNode.Make(SenseFoe)).
                    AddChild(ActionNode.Make(ChecktDistFoe)).
                    AddChild(ActionNode.Make(AttackFoe)).
                    AddChild(ActionNode.Make(CheckFoeDead))
                    );
	}

    bool CheckState() // 가디언의 현재 상태 체크
    {
        if (state.isAttacking || state.isDamaging)
        {
            return true;
        }
        return false;
    }

    bool SenseFoe() // 범위 안에 적이 있는지 감지
    {
        if (playerTr != null)   // 플레이어를 감지했을 경우 바로 다음 노드로 넘어감
            return true;

        Collider[] colls =
            Physics.OverlapSphere(
                transform.position,
                viewRange,
                1 << playerLayer
                );

        if(colls.Length >= 1)   // 플레이어를 발견한 경우
        {
            playerTr = colls[0].GetComponent<Transform>(); // 플레이어 Transform을 넣어줌
            headCtrl.playerTr = playerTr;   // headCtrl에 플레이어 Transform 전달
            return true;
        }
        else
        {
            return false;
        }
    }

    bool ChecktDistFoe()    // 적과의 거리 체크
    {
        float dist = (playerTr.position - transform.position).sqrMagnitude;
        if(dist <= minDistance * minDistance)
        {
            state.isMoving = true;
        }
        else if (dist >= maxDistance * maxDistance)
        {
            state.isMoving = false;
        }
        MoveBack();
        RotateToTarget();

        return true;
    }

    bool AttackFoe()        // 공격 함수
    {
        if(!state.isCharging)
        {
            if(attack.AttackTimer()) // 쿨타임이 돌았는지 확인
            {
                attack.StartCoroutine(attack.Attack()); // 미사일 발사!
                return true;
            }
        }
        return false;
    }

    bool CheckFoeDead()     // 적이 죽었는지 체크하는 함수
    {
        // 링크가 죽었는지 판단할수 있는 문구를 적어주세요
        return false;
    }

    #region 뒤로가기(MoveBack()), 회전하기(RotateToFoe()) 함수
    private float backRayDir = 2.5f;
    private float rayDist = 3f;
    private float walkSpeed = 5f;
    void MoveBack()   // 뒤에 바닥이 있는지 체크
    {
        Vector3 dir = (transform.position - playerTr.position).normalized;
        dir.y = 0;
        
        // 바닥 체크하는 레이
        Ray ray1 = new Ray(
            transform.position + Vector3.up,
            (-transform.forward * backRayDir) + (-transform.up)
            );
        RaycastHit hit1;
        Physics.Raycast(ray1, out hit1, rayDist);

        // 뒤 체크하는 레이
        Ray ray2 = new Ray(transform.position + Vector3.up, dir);
        RaycastHit hit2;
        Physics.Raycast(ray2, out hit2, rayDist);

        if(state.isMoving)
        {
            if (hit1.collider != null && hit2.collider == null)   // 뒤에 바닥이 있는 경우 (계속 움직임)
            {
                transform.position += dir * Time.deltaTime * walkSpeed;
                ani.OnWalkB();
            }
            else                        // 뒤에 바닥이 없는 경우 (멈춤)
            {
                state.isMoving = false;
            }
        }
        else
        {
            ani.OffWalkB();
        }

    }

    void RotateToTarget()   // 타겟으로 회전하도록 하는 함수
    {
        Vector3 dir = (playerTr.position - transform.position); // 플레이어를 향한 방향
        Vector3 cross = Vector3.Cross(transform.forward, dir);  // 타겟과 가디언과의 방향을 외적한다
        dir.y = 0;

        if (cross.y <= -1f)     // 타겟이 우측 방향에 있다면 우측으로 회전
        {
            if(!state.isMoving)
            {
                ani.OffTurnL();
                ani.OnTurnR();
            }
            Rotate(dir);
        }
        else if (cross.y >= 1f) // 타겟이 좌측 방향에 있다면 좌측으로 회전
        {
            if(!state.isMoving)
            {
                ani.OffTurnR();
                ani.OnTurnL();
            }
            Rotate(dir);
        }
        else                    // -1 <= 외적값 <= 1 인 경우 회전하지 않는다
        {
            ani.OffTurnL();
            ani.OffTurnR();
        }
    }

    void Rotate(Vector3 dir) // 회전하는 함수
    {
        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                turningSpeed * Time.deltaTime
                );
    }
    #endregion

    void Update () {
        if(!state.isDead)
        {
            root.Run();
        }
    }
}
