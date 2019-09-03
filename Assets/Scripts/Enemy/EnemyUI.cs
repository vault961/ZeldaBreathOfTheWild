using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 이녀석은 UI에 달아주는 스크립트 입니다. 적에 달아주지 마세요!!
public class EnemyUI : MonoBehaviour {

    private Camera uiCamera;
    private Canvas canvas;
    private RectTransform rectParent;
    private RectTransform rectTransform;

    [HideInInspector] public Vector3 offset = Vector3.zero;
    [HideInInspector] public Transform enemyTr;

    [HideInInspector] public Image HP;
    [HideInInspector] public Image currentHP;
    [HideInInspector] public Image suspicion;
    [HideInInspector] public Image currentSuspicion;
    [HideInInspector] public Image alert;
    
	void Awake () {
        canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas.worldCamera;
        rectParent = canvas.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();

        HP = GetComponentsInChildren<Image>()[0];
        currentHP = GetComponentsInChildren<Image>()[1];
        suspicion = GetComponentsInChildren<Image>()[2];
        currentSuspicion = GetComponentsInChildren<Image>()[3];
        alert = GetComponentsInChildren<Image>()[4];
    }

    // HP바 끄고 켜주기
    public void OffHP() { HP.enabled = currentHP.enabled = false; }
    public void OnHP(float leftHP)
    {
        HP.enabled = currentHP.enabled = true;
        currentHP.fillAmount = leftHP;
    }

    // 의심도 끄고 켜주기
    public void OffSuspicion() { suspicion.enabled = currentSuspicion.enabled = false; }
    public void OnSuspicion(float suspicionGauge)
    {
        suspicion.enabled = currentSuspicion.enabled = true;
        currentSuspicion.fillAmount = suspicionGauge;
    }
    
    // 느낌표 끄고 켜주기
    public void OnAlert() { alert.enabled = true; }
    public void OffAlert() { alert.enabled = false; }

    private void LateUpdate()
    {
        var screenPos = Camera.main.WorldToScreenPoint(enemyTr.position + offset);

        if(screenPos.z < 0.0f)
        {
            screenPos *= -1.0f;
        }

        var localPos = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectParent,
            screenPos,
            uiCamera,
            out localPos
            );

        rectTransform.localPosition = localPos;
    }
}
