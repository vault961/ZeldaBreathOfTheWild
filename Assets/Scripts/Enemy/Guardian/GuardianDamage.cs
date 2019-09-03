using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianDamage : MonoBehaviour {

    private GuardianAnimationCtrl ani;
    private GuardianState state;
    private EnemyUICtrl uiCtrl;
    public ParticleSystem explosion;
    public ParticleSystem explosionBig;

    private float DownTime = 3.5f;
    WaitForSeconds wsDeadTimer;

    private void Awake()
    {
        ani = GetComponent<GuardianAnimationCtrl>();
        state = GetComponent<GuardianState>();
        uiCtrl = GetComponent<EnemyUICtrl>();
    }

    // 가디언 처맞는 함수
    void GetDamage(float damage)   
    {
        if(!state.isDead)
        {
            state.currentHP -= damage;
            uiCtrl.ui.OnHP(state.currentHP / state.maxHP);

            if (state.currentHP <= 0f)
            {
                StartCoroutine(GetDead());
            }
            else if (!state.isAttacking)
            {
                ani.Damage();
            }
        }
    }
    
    // 가디언 뒤지는 함수
    private float explosionCount = 7.0f;
    private float explosionTerm = 0.3f;
    IEnumerator GetDead()       // 사망 코루틴
    {
        state.isDead = true;
        uiCtrl.DestoryUI();
        ani.Down();

        for(int i=0; i<explosionCount; i++)
        {
            // 랜덤한 방향으로 폭발 이펙트 생성
            Instantiate(explosion,
             transform.position + new Vector3(Random.Range(-0.5f, 0.5f),
                                              1.0f,         
                                              Random.Range(-0.5f, 0.5f)),
             transform.rotation
            );
            yield return new WaitForSeconds(0.3f);
        }

        DownTime -= explosionCount * explosionTerm;
        wsDeadTimer = new WaitForSeconds(DownTime);
        yield return wsDeadTimer;

        Instantiate(explosionBig, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon") && other.GetComponent<WeaponComponent>().isUsing)
        {
            GetDamage(other.GetComponent<WeaponComponent>().weaponItem.itemData.Damage);
        }
    }
}
