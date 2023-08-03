using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

//debug
using TMPro;

public class RoomManager : NetworkBehaviour{
    public static RoomManager Instance {get; private set;}
    [SerializeField] int mode=0; // 0 waiting, 1 guessing, 2 drawing

    public int currentTool = 0; // 0 pen, 1 eraser, 2 fill, 3 lens
    public Color32 currentColor = new Color32(255, 255, 255, 255);
    //debug comment uncomment after debug
    /*
    private static int maxDrawingTime = 120; //120sec
    private static int maxWaitingTime = 5; //5sec
    */
    //debug delete
    private static int maxDrawingTime = 10; //120sec
    private static int maxWaitingTime = 5; //5sec


    public int gameMode = 0; //0 waiting, 1 guessing, 2 drawing
    //private int totalPlayers = 0; //clientIds size
    // DEBUG COMMENT private List<ulong> clientIdQueue = new List<ulong>(GameManager.Instance.maxPlayers); //list holding client ids
    private List<ulong> clientIdQueue = new List<ulong>(8);
    //delete above line after debug and uncomment above
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
    public TextMeshProUGUI timeTxt;
    public int totalPlayers=0;

    private void Awake() {
        Instance = this;

        //Debug commented
        /*
        if(GameManager.Instance.isLobbyHost){
            NetworkManager.Singleton.StartHost();
            isWaiting.Value = true;
            //totalPlayers=1;
            currentArtist.Value = OwnerClientId;
            currentArtistIndex = 0;
            timer.Value = 0;
            clientIdQueue.Add(OwnerClientId);
        }else{
            NetworkManager.Singleton.StartClient();
            AddMeToQueueServerRpc(OwnerClientId);
            if(!isWaiting.Value){ // auto switch to guessing mode if not waiting
                gameMode=1;
                //set gamemode to 1
            } 
        }
        */

        //debug code delete after testing
        isWaiting.Value = true;
    }

    [ServerRpc]
    void AddMeToQueueServerRpc(ulong _clientId){
        clientIdQueue.Add(_clientId);
        //totalPlayers=clientIdQueue.Count;
    }

    private void Update() {
        //debug
        timeTxt.text = ((int)timer.Value).ToString();
        totalPlayers = clientIdQueue.Count;

        if(isWaiting.Value){
            if(gameMode != 0){
                //switch to waiting mode
                gameMode = 0;
            }    
        }

        //below this, only host can go
        if(!IsHost) return;
        if(clientIdQueue.Count<2){
            //timer.Value=0; ?
            //switch to witing mode and send waiting message
            //set everyone in waiting mode
            if(!isWaiting.Value){
                isWaiting.Value =true;
                Debug.Log("waitning for others");
            }
            //return();
        }else if(timer.Value <=0){
            if(!isWaiting.Value){
                //switch gamemode accordingly and set timer to max timer
                if(round<=maxRounds){
                    currentArtist.Value = clientIdQueue[currentArtistIndex];
                    currentArtistIndex++;
                    if(currentArtistIndex>=clientIdQueue.Count){
                        currentArtistIndex=0;
                        round++;
                        Debug.Log("next round");
                    }
                    timer.Value = maxDrawingTime;
                    ChangeGameModeClientRpc();
                }else{
                    isWaiting.Value = true;
                }
            }else{
                //wait for new game to start
                //iswaiting.value=false;
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
        }else{
            //load guessing mode
            gameMode =1;
            //debug
            drawingMode.SetActive(false);
            guessingMode.SetActive(true);
            Debug.Log("guessing mode");
        }
    }
    
    private IEnumerator WaitForNewGame(int timeDuration){
        //debug
        drawingMode.SetActive(false);
        guessingMode.SetActive(false);
        Debug.Log("new game starts in 5 sec");

        isLocalWaiting=true;
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

    //debug
    public void StartClientbtn(){
        NetworkManager.Singleton.StartClient();
            AddMeToQueueServerRpc(OwnerClientId);
            if(!isWaiting.Value){ // auto switch to guessing mode if not waiting
                gameMode=1;
                //set gamemode to 1
            } 
    }
    public void StartHostbtn(){
        NetworkManager.Singleton.StartHost();
            isWaiting.Value = true;
            //totalPlayers=1;
            currentArtist.Value = OwnerClientId;
            currentArtistIndex = 0;
            timer.Value = 0;
            clientIdQueue.Add(OwnerClientId);
    }
}
