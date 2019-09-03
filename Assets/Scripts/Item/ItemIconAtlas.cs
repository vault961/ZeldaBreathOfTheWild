using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ItemIconAtlas : MonoBehaviour {
    [HideInInspector]
    public SpriteAtlas atlas;

    private void Awake()
    {
        atlas = Resources.Load("Item/_ItemIconAtlas") as SpriteAtlas;
    }
}
