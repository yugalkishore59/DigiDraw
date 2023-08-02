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
    private int totalPlayers = 0; //clientIds size
    private List<ulong> clientIds = new List<ulong>(GameManager.Instance.maxPlayers); //list holding client ids
    private bool isLastRound = false;

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
            isWaiting.Value = false;
            totalPlayers=1;
            currentArtist.Value = OwnerClientId;
            timer.Value = 0;
            clientIds.Add(OwnerClientId);
        }else{
            NetworkManager.Singleton.StartClient();
            GetQueueServerRpc(OwnerClientId);
            if(!isWaiting.Value){ // auto switch to guessing mode if not waiting
                gameMode=1;
                //set gamemode to 1
            } 
        }
    }

    [ServerRpc]
    void GetQueueServerRpc(ulong _clientId){
        clientIds.Add(_clientId);
        totalPlayers=clientIds.Count;
    }

    private void Update() {

        //below this, only host can go
        if(!IsHost) return;
        if(totalPlayers<2){
            //timer.Value=0; ?
            //switch to witing mode and send waiting message
            //set everyone in waiting mode
            gameMode = 0;
            //return();
        }else if(timer.Value <=0){
            //switch gamemode accordingly
            /*if(!isWaiting.Value){                                //BUG HERE DO NOT RUN GAME
                ChangeGameModeClientRpc();
            }*/
        }else{
            timer.Value-=(int)Time.deltaTime;
        }
    }

    [ClientRpc]
    public void ChangeGameModeClientRpc(){
        if(isLastRound){ //was it last round
                ChangeToWaitingModeServerRpc();
                gameMode=0;
                //send results
        }else if(currentArtist.Value == OwnerClientId){
            gameMode = 2;
        }else{
            gameMode = 1;
        }
        //set tettings accordingly

    }

    [ServerRpc]
    void ChangeToWaitingModeServerRpc(){
        isWaiting.Value =true;
        ChangeGameModeClientRpc();
        StartCoroutine(TimerCoroutine(maxWaitingTime));
        isWaiting.Value = false;
    }

    [ClientRpc]
    void ChangeToWaitingModeClientRpc(){
        //set settings accordingly
    }

    private IEnumerator TimerCoroutine(int timerDuration){
        yield return new WaitForSeconds(timerDuration);
    }
}
