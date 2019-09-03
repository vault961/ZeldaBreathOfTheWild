using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoom : MonoBehaviour {

    // Zoom Profile
    public PostProcessingProfile vignetteProfile;
    // Magnet Profile
    public PostProcessingProfile MgtProfile;
    // 기존 profile
    PostProcessingProfile DgnProfile;

    public GameObject player;

    public float sensitivityX = 4f;
    public float sensitivityY = 4f;

    float Hdg = 0F;
    float Pitch = 0F;
    
    public int zoom = 20;
    public int normal = 60;
    public float smooth = 5;

    public bool isZoomed = false;

    public float range = 1000.0f;
    public GameObject effect;

    public InventorySystem _inventorySystem;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        DgnProfile = GetComponent<PostProcessingBehaviour>().profile;
        //vignetteProfile.vignette.enabled = false;

        //effect.transform.position = this.transform.position;
    }

    private void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward * range, Color.red);
        
        //if(Input.GetKeyDown(KeyCode.Space))//&& !EventSystem.current.IsPointerOverGameObject())
        //{
        //    RaycastHit hit;
        //    
        //    if(Physics.Raycast(transform.position, transform.position * range, out hit, range))
        //    {
        //        //Ray ray = Camera.main.ScreenPointToRay(hit.transform.position);
        //        
        //        Instantiate(effect, hit.collider.transform.position, hit.collider.transform.rotation);
        //        effect.SetActive(true);
        //        Debug.Log(hit.collider.name);
        //    }
        //}

        if (Input.GetButtonDown("ZoomButton"))
        {
            isZoomed = !isZoomed;
           
        }

        if (isZoomed)
        {
            // ZoomProfile로 교체
            GetComponent<PostProcessingBehaviour>().profile = vignetteProfile;
            vignetteProfile.vignette.enabled = true;

            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, zoom, Time.deltaTime * smooth);
            //vignetteProfile.vignette.enabled = true;
            int layermask = (1 << LayerMask.NameToLayer("Player") | (1 << LayerMask.NameToLayer("IgnoreCamCollision")));
            GetComponent<Camera>().cullingMask = ~layermask;
            
        }
        else if (player.GetComponent<MagnetController>().isUsing == true)
        {
            GetComponent<PostProcessingBehaviour>().profile = MgtProfile;
            MgtProfile.bloom.enabled = true;
            MgtProfile.colorGrading.enabled = true;
        }

        else
        {
            // 기존의 던전 Profile로 교체
            GetComponent<PostProcessingBehaviour>().profile = DgnProfile;
            vignetteProfile.vignette.enabled = false;


            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, normal, Time.deltaTime * smooth);
            //vignetteProfile.vignette.enabled = false;
            GetComponent<Camera>().cullingMask = -1;

        }
    }

   
}
