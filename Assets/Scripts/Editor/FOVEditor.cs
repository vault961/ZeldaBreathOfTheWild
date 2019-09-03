using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BokoblinSense))]
public class FOVEditor : Editor {

    private void OnSceneGUI()
    {
        // BokoblinSense 클래스 참조
        BokoblinSense sense = (BokoblinSense)target;

        // 원주 위의 시작점의 좌표를 계산
        Vector3 fromAnglePos = sense.CirclePoint(-sense.viewAngle * 0.5f);

        // 원의 색상을 흰색으로 지정
        Handles.color = Color.white;

        // 외곽선만 표현하는 원반 그림
        Handles.DrawWireDisc(
            sense.transform.position,
            Vector3.up,
            sense.viewRange);

        // 부채꼴 색상
        Handles.color = new Color(1, 1, 1, 0.2f);

        // 채워진 부채꼴 그림
        Handles.DrawSolidArc(
            sense.transform.position,
            Vector3.up,
            fromAnglePos,
            sense.viewAngle,
            sense.viewRange
            );

        // 시야각에 텍스트 표시
        Handles.Label(
            sense.transform.position + (sense.transform.forward * 2.0f),
            sense.viewAngle.ToString()
            );

    }
}
