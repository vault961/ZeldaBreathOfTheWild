using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IronState
{
    None,
    Active,
    Inactive
}

public class IronController : MonoBehaviour {

    private GameObject player;

    public IronState CurState = IronState.None;
    public float BurnColor = 4.0f;
    public float Emissive = 0.5f;

    Vector3 cameraPos;
    public bool isActive = false;

    public Material M_Iron;

    public float maxDistance = 33.0f;

    // Use this for initialization
    void Start () {
        player = InventorySystem.instance.player;

        // Material
        M_Iron = GetComponentInChildren<SkinnedMeshRenderer>().material;

        if (gameObject.CompareTag("IRON"))
        {
            player.GetComponent<MagnetController>().IronObject.Add(this.gameObject);
        }
        else if (gameObject.CompareTag("IRONDOOR"))
        {
            player.GetComponent<MagnetController>().IronDoor.Add(this.gameObject);
        }  

    }

    // Update is called once per frame
    void Update () {
        // 자석 활성화 && 조준 상태
        if (CurState == IronState.Active)
        {
            // Rigidbody
            GetComponent<Rigidbody>().useGravity = false;

            // IronObject
            if (this.GetComponent<Collider>().CompareTag("IRON"))
            {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation; //| RigidbodyConstraints.FreezePositionY;

            }
            // IronDoor
            else if (this.GetComponent<Collider>().CompareTag("IRONDOOR"))
            {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition
                                                       |RigidbodyConstraints.FreezeRotationX
                                                       |RigidbodyConstraints.FreezeRotationZ;
            }
            // 색 변환
            M_Iron.SetFloat("_Burn", BurnColor);
            M_Iron.SetColor("_Color", Color.yellow);
        }
        else if(CurState == IronState.Inactive)
        {
            if (this.GetComponent<Collider>().CompareTag("IRON"))
            {
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }

            isActive = true;

            M_Iron.SetFloat("_Burn", BurnColor);
            M_Iron.SetColor("_Color", new Color(1.0f,0.04f,0.42f));
        }
        // 자석 비활성화
        else
        {
            if (this.GetComponent<Collider>().CompareTag("IRON"))
            {
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }

            isActive = false;
            M_Iron.SetColor("_Color", Color.white);
            M_Iron.SetFloat("_Burn", 1.0f);
        }
	}
}
