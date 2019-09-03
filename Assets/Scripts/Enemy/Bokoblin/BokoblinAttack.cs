using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokoblinAttack : MonoBehaviour {

    public GameObject weaponHolder;
    public BoxCollider weaponCollider;
    private BokoblinState state;

    public float timeSapn = 1.0f;
    public float attackDelay = 3.0f;

    private void Awake()
    {
        state = GetComponent<BokoblinState>();
        weaponCollider = weaponHolder.GetComponentInChildren<BoxCollider>();
    }

    // attackDelay마다 공격하도록 하는 함수
    public bool AttackTimer()
    {
        if (timeSapn < attackDelay)
        {
            timeSapn += Time.deltaTime;
            return false;
        }
        else
        {
            timeSapn = attackDelay;
            return true;
        }
    }
    	
	void Update () {
        if(!state.isAttacking)
        {
            AttackTimer();
        }
    }
}
