using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 적에게 달아주는 스크립트인데스. 자신에게 달린 UI를 컨트롤할때 사용하는 데스
public class EnemyUICtrl : MonoBehaviour {

    public Canvas uiCameraCanvas;
    public GameObject uiPrefab;

    private GameObject enemyUI;
    public EnemyUI ui;

    public Vector3 uiOffset = new Vector3(0f, 2f, 0f);

    private void Awake()
    {
        uiCameraCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        enemyUI = Instantiate(uiPrefab, uiCameraCanvas.transform);
        ui = enemyUI.GetComponent<EnemyUI>();
    }

    private void OnEnable()
    {
        ui.enemyTr = transform;
        ui.offset = uiOffset;
    }

    public void DestoryUI()
    {
        Destroy(enemyUI);
    }
}
