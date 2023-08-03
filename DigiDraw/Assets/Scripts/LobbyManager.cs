using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class LobbyManager : MonoBehaviour {

    private async void Start() {
        await UnityServices.InitializeAsync(); //initialize unity services
        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("signed in as "+AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,createLobbyOptions);
            GameManager.Instance.hostLobby = lobby;
            GameManager.Instance.clientLobby = lobby;
            GameManager.Instance.isInLobby=true;
            Debug.Log("Created lobby - "+lobby.Name+" "+lobby.MaxPlayers+" "+lobby.LobbyCode+" "+lobby.Id);
        }catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode){
        try{
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions{
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode,joinLobbyByCodeOptions);
            GameManager.Instance.clientLobby = lobby;
            Debug.Log("Joined Lobby with code "+lobbyCode);
            Debug.Log(GetPlayer().Data["PlayerName"].Value+" just joined the lobby");
        }catch(LobbyServiceException e){
            Debug.Log(e);
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

    public void JoinOrCreateRegularLobby(){
        
    }
}
