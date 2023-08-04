using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour{
   public static LobbyManager Instance { get; private set; }

    private string playerName="Guest";
    private float heartbeatTimer=0f;
    private float lobbyPollTimer=0f;
    public Lobby joinedLobby;
    public List<Lobby> lobbyList;

   private void Awake() {
    if(Instance == null){
        Instance = this;
        DontDestroyOnLoad(this);
    }else Destroy(gameObject);

    playerName = FirebaseAndGPGS.Instance.userName;
    Authenticate();
   } 

   private void Update() {
        LobbyHeartbeat();
        HandleLobbyPolling();
   }

   public async void Authenticate() {
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);
        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void LobbyHeartbeat() {
        if (IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f) {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling() {
        if (joinedLobby != null) {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f) {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                if (!IsPlayerInLobby()) {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");
                    joinedLobby = null;
                }
            }
        }
    }

    private bool IsPlayerInLobby() {
        if (joinedLobby != null && joinedLobby.Players != null) {
            foreach (Player player in joinedLobby.Players) {
                if (player.Id == AuthenticationService.Instance.PlayerId) {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsLobbyHost() {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private Player GetPlayer() {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
        });
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool _isPrivate, int _maxRounds = 2, int _maxDrawingTime=90) {
        try{
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions{
                IsPrivate = _isPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>{
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Public,"0")},
                    {"MaxDrawingTime",new DataObject(DataObject.VisibilityOptions.Public,_maxDrawingTime.ToString())},
                    {"MaxRounds",new DataObject(DataObject.VisibilityOptions.Public,_maxRounds.ToString())}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,createLobbyOptions);
            joinedLobby = lobby;
            Debug.Log("Created lobby - "+lobby.Name);
            GameManager.Instance.SetScene("GameMultiplayer");
            
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void RefreshLobbyList() {
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            lobbyList = lobbyListQueryResponse.Results;
            //TODO : update ui
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode) {
        try{
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions{
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode,joinLobbyByCodeOptions);

            Debug.Log("Joined Lobby with code "+lobbyCode);
            Debug.Log(GetPlayer().Data["PlayerName"].Value+" just joined the lobby");

            GameManager.Instance.SetScene("GameMultiplayer");
        }catch(LobbyServiceException e){
            Debug.Log(e);
            //TODO : Android toast - invalid lobby code or something went wrong
        }
    }

    public async void JoinLobby(Lobby lobby) {
        try{
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions {
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id,joinLobbyByIdOptions);
            Debug.Log("Joined Lobby with id "+lobby.Id);
            Debug.Log(GetPlayer().Data["PlayerName"].Value+" just joined the lobby");

            GameManager.Instance.SetScene("GameMultiplayer");
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby() {
        try {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            if(joinedLobby!=null){
                Debug.Log("Quick joined Lobby with id "+joinedLobby.Id);
            Debug.Log(GetPlayer().Data["PlayerName"].Value+" just joined the lobby");
            
            GameManager.Instance.SetScene("GameMultiplayer");
            }else{
                Debug.Log("no lobby found");
                //TODO : Android toast - no lobby found
            }
            
        } catch (LobbyServiceException e) {
            Debug.Log(e);
            
        }
    }

    public async void LeaveLobby() {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId) {  //will use in future
        if (IsLobbyHost()) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

}





/* OLD SCRIPT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class LobbyManager : MonoBehaviour {

    public Lobby lobby;
    private int creatingOrJoiningLobbyStatus= 0; // 0 not creating, 1 creating lobby, 2 error creating lobby

    [SerializeField] TextMeshProUGUI regularLobbyPlayerCount;

    private async void Start() {
        await UnityServices.InitializeAsync(); //initialize unity services
        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("signed in as "+AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update() {
        
    }

    private Player GetPlayer(){
        Player player = new Player{
                    Data = new Dictionary<string, PlayerDataObject>{
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, FirebaseAndGPGS.Instance.userName)}
                    }
                };
        return player;
    }

    public async void CreateLobby(string lobbyName, int maxPlayers){
        try{
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions{
                IsPrivate = true,
                Player = GetPlayer()
            };
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,createLobbyOptions);
            GameManager.Instance.hostLobby = lobby;
            GameManager.Instance.clientLobby = lobby;
            GameManager.Instance.isInLobby=true;
            Debug.Log("Created lobby - "+lobby.Name+" "+lobby.MaxPlayers+" "+lobby.LobbyCode+" "+lobby.Id);
            creatingOrJoiningLobbyStatus = 0; //created
        }catch(LobbyServiceException e){
            Debug.Log(e);
            creatingOrJoiningLobbyStatus = 2; //error
        }
    }

    public async void JoinLobbyByCode(string lobbyCode){
        try{
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions{
                Player = GetPlayer()
            };
            lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode,joinLobbyByCodeOptions);
            GameManager.Instance.clientLobby = lobby;
            GameManager.Instance.isInLobby=true;
            Debug.Log("Joined Lobby with code "+lobbyCode);
            Debug.Log(GetPlayer().Data["PlayerName"].Value+" just joined the lobby");
            creatingOrJoiningLobbyStatus=0; //joined
        }catch(LobbyServiceException e){
            Debug.Log(e);
            creatingOrJoiningLobbyStatus=2; //error
        }
    }

    public async void ExitLobby(){
        try{
            await LobbyService.Instance.RemovePlayerAsync(GameManager.Instance.clientLobby.Id,AuthenticationService.Instance.PlayerId);
            GameManager.Instance.isInLobby = false;
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void DeleteLobby(){
        try{
            await LobbyService.Instance.DeleteLobbyAsync(GameManager.Instance.hostLobby.Id);
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public void JoinOrCreateRegularLobby(string lobbyDataPath){
        GameManager.Instance.lobbyDataPath = lobbyDataPath;
        FirebaseFirestore firebaseFirestore= FirebaseFirestore.DefaultInstance;
        firebaseFirestore.Document(lobbyDataPath).GetSnapshotAsync().ContinueWithOnMainThread(task =>{
            if (task.IsFaulted){
                Debug.LogError("Error fetching data: " + task.Exception);
                return;
            }

            if (task.IsCompleted){
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists){
                    // Data exists, you can retrieve the data using snapshot.ToDictionary()
                    FirebaseLobbyStruct firebaseLobbyStruct = snapshot.ConvertTo<FirebaseLobbyStruct>();
                    if(firebaseLobbyStruct.playersInLobby <= 0){
                        //join as server
                        creatingOrJoiningLobbyStatus = 1; //creating lobby
                        CreateLobby("Regular",8);
                        while(creatingOrJoiningLobbyStatus == 1){
                            //wait until response
                            //or show loading screen
                        }
                        if(creatingOrJoiningLobbyStatus == 2){
                            //if error creating lobby
                            Debug.Log("cannot create lobby");
                            creatingOrJoiningLobbyStatus = 0; //not creating lobby
                            return;
                        }
                        GameManager.Instance.UpdateFirebaseData();
                    }else{
                        //join as client
                        creatingOrJoiningLobbyStatus=1;
                        JoinLobbyByCode(firebaseLobbyStruct.lobbyCode);
                        while(creatingOrJoiningLobbyStatus == 1){
                            //wait until response
                            //or show loading screen
                        }
                        if(creatingOrJoiningLobbyStatus == 2){
                            //if error joining lobby
                            Debug.Log("cannot join lobby");
                            creatingOrJoiningLobbyStatus = 0; //not creating lobby
                            return;
                        }
                        GameManager.Instance.UpdateFirebaseData();
                    }
                }
                else{
                    Debug.Log("Data does not exist.");
                    //join as server
                        creatingOrJoiningLobbyStatus = 1; //creating lobby
                        CreateLobby("Regular",8);
                        while(creatingOrJoiningLobbyStatus == 1){
                            //wait until response
                            //or show loading screen
                        }
                        if(creatingOrJoiningLobbyStatus == 2){
                            //if error creating lobby
                            Debug.Log("cannot create lobby");
                            creatingOrJoiningLobbyStatus = 0; //not creating lobby
                            return;
                        }
                        GameManager.Instance.UpdateFirebaseData();
                }
            }
        });
    }
}*/
