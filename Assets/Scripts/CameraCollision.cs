using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour {

    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;
    public float smooth = 10.0f;
    Vector3 dollyDir;
    public Vector3 dollyDirAdjusted;
    public float distance;

    public OpenWorldCamera cameraBase;

	// Use this for initialization
	void Awake () {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
	}
	
    void Start()
    {
        cameraBase = GetComponentInParent<OpenWorldCamera>();
    }

	// Update is called once per frame
	void Update () {
        if (Global.gameState != Global.GameState.GAME) { return; }
        if (GetComponent<CameraZoom>().isZoomed) { return; }
        if (Player.playerState == State.STATE_THROW) { maxDistance = 2f; }
        else if (Player.playerState == State.STATE_MAGNETING) {  }
        else { maxDistance = (cameraBase.rotX - cameraBase.clampMinAngle) / 25.0f + 1; }

        switch (Player.playerState)
        {
            case State.STATE_TARGET:
                maxDistance = cameraBase.targetDis;
                break;
            case State.STATE_THROW:
                maxDistance = 2f;
                break;
            case State.STATE_MAGNETING:

                break;
            default:
                maxDistance = (cameraBase.rotX - cameraBase.clampMinAngle) / 25.0f + 1;
                break;
        }

        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        //Debug.DrawRay(transform.parent.position, desiredCameraPos, Color.magenta);

        //Vector3 dir = (transform.parent.position - desiredCameraPos);
        //float dis = dir.magnitude;
        //Debug.DrawRay(desiredCameraPos, dir.normalized * dis, Color.magenta);
        
        //if (Physics.Raycast(desiredCameraPos, dir, out hit, 3f))
        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit, LayerMask.NameToLayer("IgnoreCamCollision")))
        {
            if (hit.transform.gameObject.tag != "Player")
            distance = Mathf.Clamp(hit.distance, minDistance, maxDistance);

        }
        else {
            distance = maxDistance;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
	}
}
