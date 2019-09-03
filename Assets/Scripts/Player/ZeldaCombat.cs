using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeldaCombat : MonoBehaviour {

    public int currWeapon;
    public int currShield;

    private GameObject weapon;
    private GameObject shield;


    private Vector3 posOffsetBack;
    private Quaternion rotOffsetBack;
    private Vector3 posOffsetBack_Shield;
    private Quaternion rotOffsetBack_Shield;
    private Vector3 posOffsetRight;
    private Quaternion rotOffsetRight;
    private Vector3 posOffsetLeft;
    private Quaternion rotOffsetLeft;

    public Animator anim;
    public OpenWorldMovement owm;
    public Player player;

    public bool hasWeapon;
    public bool isPick;
    public WeaponType weaponType;
    private int pickType;

    private List<int> attackCommand = new List<int>();
    public float combatTime = 5.0f;
    public float elapedTime = 0.0f;

    public float energyTime = 0.0f;
    bool isAutoCharge;

    private MeleeWeaponTrail trail;

    private Vector3 destPos;    // 던질 목표 위치
    public GameObject target;
    float prevLt = 0f;

    public float HP;
    public float maxHP;
    public bool isDamaged;
    public bool isDead;

    public InventoryManager inventoryManager;

    // Use this for initialization
    void Start() {
        hasWeapon = false;
        isPick = false;
        currWeapon = 0;
        currShield = 0;

        posOffsetBack = new Vector3(0, 0, -0.1f);
        rotOffsetBack.eulerAngles = new Vector3(0, 0, 180f);

        posOffsetBack_Shield = new Vector3(0, -0.2f, -0.2f);
        rotOffsetBack_Shield.eulerAngles = new Vector3(0, 180f, 0);

        posOffsetRight = new Vector3(0, 0f, 0);
        rotOffsetRight.eulerAngles = new Vector3(0, 0, 0);

        posOffsetLeft = new Vector3(0, 0f, 0);
        rotOffsetLeft.eulerAngles = new Vector3(0, 0, 0);

        owm = GetComponent<OpenWorldMovement>();
        anim = owm.anim;
        player = GetComponent<Player>();

        maxHP = 60;
        HP = maxHP;

        //CreateWeapon();
    }

    // Update is called once per frame
    void Update() {
        if (Global.gameState != Global.GameState.GAME) { return; }
        //if (Player.playerState == State.STATE_CLIMBING) { return; }
        //if (Player.playerState == State.STATE_IDLE) {
        //    OnIdle();
        //} else if (Player.playerState == State.STATE_COMBAT) {
        //    OnCombat();
        //}
        if (CheckDeath()) { return; }
        CheckLeftTrigger();
        //Debug.Log(hasWeapon);
        switch (Player.playerState)
        {
            case State.STATE_IDLE:
                OnIdle();
                break;
            case State.STATE_CLIMBING:
                return;
            case State.STATE_COMBAT:
                OnCombat();
                break;
            case State.STATE_TARGET:
                OnTarget();
                break;
            case State.STATE_THROW:
                OnThrow();
                break;
            default:
                break;
        }
        //Debug.Log(energyTime);

        /////// 임시 아이템 장착
        if (Input.GetKeyDown(KeyCode.Q)) {
            currWeapon = 1;
            CreateWeapon(currWeapon);
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            //currShield = 1;
            currShield = inventoryManager.selectedItemSlot.GetComponent<ItemSlot>()._item.count;
            CreateShield(currShield);
        }
    }

    // idle 일때의 업데이트
    void OnIdle() {
        if (Input.GetButtonDown("YButton")) {
            if (!owm.jumping)       // 점프 아닐때
            {
                if (!hasWeapon)             // 무기가 없을 떈 장착
                {
                    anim.SetInteger("CurrShield", currShield);
                    anim.SetTrigger("Equip");

                }
                else
                {                           // 무기가 있으면 기 모으기
                    owm.canMove = false;
                    anim.SetBool("Charge", false);
                    anim.SetBool("StopCombat", false);
                    anim.SetBool("Combat", true);
                    anim.SetTrigger("Attack");
                    player.chargeFX.GetComponent<ParticleSystem>().Play();
                }
            }
            else {                  // 점프 중일 때
                if (hasWeapon && !anim.GetBool("JumpAttack")) {
                    anim.SetTrigger("Attack");
                }
            }
        }
        
        if (Input.GetButton("YButton")) {   // 입력 지속시 기 모으기
            if (owm.jumping) { return; }
            if (hasWeapon) {
                energyTime += Time.deltaTime * 2f;
                //Debug.Log(energyTime);

                if (energyTime > 5f) {
                    anim.SetBool("Charge", true);
                    Player.playerState = State.STATE_COMBAT;
                    elapedTime = 0.0f;
                    anim.SetTrigger("StartAttack");
                    isAutoCharge = true;
                    player.chargeFX.GetComponent<ParticleSystem>().Stop();
                }
            }
        }

        if (Input.GetButtonUp("YButton")) {
            if (owm.jumping) { return; }
            if (hasWeapon) {
                player.chargeFX.GetComponent<ParticleSystem>().Stop();
                if (energyTime < 2f) {              // 짧게 모았을 땐 일반 공격
                    anim.SetBool("Charge", false);
                }
                else {
                    anim.SetBool("Charge", true);   // 길게 모았을 땐 차지 공격
                }
                Player.playerState = State.STATE_COMBAT;
                elapedTime = 0.0f;
                anim.SetTrigger("StartAttack");
            }
        }

        if (Input.GetButtonDown("BButton"))     // 장비 해제
        {
            if (owm.jumping) { return; }
            if (hasWeapon)
            {
                anim.SetInteger("CurrShield", currShield);
                anim.SetTrigger("UnEquip");
            }
        }

        if (Input.GetButtonDown("RButton")) {      // 던지기 버튼
            if (owm.jumping) { return; }

            if (!hasWeapon)       // 무기가 없을 떈 장착
            {
                anim.SetInteger("CurrShield", currShield);
                anim.SetTrigger("Equip");

            }
            else                  // 무기가 있을땐 던지기 자세
            {
                Player.playerState = State.STATE_THROW;
                anim.SetBool("Throw", true);
            }
        }
        
    }

    // combat일 때의 업데이트
    void OnCombat() {
        elapedTime += Time.deltaTime;   // combat 자세 지속시간

        if (Input.GetButtonDown("YButton")) {
            if (!owm.jumping)
            {
                if (anim.GetBool("StopCombat") == false)    // 첫 공격 아닐땐 콤보 추가
                {
                    attackCommand.Add(1);
                }
                else
                {              // 첫 공격 일땐 기 모으기
                    owm.canMove = false;
                    anim.SetBool("Charge", false);
                    //anim.SetBool("StopCombat", false);
                    anim.SetBool("Combat", true);
                    anim.SetTrigger("Attack");
                    player.chargeFX.GetComponent<ParticleSystem>().Play();
                }
            }
            else {
                if (hasWeapon && !anim.GetBool("JumpAttack")) {
                    anim.SetTrigger("Attack");
                }
            }
            elapedTime = 0.0f;      // combat자세 시간 초기화
        }

        if (Input.GetButton("YButton"))     // 입력 지속시 기 모으기
        {
            if (owm.jumping) { return; }
            if (hasWeapon && anim.GetBool("StopCombat") == true) {
                energyTime += Time.deltaTime * 2f;

                if (energyTime > 5f)
                {
                    anim.SetBool("Charge", true);
                    //Player.playerState = State.STATE_COMBAT;
                    elapedTime = 0.0f;
                    anim.SetTrigger("StartAttack");
                    isAutoCharge = true;
                    anim.SetBool("StopCombat", false);
                    player.chargeFX.GetComponent<ParticleSystem>().Stop();
                }
            }
        }

        if (Input.GetButtonUp("YButton"))
        {
            if (owm.jumping) { return; }
            if (isAutoCharge) { isAutoCharge = false; return; }
            if (hasWeapon && anim.GetBool("StopCombat") == true)
            {
                player.chargeFX.GetComponent<ParticleSystem>().Stop();
                if (energyTime < 2f)        // 짧게 모았을 땐 일반 공격
                {
                    anim.SetBool("Charge", false);
                }
                else                        // 길게 모았을 땐 차지 공격
                {
                    anim.SetBool("Charge", true);
                }
                //Player.playerState = State.STATE_COMBAT;
                anim.SetBool("StopCombat", false);
                elapedTime = 0.0f;
                anim.SetTrigger("StartAttack");
            }
        }

        if (elapedTime > combatTime) {      // 일정 시간 경과하면 idle 상태로
            Player.playerState = State.STATE_IDLE;
            anim.SetBool("Combat", false);
            anim.SetBool("StopCombat", true);
            elapedTime = 0.0f;
            energyTime = 0.0f;
        }

        if (Input.GetButtonDown("RButton")) {      // 던지기 버튼
            if (owm.jumping || anim.GetBool("StopCombat") == false) { return; }

            Player.playerState = State.STATE_THROW;
            anim.SetBool("Throw", true);
            elapedTime = 0.0f;
            energyTime = 0.0f;
        }
    }

    // Throw일 때 업데이트
    void OnThrow() {
        // 화면 중간으로 라인트레이스
        CheckThrowDest();

        if (Input.GetButtonUp("RButton")) {     // 무기 던지기
            owm.canMove = false;
            anim.SetTrigger("StartThrow");
            //anim.CrossFade("ThrowObject", 0.1f, 0, 0f, 1f);

        }

        if (Input.GetButtonDown("BButton")) {
            EndThrow();
        }

    }

    // 무기 생성
    public void CreateWeapon(int index) {
        currWeapon = index;
        if (currWeapon == 0) { return; }

        if (weapon != null) {       // 다른걸 들고있으면
            Destroy(weapon, 0f);
        }

        StartCoroutine("CreateWeaponCour", index);

    }

    // 방패 생성
    public void CreateShield(int index) {
        currShield = index;
        if (currShield == 0) { return; }
        if (shield != null) {           // 다른걸 들고있으면
            Destroy(shield, 0f);         // 기존에 들고있는거 파괴
        }

        StartCoroutine("CreateShieldCour", index);
    }

    // 무기 위치 변경
    void ChangeWeaponParent() {
        if (currWeapon == 0) { return; }
        if (hasWeapon == false && isPick == false)
        {

            weapon.transform.parent = player.rightHandBone.transform;
            weapon.transform.localPosition = posOffsetRight;
            weapon.transform.localRotation = rotOffsetRight;
        }
        else
        {
            weapon.transform.parent = player.backBone.transform;
            weapon.transform.localPosition = posOffsetBack;
            weapon.transform.localRotation = rotOffsetBack;
        }
        ChangeShieldParent();

        if (!isPick) {
            hasWeapon = !hasWeapon;
        }
    }

    // 방패 위치 변경
    void ChangeShieldParent() {
        if (currShield == 0) { return; }
        if (hasWeapon == false)
        {
            shield.transform.parent = player.leftHandBone.transform;
            shield.transform.localPosition = posOffsetLeft;
            shield.transform.localRotation = rotOffsetLeft;
        }
        else
        {
            shield.transform.parent = player.backBone.transform;

            shield.transform.localPosition = posOffsetBack_Shield;
            shield.transform.localRotation = rotOffsetBack_Shield;
        }
    }

    void CheckNextAttack() {
        if (attackCommand.Count == 0)   // 공격 끝
        {
            anim.SetBool("StopCombat", true);
            anim.SetBool("NextAttack", false);
            owm.canMove = true;
            //Player.playerState = State.STATE_IDLE;
        }
        else
        {
            attackCommand.RemoveAt(0);
            anim.SetBool("NextAttack", true);
        }
        energyTime = 0.0f;
    }

    void EndAttack() {
        owm.canMove = true;
        anim.SetBool("StopCombat", true);
        anim.SetBool("NextAttack", false);
        energyTime = 0.0f;
        attackCommand.Clear();
        //Player.playerState = State.STATE_IDLE;
    }

    void ToggleEmit() {
        trail.ToggleEmit();
        weapon.GetComponent<BoxCollider>().enabled = !weapon.GetComponent<BoxCollider>().enabled;
        weapon.GetComponent<WeaponComponent>().isUsing = !weapon.GetComponent<WeaponComponent>().isUsing;
    }

    void ToggleCollider() {
        weapon.GetComponent<BoxCollider>().enabled = !weapon.GetComponent<BoxCollider>().enabled;
        weapon.GetComponent<WeaponComponent>().isUsing = !weapon.GetComponent<WeaponComponent>().isUsing;
    }

    void StartGround() {
        owm.canMove = false;
        anim.SetBool("JumpAttack", true);
    }

    void EndGround() {
        owm.canMove = true;
        anim.SetBool("JumpAttack", false);
    }

    void EndThrow() {
        anim.SetBool("Throw", false);
        if (anim.GetBool("Combat"))
        {
            Player.playerState = State.STATE_COMBAT;
        }
        else
        {
            Player.playerState = State.STATE_IDLE;
        }
        owm.canMove = true;
        //Debug.Log("ed");
    }

    void CheckThrowDest() {
        RaycastHit hit;
        Transform camTr = Camera.main.transform.parent;
        Debug.DrawRay(camTr.position, camTr.localRotation * Vector3.forward * 10f, Color.red);
        if (Physics.Raycast(camTr.position, camTr.localRotation * Vector3.forward, out hit, 10f))
        {
            destPos = hit.point;
        }
        else
        {
            destPos = camTr.position + (camTr.localRotation * Vector3.forward * 10f);
        }
    }

    void ThrowObject() {            // 무기 던지기
        weapon.GetComponent<BoxCollider>().enabled = true;      // 콜리더 켜줌
        weapon.GetComponent<BoxCollider>().isTrigger = false;   // 무기를 콜리더로 변경
        weapon.GetComponent<Rigidbody>().isKinematic = false;   // 물리 연산 받도록
        weapon.transform.parent = null;                         // 부모를 월드로
        
        Vector3 force = (destPos - weapon.transform.position).normalized;

        weapon.transform.localRotation = Quaternion.Euler(50f, 0, 90f);
        //weapon.GetComponent<Rigidbody>().AddTorque(transform.up * 30f);
        weapon.GetComponent<Rigidbody>().AddForce(force * 30f, ForceMode.Impulse);

        weapon.GetComponentInChildren<ParticleSystem>().Play();
        weapon.GetComponent<WeaponComponent>().isUsing = true;
        weapon.GetComponent<WeaponComponent>().StartCoroutine(weapon.GetComponent<WeaponComponent>().ChangeIsThrow());
        InventorySystem.instance.RemoveItem(ItemType.TYPE_WEAPON, currWeapon - 1);
        weapon = null;
        hasWeapon = false;
        currWeapon = 0;
    }

    public void PickItem(ItemType type) {
        anim.SetBool("PickUp", true);
        owm.canMove = false;
        pickType = (int)type;

        switch (type) {
            case ItemType.TYPE_WEAPON:
                if (hasWeapon == false && currWeapon == 0)
                {
                    CreateWeapon(InventorySystem.instance.itemLists[(int)type].Count);
                }
                break;
            default:
                break;
        }
        
    }

    void EndPick() {
        owm.canMove = true;
        anim.SetBool("PickUp", false);
        if (pickType == (int)ItemType.TYPE_WEAPON)
        {
            ChangeWeaponParent();
        }
        isPick = false;
    }

    void OnTriggerEnter(Collider col) {
        if (isDead) { return; }
        if (col.gameObject.layer == LayerMask.NameToLayer("EnemyWeapon")) {
            GetDamage(10.0f);
        }
    }

    public void GetDamage(float damage) {
        if (isDamaged == true) { return; }
        isDamaged = true;
        //owm.canMove = false;
        //col.GetComponent<>().damage 만큼 체력감소
        HP -= damage;
        if (anim.GetBool("StopCombat") == false) { EndAttack(); }
        anim.SetBool("IsDamaged", true);
    }

    void EndHit() {
        isDamaged = false;
        //owm.canMove = true;
        anim.SetBool("IsDamaged", false);
    }

    bool CheckDeath() {
        if (isDead) { return true; }
        if (HP <= 0f) {
            isDead = true;
            owm.canMove = false;
            anim.SetTrigger("IsDeath");
            return true;
        }
        return false;
    }

    IEnumerator CreateWeaponCour(int index) {
        ITEM.Item item = InventorySystem.instance.GetItemFromInventory(ItemType.TYPE_WEAPON, index - 1);


        Transform tr = (hasWeapon) ? player.rightHandBone.transform : player.backBone.transform;
        weapon = Instantiate(Resources.Load("Prefabs/" + item.itemData.Name), tr) as GameObject;

        if (hasWeapon == false && isPick == false)
        {
            weapon.transform.parent = player.backBone.transform;
            weapon.transform.localPosition = posOffsetBack;
            weapon.transform.localRotation = rotOffsetBack;
        }
        else
        {
            weapon.transform.parent = player.rightHandBone.transform;
            weapon.transform.localPosition = posOffsetRight;
            weapon.transform.localRotation = rotOffsetRight;
        }

        //Destroy(weapon.GetComponent<Rigidbody>());
        weapon.GetComponent<Rigidbody>().isKinematic = true;
        weapon.GetComponent<BoxCollider>().isTrigger = true;
        weapon.GetComponent<BoxCollider>().enabled = false;     // 무기는 휘두를때만 콜리더 킴
        weapon.GetComponent<WeaponComponent>().weaponItem.itemData = item.itemData;
        //weaponType = weapon.GetComponent<WeaponComponent>().weaponItem.weaponType;
        weapon.GetComponent<WeaponComponent>().isUsing = false;
        trail = weapon.GetComponent<MeleeWeaponTrail>();        // 현재 무기의 트레일 이펙트에 접근

        weapon.GetComponentInChildren<ParticleSystem>().Stop();

        yield return null;
    }

    IEnumerator CreateShieldCour(int index) {
        ITEM.Item item = InventorySystem.instance.GetItemFromInventory(ItemType.TYPE_SHIELD, index - 1);


        Transform tr = (hasWeapon) ? player.leftHandBone.transform : player.backBone.transform;
        shield = Instantiate(Resources.Load("Prefabs/" + item.itemData.Name), tr) as GameObject;

        if (hasWeapon == false)     // 생성 위치 보정
        {
            shield.transform.parent = player.backBone.transform;
            shield.transform.localPosition = posOffsetBack_Shield;
            shield.transform.localRotation = rotOffsetBack_Shield;
        }
        else
        {
            shield.transform.parent = player.leftHandBone.transform;
            shield.transform.localPosition = posOffsetLeft;
            shield.transform.localRotation = rotOffsetLeft;
        }

        //Destroy(shield.GetComponent<Rigidbody>());
        shield.GetComponent<Rigidbody>().isKinematic = true;
        shield.GetComponent<ShieldComponent>().shieldItem.itemData = item.itemData;
        shield.GetComponent<ShieldComponent>().isUsing = true;
        shield.GetComponent<BoxCollider>().isTrigger = true;

        shield.GetComponentInChildren<ParticleSystem>().Stop();

        yield return null;
    }

    void CheckLeftTrigger() {
        if (Player.playerState != State.STATE_IDLE && Player.playerState != State.STATE_COMBAT && Player.playerState != State.STATE_TARGET) { return; }
        if (owm.jumping) { return; }
        float lt = Input.GetAxisRaw("LeftTrigger");
        if (lt >= 0.9f)
        {
            int layer = (1 << 14);
            //Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward * 5f, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 15f, layer)) {
                if (target == null)
                {
                    target = hit.collider.gameObject;
                    Player.playerState = State.STATE_TARGET;
                    anim.SetBool("Target", true);
                }
            }
        }
        else if(prevLt >= 0.9f){
            if (anim.GetBool("Combat"))
            {
                Player.playerState = State.STATE_COMBAT;
            }
            else
            {
                Player.playerState = State.STATE_IDLE;
            }
            target = null;
            anim.SetBool("Target", false);
        }
        prevLt = lt;
    }


    void OnTarget() {
        if (Input.GetButtonDown("YButton"))
        {
            if (!owm.jumping)       // 점프 아닐때
            {
                if (!hasWeapon)             // 무기가 없을 떈 장착
                {
                    anim.SetInteger("CurrShield", currShield);
                    anim.SetTrigger("Equip");

                }
                else if (anim.GetBool("StopCombat") == false)    // 첫 공격 아닐땐 콤보 추가
                {
                    attackCommand.Add(1);
                }
                else{                           // 무기가 있으면 기 모으기
                    owm.canMove = false;
                    anim.SetBool("Charge", false);
                    //anim.SetBool("StopCombat", false);
                    anim.SetBool("Combat", true);
                    anim.SetTrigger("Attack");
                    player.chargeFX.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        if (Input.GetButton("YButton"))
        {   // 입력 지속시 기 모으기
            if (owm.jumping) { return; }
            if (hasWeapon)
            {
                energyTime += Time.deltaTime * 2f;
                //Debug.Log(energyTime);

                if (energyTime > 5f)
                {
                    anim.SetBool("Charge", true);
                    //Player.playerState = State.STATE_COMBAT;
                    elapedTime = 0.0f;
                    anim.SetTrigger("StartAttack");
                    isAutoCharge = true;
                    anim.SetBool("StopCombat", false);
                    player.chargeFX.GetComponent<ParticleSystem>().Stop();
                }
            }
        }

        if (Input.GetButtonUp("YButton"))
        {
            if (owm.jumping) { return; }
            if (isAutoCharge) { isAutoCharge = false; }
            if (hasWeapon && anim.GetBool("StopCombat") == true)
            {
                player.chargeFX.GetComponent<ParticleSystem>().Stop();
                if (energyTime < 2f)
                {              // 짧게 모았을 땐 일반 공격
                    anim.SetBool("Charge", false);
                }
                else
                {
                    anim.SetBool("Charge", true);   // 길게 모았을 땐 차지 공격
                }
                //Player.playerState = State.STATE_COMBAT;
                elapedTime = 0.0f;
                anim.SetTrigger("StartAttack");
                anim.SetBool("StopCombat", false);

            }
        }

        if (Input.GetButtonDown("BButton"))     // 장비 해제
        {
            if (owm.jumping) { return; }
            if (hasWeapon)
            {
                anim.SetInteger("CurrShield", currShield);
                anim.SetTrigger("UnEquip");
            }
        }

        if (Input.GetButtonDown("XButton")) {
            if (anim.GetBool("Backflip") == true) { return; }
            anim.SetBool("Backflip", true);
            owm.canMove = false;
        }

        if (anim.GetBool("Backflip") == true) {
            transform.position = Vector3.Lerp(transform.position, transform.position + (-transform.forward * 0.2f), 0.5f);
        }
    }

    void EndBackflip() {
        anim.SetBool("Backflip", false);
        owm.canMove = true;
    }
}
