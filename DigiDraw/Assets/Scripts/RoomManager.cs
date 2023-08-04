using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class RoomManager : NetworkBehaviour{
    public static RoomManager Instance {get; private set;}
    public int currentTool = 0; // 0 pen, 1 eraser, 2 fill, 3 lens
    public Color32 currentColor = new Color32(255, 255, 255, 255);

    private static int maxDrawingTime = 90; //90sec
    private static int maxWaitingTime = 10; //10sec


    public int gameMode = 0; //0 waiting, 1 guessing, 2 drawing
    private List<ulong> clientIdQueue;
    private int currentArtistIndex = 0;
    private int round = 1; 
    private int maxRounds = 2; //max 2 rounds per game
    private bool isLocalWaiting = false;

    private NetworkVariable<ulong> currentArtist = new NetworkVariable<ulong>(); //who is drawing now
    private NetworkVariable<float> timer = new NetworkVariable<float>();
    private NetworkVariable<bool> isWaiting = new NetworkVariable<bool>(); //may use after game ends

    //will be needing in future
    [SerializeField] GameObject selected; // selected box image
    [SerializeField] Image currentColorImage; // image to show current color

    //debug
    public GameObject drawingMode, guessingMode;
    public TextMeshProUGUI timeTxt,lobbyCodeTxt,testvartxt;
    bool isGameStarted =false;

    private void Awake() {
        Instance = this;
        isWaiting.Value = true;
        timer.Value = 0;
        clientIdQueue = new List<ulong>(LobbyManager.Instance.joinedLobby.MaxPlayers); //list holding client ids
    }

    //debug
    private NetworkVariable<int> testVar = new NetworkVariable<int>();

    public void InitializeGame(){
        int.TryParse(LobbyManager.Instance.joinedLobby.Data["MaxDrawingTime"].Value,out maxDrawingTime);
        int.TryParse(LobbyManager.Instance.joinedLobby.Data["MaxRounds"].Value,out maxRounds);

        if(maxDrawingTime == 0) maxDrawingTime =90;
        if(maxRounds == 0) maxRounds = 2;

        if(LobbyManager.Instance.IsLobbyHost()){
            //debug 
            testVar.Value = 1;

            currentArtist.Value = OwnerClientId;
            currentArtistIndex = 0;
            clientIdQueue.Add(OwnerClientId);
            lobbyCodeTxt.text += "\nI am host - "+OwnerClientId;
            lobbyCodeTxt.text += "\ntotal players - "+clientIdQueue.Count;
        }else{
            //debug 
            testVar.Value++;

            lobbyCodeTxt.text += "\ni am client "+OwnerClientId.ToString();
            AddMeToQueueServerRpc(OwnerClientId);
            if(!isWaiting.Value){ // auto switch to guessing mode if not waiting
                gameMode=1;
                //set gamemode to 1
                //debug
                drawingMode.SetActive(false);
                guessingMode.SetActive(true);
                Debug.Log("guessing mode");
                lobbyCodeTxt.text += "\nguessing mode";
            } 
        }
        lobbyCodeTxt.text += "\n"+LobbyManager.Instance.joinedLobby.LobbyCode;
        isGameStarted = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void AddMeToQueueServerRpc(ulong _clientId){
        lobbyCodeTxt.text += "\ncalled server rpc";
        clientIdQueue.Add(_clientId);
        lobbyCodeTxt.text += "\nnew player joined. total = "+clientIdQueue.Count;
    }

    private void Update() {
        testvartxt.text = testVar.Value.ToString();
        if(!isGameStarted) return;
        timeTxt.text = ((int)timer.Value).ToString() + "s";

        if(isWaiting.Value){
            if(gameMode != 0){
                //switch to waiting mode
                gameMode = 0;
            }    
        }
        
        if(!IsServer) return; //below this, only host can go         

        if(clientIdQueue.Count<2){
            if(!isWaiting.Value){
                isWaiting.Value =true;
                Debug.Log("waitning for others");
                if(clientIdQueue.Count==1) lobbyCodeTxt.text += "\nwaiting for others";
            }
        }else if(timer.Value <=0){
            if(!isWaiting.Value){
                if(round<=maxRounds){
                    currentArtist.Value = clientIdQueue[currentArtistIndex];
                    currentArtistIndex++;
                    if(currentArtistIndex>=clientIdQueue.Count){
                        currentArtistIndex=0;
                        round++;
                        Debug.Log("next round");
                        lobbyCodeTxt.text += "\nnext round";
                    }
                    timer.Value = maxDrawingTime;
                    ChangeGameModeClientRpc();
                }else{
                    isWaiting.Value = true;
                }
            }else{
                if(!isLocalWaiting){
                    StartCoroutine(WaitForNewGame(maxWaitingTime));
                }
            }
        }else{
            timer.Value-=Time.deltaTime;
        }
    }

    [ClientRpc]
    void ChangeGameModeClientRpc(){
        if(currentArtist.Value == OwnerClientId){
            //load drawing mode
            gameMode = 2;
            //debug
            drawingMode.SetActive(true);
            guessingMode.SetActive(false);
            Debug.Log("drawing mode");
            lobbyCodeTxt.text += "drawing mode";
        }else{
            //load guessing mode
            gameMode =1;
            //debug
            drawingMode.SetActive(false);
            guessingMode.SetActive(true);
            Debug.Log("guessing mode");
            lobbyCodeTxt.text += "guessing mode";
        }
    }
    
    private IEnumerator WaitForNewGame(int timeDuration){
        isLocalWaiting=true;
        //debug
        drawingMode.SetActive(false);
        guessingMode.SetActive(false);
        Debug.Log("new game starts in 5 sec");
        lobbyCodeTxt.text += "\nnew game starts in 5 sec";

        yield return new WaitForSeconds(timeDuration);
        //send results
        round=1;
        currentArtistIndex=0;
        isWaiting.Value = false;
        yield return new WaitForSeconds(1);
        isLocalWaiting=false;

    }

    private IEnumerator TimerCoroutine(int timerDuration){
        yield return new WaitForSeconds(timerDuration);
    }
}
