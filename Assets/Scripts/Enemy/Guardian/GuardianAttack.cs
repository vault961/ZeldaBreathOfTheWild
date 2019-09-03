using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianAttack : MonoBehaviour {

    private GuardianState state;
    private GuardianAnimationCtrl ani;
    private Transform eyeTr;

    public ParticleSystem chargeEffect;
    public ParticleSystem chargeSphere;
    public GameObject missile;

    private float timeSpan = 0f;
    private float coolTime = 4.0f;
    private float missileSpeed = 3000f;

    private float chargeTime;
    private WaitForSeconds wsCharge;

    private void Awake()
    {
        state = GetComponent<GuardianState>();
        ani = GetComponent<GuardianAnimationCtrl>();
        eyeTr = GetComponentInChildren<MuzzleComponents>().GetComponent<Transform>();

        chargeTime = 3.0f;
        wsCharge = new WaitForSeconds(chargeTime);
    }

    public bool AttackTimer()
    {
        if (timeSpan < coolTime)
        {
            timeSpan += Time.deltaTime;
            return false;
        }
        else
        {
            timeSpan = coolTime;
            return true;
        }
    }

    public IEnumerator Attack()
    {
        timeSpan = 0f;

        // 기모으는 중
        state.isCharging = true;
        chargeEffect.Play();
        chargeSphere.Play();
        yield return wsCharge;

        // 투사체 발사
        state.isAttacking = true;
        state.isCharging = false;
        ani.Attack();
        yield break;
    }

    public void LaunchMissile() // 미사일 발사 함수
    {
        chargeEffect.Stop();
        chargeSphere.Stop();
        // 미사일 발사
        GameObject _missile = Instantiate(
            missile,
            eyeTr.position,
            Quaternion.identity
            );
        _missile.transform.rotation = eyeTr.rotation;
        _missile.GetComponent<Rigidbody>().AddForce(_missile.transform.forward * missileSpeed);
    }

    private void Update()
    {
        // 충전중 or 공격중이 아니라면 
        if (!state.isCharging && !state.isAttacking)
        {
            AttackTimer();
        }
    }
}
