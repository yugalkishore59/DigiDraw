using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour{
    public static GameHandler Instance {get; private set;}

    public int gameMode = 0; //0 waiting, 1 guessing, 2 drawing

    [SerializeField] GameObject guessingUI;
    [SerializeField] GameObject drawingUI;

    public int currentTool = 0; // 0 pen, 1 eraser, 2 fill, 3 lens
    public Color32 currentColor = new Color32(255, 255, 255, 255);

    [SerializeField] Image currentColorImage; 

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

    public void SetCurrentColor(Color32 _color){
        currentColor = _color;
        currentColorImage.color = _color;
    }
    public void SetCurrentTool(int _tool){
        currentTool = _tool;
        //TODO : add selected ui
        /*Vector3 _position = new Vector3(-(3-_tool)*180,0,0);
        selected.GetComponent<RectTransform>().anchoredPosition = _position;*/
    }

    public void SetCurrentColorWithHex(string hex){
        Color color = new Color32();
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            SetCurrentColor(color);
        }
        else
        {
            Debug.LogError("Invalid hex code: " + hex);
        }
    }

}
