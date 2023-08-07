using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHandler : MonoBehaviour{
    public static GameHandler Instance {get; private set;}

    public int gameMode = 0; //0 waiting, 1 guessing, 2 drawing

    [SerializeField] GameObject guessingUI;
    [SerializeField] GameObject drawingUI;
    [SerializeField] GameObject colorWheelBg;

    public int currentTool = 0; // 0 pen, 1 eraser, 2 fill, 3 lens
    public Color32 currentColor = new Color32(0, 0, 0, 255);

    [SerializeField] Image currentColorImage; 

    [SerializeField] GameObject messageContainer;
    [SerializeField] GameObject messageTxt;
    [SerializeField] private TMP_InputField messageInputField;

    private List<GameObject> messageList = new List<GameObject>();
    [SerializeField] int maxMessages = 5; //DEBUG : change it back to 20
     

    private void Awake() {
        Instance = this;
    }

    public void ChangeGameMode(int mode){
        PixelArtCanvasScript.Instance.GetGrid().ClearCanvas();
        PixelArtCanvasScript.Instance.GetGrid().ResetColors();
        currentTool=0;
        currentColor = new Color32(0, 0, 0, 255);
        currentColorImage.color = currentColor;
        gameMode = mode;
        Camera.main.transform.position = new Vector3(0,-4,-10);
        switch(mode){
            case 0 : WaitingMode();
            break;
            case 1 : GuessingMode();
            break;
            case 2 : DrawingMode();
            break;
        }
        colorWheelBg.SetActive(false);
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
        string _message = FirebaseAndGPGS.Instance.userName + " is drawing";
        RoomManager.Instance.GetPlayerDummyScript().SendNewMessageServerRpc(_message,"DigiDraw",true);
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

    public void SendNewMessage(string _message, string _sender, bool isDidiDraw){
        GameObject newMessage = Instantiate(messageTxt,messageContainer.transform);
        TextMeshProUGUI msg = newMessage.GetComponent<TextMeshProUGUI>();
        messageList.Add(newMessage);
        if(isDidiDraw){
            msg.text =  "<color=yellow><u><b>" +_sender + "</u> : </color> </b>" + _message;
            return;
        }
        msg.text =  "<color=green><u><b>"+_sender+"</u> : </color> </b>" + _message;

        if (messageList.Count > maxMessages){
            // Delete the oldest message
            Destroy(messageList[0]);
            messageList.RemoveAt(0);
        }
    }

    public void SendNewMessageInputField(){
        string _message = messageInputField.text;
        messageInputField.text = "";
        PlayerDummyScript playerScript = RoomManager.Instance.GetPlayerDummyScript();
        //TODO: Check for gussed word
        //TODO: update score and ranking accordingly
        playerScript.SendNewMessageServerRpc(_message,FirebaseAndGPGS.Instance.userName,false);
    }

}
