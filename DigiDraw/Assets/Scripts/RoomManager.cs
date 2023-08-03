using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class RoomManager : NetworkBehaviour{
    public static RoomManager Instance {get; private set;}
    [SerializeField] int mode=0; // 0 waiting, 1 guessing, 2 drawing

    public int currentTool = 0; // 0 pen, 1 eraser, 2 fill, 3 lens
    public Color32 currentColor = new Color32(255, 255, 255, 255);
    private static int maxDrawingTime = 120; //120sec
    private static int maxWaitingTime = 5; //5sec

    public int gameMode = 0; //0 waiting, 1 guessing, 2 drawing
    //private int totalPlayers = 0; //clientIds size
    private List<ulong> clientIdQueue = new List<ulong>(GameManager.Instance.maxPlayers); //list holding client ids
    private int currentArtistIndex = 0;
    private int round = 1; 
    private int maxRounds = 2; //max 2 rounds per game

    private NetworkVariable<ulong> currentArtist = new NetworkVariable<ulong>(); //who is drawing now
    private NetworkVariable<int> timer = new NetworkVariable<int>();
    private NetworkVariable<bool> isWaiting = new NetworkVariable<bool>(); //may use after game ends

    //will be needing in future
    [SerializeField] GameObject selected; // selected box image
    [SerializeField] Image currentColorImage; // image to show current color

    private void Awake() {
        Instance = this;

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
    }

    [ServerRpc]
    void AddMeToQueueServerRpc(ulong _clientId){
        clientIdQueue.Add(_clientId);
        //totalPlayers=clientIdQueue.Count;
    }

    private void Update() {

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
                    }
                    timer.Value = maxDrawingTime;
                    ChangeGameModeClientRpc();
                }else{
                    isWaiting.Value = true;
                    StartCoroutine(TimerCoroutine(maxWaitingTime));
                    //send results
                    isWaiting.Value = false;
                }
            }
        }else{
            timer.Value-=(int)Time.deltaTime;
        }
    }

    [ClientRpc]
    void ChangeGameModeClientRpc(){
        if(currentArtist.Value == OwnerClientId){
            //load drawing mode
            gameMode = 2;
        }else{
            //load guessing mode
            gameMode =1;
        }
    }

    private IEnumerator TimerCoroutine(int timerDuration){
        yield return new WaitForSeconds(timerDuration);
    }
}
