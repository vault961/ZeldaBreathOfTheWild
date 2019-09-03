using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianAnimationCtrl : MonoBehaviour {

    private Animator ani;

    private readonly int hashWait = Animator.StringToHash("Wait");
    private readonly int hashTurnL = Animator.StringToHash("Turn L");
    private readonly int hashTurnR = Animator.StringToHash("Turn R");
    private readonly int hashWalkB = Animator.StringToHash("Walk B");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashDamage = Animator.StringToHash("Damage");
    private readonly int hashDown = Animator.StringToHash("Down");

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>();
    }

    public void OnWait() { ani.SetBool(hashWait, true); }
    public void OffWait() { ani.SetBool(hashWait, false); }

    public void OnTurnL() { ani.SetBool(hashTurnL, true); }
    public void OffTurnL() { ani.SetBool(hashTurnL, false); }

    public void OnTurnR() { ani.SetBool(hashTurnR, true); }
    public void OffTurnR() { ani.SetBool(hashTurnR, false); }

    public void OnWalkB() { ani.SetBool(hashWalkB, true); }
    public void OffWalkB() { ani.SetBool(hashWalkB, false); }

    public void Attack() { ani.SetTrigger(hashAttack); }

    public void Damage() { ani.SetTrigger(hashDamage); }

    public void Down() { ani.SetTrigger(hashDown); }
}
