using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour {

    private OpenWorldMovement movement;
    private Image staminaImage;
    private RawImage staminaImageBack;
    private float maxStamina;
    private Color color;
    private Color backColor;
    // Use this for initialization
    void Start () {
        movement = InventorySystem.instance.player.GetComponent<OpenWorldMovement>();
        staminaImage = GetComponent<Image>();
        staminaImageBack = GetComponentInParent<RawImage>();
        color = staminaImage.color;
        backColor = staminaImageBack.color;
        maxStamina = movement.stamina;
	}
	
	// Update is called once per frame
	void Update () {
        if (staminaImage.fillAmount == 1.0f) { color.a = backColor.a = 0.0f; }
        else { color.a = backColor.a = 1.0f; }
        staminaImage.color = color;
        staminaImageBack.color = backColor;
        staminaImage.fillAmount = movement.stamina / maxStamina;
    }
}
