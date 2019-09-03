using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokoblinSense : MonoBehaviour {

    private BokoblinState state;
    private BokoblinAnimationCtrl ani;

    // 보코블린 시야각
    [Range(0, 360)]
    public float viewAngle = 90.0f;

    // 보코블린 시야거리
    public float viewRange = 15.0f;

    // 보코블린 의심도
    public float suspicion = 0.0f;
    public bool isSense = false;

    private Transform bokoTr;
    private Transform playerTr;
    private int playerLayer;
    private int obstacleLayer;
    private int layerMask;

    private float offset = 1.5f;

    private void Awake()
    {
        state = GetComponent<BokoblinState>();
        ani = GetComponent<BokoblinAnimationCtrl>();

        bokoTr = GetComponent<Transform>();
        playerTr = GameObject.FindGameObjectWithTag("Player").transform;

        playerLayer = LayerMask.NameToLayer("Player");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
        layerMask = 1 << playerLayer | 1 << obstacleLayer;
    }
    
	void Update () {
        if(!state.isDead)
        {
            SensePlayer();
            SetSuspicion();
        }
    }

    //FOV GUI에 사용되는 녀석
    public Vector3 CirclePoint(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3(
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0,
            Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    // 의심도
    public float GetSuspicion() { return suspicion; }
    private void SetSuspicion()
    {        
        if (!state.isAlert) // 얼럿 상태인 경우 의심도 변동 없음(계속 1.0으로 유지)
        {
            float dist = Vector3.Distance(playerTr.position, bokoTr.position);

            // 플레이어를 감지한 경우 (isSense가 true) 의심도가 올라감
            if (isSense)
            {
                if (suspicion >= 1.0f)
                {
                    suspicion = 1.0f;
                }
                else
                    // 의심도는 거리에 반비례해서 올라감 (가까울수록 빨리 올라감)
                    suspicion += Time.deltaTime * (5.0f / dist);
            }
            // 플레이어가 보이지 않으면 의심도는 하락함
            else
            {
                if (suspicion <= 0.0f)
                {
                    suspicion = 0.0f;
                }
                else
                    suspicion -= Time.deltaTime;
            }
        }
    }

    // 플레이어 감지 함수
    public bool GetSense() { return isSense; }
    private bool SensePlayer()
    {
        if (bokoTr == null)
            return isSense;

        Vector3 startPos = bokoTr.position;

        //Debug.DrawRay(startPos, playerTr.position);

        // 보코블린의 범위 안에 플레이어 캐릭터를 찾아냄
        Collider[] colls =
            Physics.OverlapSphere(
                startPos,           // 보코블린 위치
                viewRange,          // 보코블린 시야 거리
                1 << playerLayer    // 플레이어 레이어 마스크
                );
        //Debug.Log(colls.Length);
        // 범위 안에 플레이어가 들어와있는지 판단
        if (colls.Length >= 1)
        {
            RaycastHit hit;
            Vector3 dir = (playerTr.position - bokoTr.position);

            // 1. 플레이어가 시야각 안에 들어와있고
            // 2. 플레이어에게 레이케스트가 맞았을때(사이에 장애물이 없을때) 플레이어를 감지함
            if (Vector3.Angle(bokoTr.forward, dir.normalized) < viewAngle * 0.5f
                && Physics.Raycast(startPos, dir, out hit, viewRange, layerMask))
            {
                isSense = hit.collider.CompareTag("Player");
            }
            else
                isSense = false;
        }
        else
        {
            isSense = false;
        }

        return isSense;
    }
}
