using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokoblinState : MonoBehaviour {

    public enum State
    {
        IDLE = 0,
        SUSPICION,
        COMBAT
    }

    public State currentState = State.IDLE;

    public enum Weapon
    {
        UNARMED = 0,
        BLUNT,
    }

    public Weapon currentWeapon = Weapon.BLUNT;

    public bool isDead = false;
    public bool isAlert = false;
    public bool isAttacking = false;
    public bool isDamaging = false;
    public bool isKnockDown = false;

    public float maxHP = 100.0f;
    public float currentHP = 100.0f;

}
