using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    STATE_IDLE,
    STATE_CLIMBING,
    STATE_COMBAT,
    STATE_TARGET,
    STATE_THROW,
    STATE_ZOOM,
    STATE_MAGNET,
    STATE_MAGNETING
}

public enum Location
{
    Field,
    Dungeon
}


public class Player : MonoBehaviour {

    public static State playerState = State.STATE_IDLE;
    public static Location playerLocation = Location.Field;

    //public float stamina = 10.0f;
    public GameObject hair;
    private Vector3 hairPositionOffset = new Vector3(0f, -1.48f, 0f);

    private GameObject hairBone;
    private GameObject upperWearBone;
    private GameObject underWearBone;
    public GameObject leftHandBone;
    public GameObject rightHandBone;
    public GameObject backBone;
    public GameObject[] interactPanel;

    private InteractComponent interact;
    private bool invenOpen = true;

    public GameObject cameraFollowMagneting;

    public GameObject hardLandingFX;
    public GameObject chargeFX;
    
    public List<Material> materials;

    [HideInInspector]
    public Vector3 currentPos = Vector3.zero;

	// Use this for initialization
	void Awake () {
        
        Transform[] hairPos = GetComponentsInChildren<Transform>();
        foreach (var ch in hairPos) {
            switch (ch.name) {
                case "mixamorig:Head":
                    hairBone = ch.gameObject;
                    break;
                case "mixamorig:Spine":
                    underWearBone = ch.gameObject;
                    break;
                case "mixamorig:Spine1":
                    upperWearBone = ch.gameObject;
                    break;
                case "BackSlot":
                    backBone = ch.gameObject;
                    break;
                case "ShieldSlot":
                    leftHandBone = ch.gameObject;
                    break;
                case "WeaponSlot":
                    rightHandBone = ch.gameObject;
                    break;
                case "CameraFollowMagneting":
                    cameraFollowMagneting = ch.gameObject;
                    break;
            }
        }

        hair = Instantiate(hair, hairBone.transform);
        //hair.layer = LayerMask.NameToLayer("Player");
        Transform[] tran = hair.GetComponentsInChildren<Transform>();
        foreach (Transform t in tran)
        {
            t.gameObject.layer = LayerMask.NameToLayer("Player");
        }

        hair.transform.parent = hairBone.transform;
        hair.transform.localPosition = hairPositionOffset;

        hardLandingFX = Instantiate(hardLandingFX, transform);
        hardLandingFX.transform.parent = transform;

        chargeFX = Instantiate(chargeFX, upperWearBone.transform);
        chargeFX.transform.parent = upperWearBone.transform;
        chargeFX.transform.localPosition = Vector3.zero;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) { materials.Add(r.material); }

        DontDestroyOnLoad(gameObject);
        
    }

    // Update is called once per frame
    void Update() {
        //
        if (Input.GetButtonDown("AButton") && interact != null)
        {
            interact.Interact();
            for (int i = 0; i < interactPanel.Length; ++i)
            {
                interactPanel[i].SetActive(false);
            }
        }

        if (Input.GetButtonDown("Inventory"))
        {
            if (Global.gameState == Global.GameState.GAME)
            {
                Global.gameState = Global.GameState.INVENTORY;
                UIManager.instance.ActiveAccess(invenOpen);
                invenOpen = !invenOpen;
            }
            else if(Global.gameState == Global.GameState.INVENTORY)
            {
                Global.gameState = Global.GameState.GAME;
                UIManager.instance.ActiveAccess(invenOpen);
                invenOpen = !invenOpen;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player")) { return; }

        if (col.gameObject.GetComponent<InteractComponent>() != null && !col.gameObject.GetComponent<InteractComponent>().isUsing)
        {
            interact = col.gameObject.GetComponent<InteractComponent>();
            if (col.CompareTag("Interact"))
            {
                interactPanel[0].SetActive(true);
            }
            else
            {
                interactPanel[1].SetActive(true);
            }
        }
        else if (col.gameObject.GetComponentInParent<InteractComponent>() != null && !col.gameObject.GetComponentInParent<InteractComponent>().isUsing)
        {
            interact = col.gameObject.GetComponentInParent<InteractComponent>();
            if (col.CompareTag("Interact"))
            {
                interactPanel[0].SetActive(true);
            }
            else
            {
                interactPanel[1].SetActive(true);
            }
        }

    }

    void OnTriggerStay(Collider col) {

        if (col.CompareTag("Player")) { return; }

        if (col.gameObject.GetComponent<InteractComponent>() != null && !(col.gameObject.GetComponent<InteractComponent>().isUsing))
        {
            interact = col.gameObject.GetComponent<InteractComponent>();
            if (col.CompareTag("Interact"))
            {
                interactPanel[0].SetActive(true);
            }
            else
            {
                interactPanel[1].SetActive(true);
            }
        }
        else if (col.gameObject.GetComponentInParent<InteractComponent>() != null && !col.gameObject.GetComponentInParent<InteractComponent>().isUsing)
        {
            interact = col.gameObject.GetComponentInParent<InteractComponent>();
            if (col.CompareTag("Interact"))
            {
                interactPanel[0].SetActive(true);
            }
            else
            {
                interactPanel[1].SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player") { return; }

        InteractPanelOff();
    }

    public void InteractPanelOff()
    {
        if (interact != null)
        {
            interact = null;
            for (int i = 0; i < interactPanel.Length; ++i)
            {
                interactPanel[i].SetActive(false);
            }
        }
    }

    public void Reset()
    {
        GetComponent<OpenWorldMovement>().canMove = true;
        float value = 0.0f;
        foreach (var m in GetComponent<Player>().materials)
        {
            m.SetFloat("_SliceAmount", value);
        }
    }
}
