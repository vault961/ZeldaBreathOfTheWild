using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HPCtrl : MonoBehaviour {

    public GameObject hpImagePrefab;

    private ZeldaCombat zeldaHpCombat;

    public List<GameObject> heartList = new List<GameObject>();

    private float createHeartCount;

	// Use this for initialization
	void Start () {
        zeldaHpCombat = InventorySystem.instance.player.GetComponent<ZeldaCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        HeartCtrl();
    }

    public void HeartCtrl()
    {
        createHeartCount = (zeldaHpCombat.maxHP / 20) - heartList.Count;

        for (int i = 0; i < createHeartCount; ++i)
        {
            heartList.Add(Instantiate(hpImagePrefab, gameObject.transform));
        }
        float hpPercent = zeldaHpCombat.HP / 20;

        for (int i = 0; i < heartList.Count; ++i)
        {
            if (hpPercent >= 1.0f)
            {
                heartList[i].GetComponentInChildren<Image>().fillAmount = 1.0f;
            }
            else if (hpPercent < 1.0f)
            {
                heartList[i].GetComponentInChildren<Image>().fillAmount = hpPercent;
                for (int j = i + 1; j < heartList.Count; ++j)
                {
                    heartList[j].GetComponentInChildren<Image>().fillAmount = 0.0f;
                }
                break;
            }
            hpPercent--;
        }
    }
}
