using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ScrollSnaps;

public class IconManager : MonoBehaviour {
    
    public DirectionalScrollSnap directionalScroll;
   
	// Update is called once per frame
	void Update ()
    {
        ImageTransition();
	}

    public void ImageTransition()
    {
        for(int i = 0; i < 4; i++)
        {
            //closestSnapPositionIndex 를 통해서 현재 인덱스가 어디에 위치하였는지 알수 있음
            if (directionalScroll.closestSnapPositionIndex == i)
            {
                GetComponentsInChildren<Image>(true)[i].color = Color.white;
                GetComponentsInChildren<Text>(true)[i].enabled = true;
            }

            else
            {
                GetComponentsInChildren<Image>(true)[i].color = Color.gray;
                GetComponentsInChildren<Text>(true)[i].enabled = false;
            }
        }
    }
}

    

