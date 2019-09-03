using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkP_WaterCtrl : MonoBehaviour {

    public GameObject player;

    public List<ParticleSystem> waterParticle;
    public ParticleSystem waterRipple;
    public ParticleSystem waterSplash;

    public Vector3 postPos;
    public bool inWater;


	// Use this for initialization
	void Start () {

        player = InventorySystem.instance.player;

        inWater = false;

        //for(int i=0; i<4; i++)
        //{   
        //    waterParticle.Add(player.GetComponentInChildren<ParticleSystem>());
        //}

        waterRipple.Play(false);
        waterSplash.Play(false);
    }
	
	// Update is called once per frame
	void Update () {
		
        if(inWater == true)
        {
            if (waterRipple.isPlaying == false)
                waterRipple.Play();
        }
	}

    IEnumerator RippleDelay()
    {
        if(waterRipple.isPlaying == false)
            waterRipple.Play();

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (inWater == false)
            {
                inWater = true;
                waterSplash.Play(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inWater = false;
        }
    }


}
