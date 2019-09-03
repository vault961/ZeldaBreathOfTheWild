using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour {

    //private static Global _instance = null;

    //public void Awake() {
    //    if (_instance == null)
    //    {
    //        _instance = this;
    //    }
    //}


    //public static Global Instance() {
    //    return _instance;
    //}

    public enum GameState {
        MAIN_MENU,
        GAME,
        INVENTORY,
        MAP,
        CHANGE_WEAPON,
        CUT_SCENE,
    }

    public static GameState gameState = GameState.CUT_SCENE;



}
