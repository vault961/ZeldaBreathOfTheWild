using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractComponent : MonoBehaviour{
    public GameObject player;
    public string itemName;
    public bool isUsing = false;

    public void Start() { player = InventorySystem.instance.player; }
    public abstract void Interact();
}

public class WeaponComponent : InteractComponent{

    public ITEM.Item weaponItem;

    private bool isTrigger = false;
    public bool isCollision = false;

    public GameObject swordHitFx;
    public GameObject swordJumpHitFx;

    private void Awake()
    {
        weaponItem = new ITEM.Item(itemName);
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("IgnoreWeapon")) { return; }
        if (col.tag == "Player" || !isUsing) { return; }
        isTrigger = true;
        
        if (player.GetComponent<ZeldaCombat>().anim.GetBool("JumpAttack"))
        {
            StartCoroutine("CreateParticle", swordJumpHitFx);
        }
        else {
            StartCoroutine("CreateParticle", swordHitFx);
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.tag == "Player") { return; }
        isTrigger = false;
    }

    public override void Interact() {
        InventorySystem.instance.AddItem(weaponItem);
        player.GetComponent<ZeldaCombat>().isPick = true;
        player.GetComponent<ZeldaCombat>().PickItem((ItemType)weaponItem.itemData.Type);
        Destroy(this.gameObject, 0.0f);
    }

    IEnumerator CreateParticle(GameObject fx)
    {
        GameObject effect = Instantiate(fx, transform.position + player.transform.forward * 0.5f, Quaternion.LookRotation(Vector3.up));

        yield return null;
    }

    public IEnumerator ChangeIsThrow() {
        yield return new WaitForSeconds(2.0f);

        isUsing = false;
    }

}
