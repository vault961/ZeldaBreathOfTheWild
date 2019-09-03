using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgent : MonoBehaviour {

    private NavMeshAgent agent;

    private readonly float traceSpeed = 4.0f;
    private readonly float accelSpeed = 10.0f;
    private readonly float agularSpeed = 360.0f;
    public float attackDistance = 3.0f;
    public float speed { get { return agent.velocity.magnitude; } }

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackDistance;
        agent.speed = traceSpeed;   
        agent.acceleration = accelSpeed;
        agent.angularSpeed = agularSpeed;
	}

    // 타겟을 넘겨받는 get/set 변수
    private Vector3 _traceTarget;
    public Vector3 traceTarget
    {
        get { return _traceTarget; }
        set
        {
            _traceTarget = value;
            TraceTarget(_traceTarget);
        }
    }

    // 넘겨받은 타겟(Vector3)을 쫓아가는 함수
    void TraceTarget(Vector3 pos)
    {
        if (agent.isPathStale) return;

        agent.destination = pos;
        agent.isStopped = false;
    }

    // 에이전트를 멈추는 함수
    public void Stop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
	
}
