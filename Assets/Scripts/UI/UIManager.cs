using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class UIManager : MonoBehaviour {
    private static UIManager _instance = null;
    public static UIManager instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        if (_instance == null)
            _instance = this;

        else if (_instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public InventoryManager inventoryUI;          //인벤
    
    private void Start()
    {
        inventoryUI.gameObject.SetActive(false);
    }

    public void ActiveAccess(bool active)
    {
        inventoryUI.gameObject.SetActive(active);
    }

    public SpriteAtlas GetItemIconAtlas()
    {
        SpriteAtlas atlas = GetComponent<ItemIconAtlas>().atlas;
        return atlas;
    }
    
}
