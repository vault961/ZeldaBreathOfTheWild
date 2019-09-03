using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartGame : MonoBehaviour {


	// Use this for initialization
	public void ChangeGameScene () {
        LoadingSceneManager.LoadScene("Zelda_Play");
    }

	public void QuitGameScene()
	{
		Application.Quit();
	}

	
}
