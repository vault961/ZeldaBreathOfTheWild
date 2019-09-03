using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianState : MonoBehaviour {

    public bool isMoving = false;
    public bool isCharging = false;
    public bool isAttacking = false;
    public bool isDead = false;
    public bool isDamaging = false;

    public float maxHP = 100.0f;
    public float currentHP = 100.0f;
}
