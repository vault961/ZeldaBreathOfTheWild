using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianEventCtrl : MonoBehaviour {
    private GuardianState state;
    private GuardianAttack attack;

	// Use this for initialization
	void Start () {
        state = GetComponentInParent<GuardianState>();
        attack = GetComponentInParent<GuardianAttack>();
	}
    
    void Missile() { attack.LaunchMissile(); }
    void OffAttackState() { state.isAttacking = false; }
}
