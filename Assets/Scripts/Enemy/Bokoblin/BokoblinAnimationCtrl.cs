using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokoblinAnimationCtrl : MonoBehaviour {

    public Animator ani;

    private readonly int hashFind = Animator.StringToHash("Find");
    private readonly int hashWalk = Animator.StringToHash("Walk");
    private readonly int hashDoubt = Animator.StringToHash("Doubt");
    private readonly int hashWait = Animator.StringToHash("Wait");
    private readonly int hashTurn = Animator.StringToHash("Turn");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashAttackIdx = Animator.StringToHash("AttackIdx");
    private readonly int hashWaitB = Animator.StringToHash("WaitB");
    private readonly int hashDamage = Animator.StringToHash("Damage");
    private readonly int hashGetUp = Animator.StringToHash("GetUp");

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>();
    }

    public void OnFind() { ani.SetTrigger(hashFind); }

    public void OnWalk() { ani.SetBool(hashWalk, true); }
    public void OffWalk() { ani.SetBool(hashWalk, false); }

    public void OnDoubt() { ani.SetBool(hashDoubt, true); }
    public void OffDoubt() { ani.SetBool(hashDoubt, false); }

    public void OnWait() { ani.SetBool(hashWait, true); }
    public void OffWait() { ani.SetBool(hashWait, false); }

    public void OnTurn(float dot) { ani.SetFloat(hashTurn, dot); }
    public void OffTurn() { ani.SetFloat(hashTurn, 0.0f); }

    public void OnAttack() { ani.SetTrigger(hashAttack); }
    public void AttackIdx(int num) { ani.SetInteger(hashAttackIdx, num); }

    public void OnWaitB() { ani.SetBool(hashWaitB, true); }
    public void OffWaitB() { ani.SetBool(hashWaitB, false); }

    public void Damage() { ani.SetTrigger(hashDamage); }
    public void GetUp() { ani.SetTrigger(hashGetUp); }

}
