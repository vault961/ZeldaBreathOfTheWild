using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DontDestroyUI : MonoBehaviour
{

    private Canvas[] UICanvases;

    // Use this for initialization
    void Start()
    {
        UICanvases = GetComponentsInChildren<Canvas>();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        foreach (var canvas in UICanvases)
        {
            if (Global.gameState != Global.GameState.GAME && !canvas.CompareTag("Inventory"))
            {
                canvas.enabled = false;
            }
            else
            {
                canvas.enabled = true;
            }
        }
        if (SceneManager.GetActiveScene().name != "Zelda_Play")
        {
            UICanvases[1].enabled = false;
        }
    }
}
