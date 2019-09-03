using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokoblinDamage : MonoBehaviour {

    private BokoblinAnimationCtrl ani;
    private BokoblinState state;
    private EnemyUICtrl uiCtrl;
    private Rigidbody[] ragdolls;
    private BokoblinAI ai;
    public ParticleSystem explosion;

    // 콤보 어택을 맞을때 사용하는 변수들
    public int hitCount = 0;
    public int maxCount = 3;
    public float comboTimer = 4.0f;
    public float originTime = 4.0f;

    private void Awake()
    {
        ani = GetComponent<BokoblinAnimationCtrl>();
        state = GetComponent<BokoblinState>();
        uiCtrl = GetComponent<EnemyUICtrl>();
        ragdolls = GetComponentsInChildren<Rigidbody>();
        ai = GetComponent<BokoblinAI>();
    }

    // 처 맞는 함수 입니다
    public void GetDamage(float damage)
    {
        if(state.isDead)
        {
            return;
        }

        if(!state.isAlert)
        {
            ai.StartCoroutine(ai.Alert());  // 데미지를 받을시 바로 얼럿상태로 전환
        }

        state.currentHP -= damage;     // 요거시는 나중에 무기 데미지에 따라 다르게 바꿔야 하구연
        uiCtrl.ui.OnHP( state.currentHP/state.maxHP );

        if(state.currentHP <= 0f)   // 체력이 0이 되면 사망
        {
            StopCoroutine(KnockDown());
            StartCoroutine(GetDead());
        }
        else if(!state.isKnockDown) // 기절 중이 아니라면 히트 카운트를 센다
        {
            hitCount++;
            comboTimer = originTime;

            if (hitCount == maxCount) // 히트카운트가 맥스카운트와 같을 시 기절 코루틴
            {
                StartCoroutine(KnockDown());
            }
            else if(!state.isAttacking) // 공격중이 아니라면 처 맞는 애니메이션
            {
                ani.Damage();
            }
        }
    }

    // 기절 코루틴~~
    WaitForSeconds knockdownTime = new WaitForSeconds(3.0f);
    IEnumerator KnockDown()
    {
        ani.ani.enabled = false;
        SetRagdoll(false);
        state.isKnockDown = true;
        yield return knockdownTime; // 이 시간 만큼 기절했다가 일어나버리기

        SetRagdoll(true);
        ani.ani.enabled = true;
        state.isKnockDown = false;
        ani.GetUp();
        hitCount = 0;
    }

    // 보코블린 사망 함수
    IEnumerator GetDead()
    {
        state.isDead = true;
        ani.ani.enabled = false;
        SetRagdoll(false);
        yield return knockdownTime;

        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // 레그돌 꺼주고 켜주기 (_kinematic 매개변수로 판단)
    void SetRagdoll(bool _kinematic)
    {
        foreach(Rigidbody bone in ragdolls)
        {
            if (!bone.CompareTag("Weapon"))
            {
                bone.isKinematic = _kinematic;
                bone.GetComponent<Collider>().enabled = !_kinematic;
            }
        }
    }

    // 콤보 시간 카운트. 콤보타이머가 도는 동안에 히트카운트를 저장한다. 
    // 타이머가 끝나면 히트카운트는 0이 된다
    void HitCounter()
    {
        if (hitCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f) 
            {
                hitCount = 0;
                comboTimer = originTime;
            }
        }
    }

    private void Update()
    {
        HitCounter();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Weapon") && other.GetComponent<WeaponComponent>().isUsing)
        {
            GetDamage(other.GetComponent<WeaponComponent>().weaponItem.itemData.Damage);
        }
    }
}
