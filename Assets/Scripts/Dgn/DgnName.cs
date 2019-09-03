using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DgnName : MonoBehaviour {

    public string startText;

	// Use this for initialization
	void Start () {
        StartCoroutine("PrintDgnName");
    }

    IEnumerator PrintDgnName()
    {
        GetComponent<Text>().text = "<size=50>자력의 힘</size>\n<size=30>- 마 오누의 사당 -</size>";
        

        yield return new WaitForSeconds(5.0f);

        GetComponent<Text>().text = null;

    }

}
