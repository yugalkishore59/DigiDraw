using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class RoomManager : NetworkBehaviour{
    public static RoomManager Instance {get; private set;}

    private PlayerDummyScript playerScript=null;
    private bool isInitialized = false;
    private bool isStartingNewGame = false;

    private List<ulong> clientIdList = new List<ulong>();
    private int currentArtistIndex = 0;
    private int round = 1; 
    private int maxRounds = 2;
    private int maxDrawingTime = 90;
    private int maxWaitingTime = 5;

    private NetworkVariable<ulong> currentArtist = new NetworkVariable<ulong>(); //who is drawing now
    private NetworkVariable<float> timer = new NetworkVariable<float>();
    private NetworkVariable<bool> isWaiting = new NetworkVariable<bool>();

    [SerializeField] TextMeshProUGUI timeTxt;
    [SerializeField] TextMeshProUGUI lobbyCodeTxt;
    [SerializeField] TextMeshProUGUI playerCountTxt;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        currentArtist.OnValueChanged += OnSomeValueChanged; //added listner
    }

    public void SetPlayerScript(PlayerDummyScript _script){
        playerScript = _script;
        InitializeGame();
    }

    public PlayerDummyScript GetPlayerDummyScript(){
        return playerScript;
    }

    public void InitializeGame(){
        int.TryParse(LobbyManager.Instance.joinedLobby.Data["MaxDrawingTime"].Value,out maxDrawingTime);
        int.TryParse(LobbyManager.Instance.joinedLobby.Data["MaxRounds"].Value,out maxRounds);

        if(maxDrawingTime == 0) maxDrawingTime = 90;
        if(maxRounds == 0) maxRounds = 2;

        lobbyCodeTxt.text = LobbyManager.Instance.joinedLobby.LobbyCode;

        if(playerScript.IsHostPlayer()){
            currentArtist.Value = (ulong)LobbyManager.Instance.joinedLobby.MaxPlayers;
            currentArtistIndex = 0;
            clientIdList.Add(playerScript.OwnerClientIdPlayer());

            //making lobby visible to others if not made private
            bool _isPrivate = LobbyManager.Instance.joinedLobby.Data["IsPrivate"].Value == "true";
            LobbyManager.Instance.ChangeLobbyVisibility(_isPrivate);
        }else{
            playerScript.AddMeToListServerRpc();
            //set gamemode accordingly
            if(isWaiting.Value) GameHandler.Instance.ChangeGameMode(0);
            else{
                GameHandler.Instance.ChangeGameMode(1);
                         
                //below line is not working properly
                playerScript.RequestPixelDataServerRpc(playerScript.OwnerClientIdPlayer());
            }
        }
        isInitialized = true;
        //TODO : share lobby code
    }

    public void AddMeToList(ulong _id){
        clientIdList.Add(_id);
    }

    private void Update() {
        if(!isInitialized) return;
        timeTxt.text = ((int)timer.Value).ToString() + "s";
        //below line can be optimised - ie, execute only if new player joins
        playerCountTxt.text = (LobbyManager.Instance.joinedLobby.MaxPlayers - LobbyManager.Instance.joinedLobby.AvailableSlots).ToString()
                                + "/" + LobbyManager.Instance.joinedLobby.MaxPlayers;
        if(!playerScript.IsHostPlayer()) return;

        HandleTurns();
    }

    public void HandleTurns(){
        if(clientIdList.Count < 2){
            if(!isWaiting.Value){
                isWaiting.Value = true;     
                playerScript.ChangeGameModeClientRpc();
            }     
        }else if(timer.Value<=0){
            if(!isWaiting.Value){
                if(round<=maxRounds){
                    timer.Value = maxDrawingTime;
                    currentArtist.Value = clientIdList[currentArtistIndex];
                    currentArtistIndex+=1;
                    if(currentArtistIndex>=clientIdList.Count){
                        currentArtistIndex=0;
                        round++;
                    }
                    // i think currentArtist.Value didnot synced before this funcion
                    //DEBUG COMMENT
                    //playerScript.ChangeGameModeClientRpc();
                }else{
                    isWaiting.Value = true;
                }
            }else{
                //start new game
                if(!isStartingNewGame){
                    isStartingNewGame = true;
                    StartCoroutine(StartNewGame());
                }
            }
            
        }else{
            timer.Value-=Time.deltaTime;
        }
    }

    private void OnSomeValueChanged(ulong previous, ulong current){
       ChangeGameMode();
    }

    public void ChangeGameMode(){
        // i think currentArtist.Value didnot synced before this funcion
        if(isWaiting.Value) GameHandler.Instance.ChangeGameMode(0); //waiting
        else if(playerScript.OwnerClientIdPlayer()!=currentArtist.Value) GameHandler.Instance.ChangeGameMode(1); //guessing
        else GameHandler.Instance.ChangeGameMode(2); //drawing
    }

    private IEnumerator StartNewGame(){
        //TODO : send message new game starts in 5 sec
        playerScript.ChangeGameModeClientRpc();
        yield return new WaitForSeconds(maxWaitingTime);
        round = 1;
        currentArtistIndex = 0;
        isWaiting.Value = false;
        yield return new WaitForSeconds(1); // making sure new game starts before isStartingNewGame=false
        isStartingNewGame = false;
    }

}




    //old script
    /*private static int maxDrawingTime = 90; //90sec
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
    public TextMeshProUGUI timeTxt,lobbyCodeTxt;
    bool isGameStarted =false;

    private GameObject player=null;
    PlayerDummyScript playerScript;
    bool isPlayerReady = false;
    bool isStartinitialise = false;
    bool isInitialized = false;
    bool isInitializing = false;

    public void SetPlayer(GameObject _player){
        player=_player;
        playerScript=player.GetComponent<PlayerDummyScript>();
        lobbyCodeTxt.text = "\ngot player";
    }

    //debug
    public void AddLog(string s){
        lobbyCodeTxt.text += s;
    }

    private void Awake() {
        Instance = this;
        isWaiting.Value = true;
        timer.Value = 0;
        clientIdQueue = new List<ulong>(LobbyManager.Instance.joinedLobby.MaxPlayers); //list holding client ids
    }
    public void Startinitialise(){
        isStartinitialise = true;
    }

    public void InitializeGame(){
        isInitializing = true;
        int.TryParse(LobbyManager.Instance.joinedLobby.Data["MaxDrawingTime"].Value,out maxDrawingTime);
        int.TryParse(LobbyManager.Instance.joinedLobby.Data["MaxRounds"].Value,out maxRounds);

        if(maxDrawingTime == 0) maxDrawingTime =90;
        if(maxRounds == 0) maxRounds = 2;

        if(playerScript.IsHostPlayer()){

            currentArtist.Value = playerScript.OwnerClientIdPlayer();
            currentArtistIndex = 0;
            clientIdQueue.Add(playerScript.OwnerClientIdPlayer());
            lobbyCodeTxt.text += "\nI am host - "+playerScript.OwnerClientIdPlayer();
            lobbyCodeTxt.text += "\ntotal players - "+clientIdQueue.Count;
        }else if(playerScript.IsClientPlayer()){
            //debug 
            //testVar.Value++;

            //if(IsClient) lobbyCodeTxt.text += "\ni am client "+OwnerClientId.ToString();
            //else lobbyCodeTxt.text += "\ni am not client "+OwnerClientId.ToString();
            //while(!IsClient){}
            lobbyCodeTxt.text += "\ni am client "+playerScript.OwnerClientIdPlayer().ToString();
            playerScript.AddMeToQueue();
            if(!isWaiting.Value){ // auto switch to guessing mode if not waiting
                gameMode=1;
                //set gamemode to 1
                //debug
                drawingMode.SetActive(false);
                guessingMode.SetActive(true);
                Debug.Log("guessing mode");
                lobbyCodeTxt.text += "\nguessing mode";
            } 
        }else{
            lobbyCodeTxt.text += "\n not host not client"; 
            isInitializing = false;
            return;
        }
        lobbyCodeTxt.text += "\n"+LobbyManager.Instance.joinedLobby.LobbyCode;
        lobbyCodeTxt.text += "\n relay code - "+LobbyManager.Instance.joinedLobby.Data["RelayCode"].Value;
        isGameStarted = true;
        isInitialized = true;
    }

    public void AddMeToQueue(ulong _clientId){
        lobbyCodeTxt.text += "\ncalled server rpc";
        clientIdQueue.Add(_clientId);
        lobbyCodeTxt.text += "\nnew player joined. total = "+clientIdQueue.Count;
    }

    private void Update() {
        //if(Input.touchCount >0) TestServerRpc();
        if(player==null){
            lobbyCodeTxt.text += "\nno player";
            return;
        } 
        if(!isInitialized && !isInitializing &&  isStartinitialise) InitializeGame();
        
        timeTxt.text = ((int)timer.Value).ToString() + "s";
        if(!isGameStarted) return;
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
                    playerScript.ChangeGameMode();
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

    public void ChangeGameMode(){
        if(currentArtist.Value == playerScript.OwnerClientIdPlayer()){
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
}*/
