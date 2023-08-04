using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class RelayManager : MonoBehaviour{
    public static RelayManager Instance {get; private set;}

    private void Awake() {
        Instance = this;

        StartGame();
    }

    private async void StartGame(){
        if(LobbyManager.Instance.IsLobbyHost()){
            string relayCode = await CreateRelay();
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(LobbyManager.Instance.joinedLobby.Id,new UpdateLobbyOptions{
                Data = new Dictionary<string, DataObject>{
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Public,relayCode)}
                }
            });
            LobbyManager.Instance.joinedLobby = lobby;
        }else{
             string _relayCode="0";
            while(_relayCode == "0"){
                _relayCode = LobbyManager.Instance.joinedLobby.Data["RelayCode"].Value;
            }
            JoinRelay(_relayCode);
        }
    }

     private async Task<string> CreateRelay(){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.Instance.joinedLobby.MaxPlayers-1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            return joinCode;
        }catch(RelayServiceException e){
            Debug.Log(e);
            return null;
        }
    }

    private async void JoinRelay(string joinCode){
        try{
            await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log(joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }
    }
}
