using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 애니메이션 이벤트에 삽입될 함수를 가지고 있는 클래스입니다
public class BokoblinEventCtrl : MonoBehaviour {
    private BokoblinState state;
    private BokoblinAttack attack;
    private Transform bokoTr;

    private float movingSpeed = 0.0f;
    private bool isMove = false;

    private void Awake()
    {
        state = GetComponentInParent<BokoblinState>();
        attack = GetComponentInParent<BokoblinAttack>();
        bokoTr = GetComponentInParent<Transform>().parent;
    }

    // 어택을 하고난 뒤 어택 타이머를 초기화 시켜줘요
    void ResetAttack()
    {
        if(state.isAttacking)
        {
            state.isAttacking = false;
            attack.timeSapn = 0.0f;
        }
    }

    // 공격하는 동안 앞으로 전진하는 함수여요
    void StartMoving(float speed) { movingSpeed = speed; isMove = true; }
    void StopMoving() { isMove = false; }

    // 무기를 휘두르는 동안에만 웨폰 콜라이더를 켜주는 함수입니다
    void OnWeaponCollider() { attack.weaponCollider.enabled = true; }
    void OffWepaonCollider() { attack.weaponCollider.enabled = false; }

    // 처맞는 동안에 isDamaging이라는 변수를 true 바꿔줍니다
    void OnDamage() { state.isDamaging = true; }
    void OffDamage() { state.isDamaging = false; }

    private void Update()
    {
        if(isMove)
        {
            bokoTr.position += bokoTr.forward * Time.deltaTime * movingSpeed;
        }
    }
}
