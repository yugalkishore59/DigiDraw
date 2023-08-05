using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour{
    public static GameHandler Instance {get; private set;}

    private int gameMode = 0; //0 waiting, 1 guessing, 2 drawing

    [SerializeField] GameObject guessingUI;
    [SerializeField] GameObject drawingUI;

    private void Awake() {
        Instance = this;
    }

    public void ChangeGameMode(int mode){
        gameMode = mode;
        switch(mode){
            case 0 : WaitingMode();
            break;
            case 1 : GuessingMode();
            break;
            case 2 : DrawingMode();
            break;
        }
    }

    private void WaitingMode(){
        guessingUI.SetActive(false);
        drawingUI.SetActive(false);
    }
    private void GuessingMode(){
        guessingUI.SetActive(true);
        drawingUI.SetActive(false);
    }
    private void DrawingMode(){
        guessingUI.SetActive(false);
        drawingUI.SetActive(true);
    }
}
