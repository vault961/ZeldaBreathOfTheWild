using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DgnEnter : InteractComponent
{
    // Update is called once per frame
    public GameObject GroundFX;
    public GameObject ParticleFX;

    public new void Start() {
        player = InventorySystem.instance.player;
        GroundFX = Instantiate(GroundFX, player.transform);
        ParticleFX = Instantiate(ParticleFX, player.transform);
    }

    public override void Interact()
    {
        player.GetComponent<OpenWorldMovement>().canMove = false;
        player.GetComponent<Player>().currentPos = player.transform.position;
        GroundFX.GetComponent<ParticleSystem>().Play();
        ParticleFX.GetComponent<ParticleSystem>().Play();
        StartCoroutine("Dissolve");
        StartCoroutine("SizeUp");
    }

    IEnumerator SizeUp() {
        ParticleSystem ps = GroundFX.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule module = ps.main;
        float size = 0.0f;
        while (ps.main.startSize.constant < 4.5f) {
            size += 0.15f;
            module.startSize = size;

            yield return new WaitForSeconds(0.1f);
        }

        GroundFX.GetComponent<ParticleSystem>().Stop();
        ParticleFX.GetComponent<ParticleSystem>().Stop();

        player.GetComponent<Player>().InteractPanelOff();

        if(player.GetComponent<MagnetController>().IronObject != null)
            player.GetComponent<MagnetController>().IronObject.Clear();

        if (player.GetComponent<MagnetController>().IronDoor != null)
            player.GetComponent<MagnetController>().IronDoor.Clear();


        LoadingSceneManager.LoadScene("Zelda_Dgn");

    }

    IEnumerator Dissolve() {
        float value = 0.0f;
        while (value < 1.0f) {
            foreach (var m in player.GetComponent<Player>().materials)
            {
                m.SetFloat("_SliceAmount", value);
                value += 0.002f;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

}
