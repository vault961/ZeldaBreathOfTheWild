using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DgnPotalOpen : InteractComponent
{
    public ParticleSystem potalParticle;

    private new void Start()
    {
        potalParticle.Stop();
    }
    public override void Interact()
    {
        potalParticle.Play();
        potalParticle.gameObject.GetComponent<BoxCollider>().enabled = true;
    }
}
