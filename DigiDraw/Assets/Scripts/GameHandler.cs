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
    [SerializeField] private TextMeshProUGUI currentWordTxt;

    private List<GameObject> messageList = new List<GameObject>();
    private int maxMessages = 20; //DEBUG : change it back to 20

    private List<string> easyWordList;
    private List<string> hardWordList;
    private List<string> mediumWordList;
     

    private void Awake() {
        Instance = this;
        FetchWordList();
    }

    public void ChangeGameMode(int mode){
        currentWordTxt.text = "";
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
        }else{
            msg.text =  "<color=green><u><b>"+_sender+"</u> : </color> </b>" + _message;
        }

        if (messageList.Count > maxMessages){
            // Delete the oldest message
            Destroy(messageList[0]);
            messageList.RemoveAt(0);
        }
    }

    public void SendNewMessageInputField(){
        string _message = messageInputField.text;
        bool isEmpty=true;
        //TODO : check and filter explicit words
        foreach(char c in _message){
            if(c !=' '){
                isEmpty= false;
                break;
            }
        }
        messageInputField.text = "";
        if(isEmpty) return;
        PlayerDummyScript playerScript = RoomManager.Instance.GetPlayerDummyScript();
        //TODO: Check for gussed word
        //TODO: update score and ranking accordingly
        playerScript.SendNewMessageServerRpc(_message,FirebaseAndGPGS.Instance.userName,false);
    }

    private void FetchWordList(){
        string documentName="Easy";
        try{
        easyWordList = FirebaseAndGPGS.Instance.FetchWordList(documentName);
        documentName="Medium";
        mediumWordList = FirebaseAndGPGS.Instance.FetchWordList(documentName);
        documentName="Hard";
        hardWordList = FirebaseAndGPGS.Instance.FetchWordList(documentName);
        }catch{
             Debug.Log("error loading words data from firestore");
        }
    }

    //only relay host can fetch new word
    public void GetNewWord(){
        //TODO: get current word according to difficulty
        //for now ramdom difficulty

        int dif = Random.Range(1,3);
        switch(dif){
            case 1 : GetEasyWord();
            break;
            case 2 : GetMediumWord();
            break;
            case 3 : GetHardWord();
            break;
        }
        RoomManager.Instance.GetPlayerDummyScript().SetNewWordServerRpc();
    }

    private void GetEasyWord(){
        RoomManager.Instance.currentWord.Value = easyWordList[Random.Range(0,easyWordList.Count-1)];
        //TODO: change text according to hints
    }

    private void GetMediumWord(){
        RoomManager.Instance.currentWord.Value = mediumWordList[Random.Range(0,mediumWordList.Count-1)];
        //TODO: change text according to hints
    }

    private void GetHardWord(){
        RoomManager.Instance.currentWord.Value = hardWordList[Random.Range(0,hardWordList.Count-1)];
        //TODO: change text according to hints
    }

    public void SetNewWord(){
        currentWordTxt.text = RoomManager.Instance.currentWord.Value.ToString();
    }

}
