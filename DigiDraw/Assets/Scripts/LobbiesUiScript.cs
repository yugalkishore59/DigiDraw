using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbiesUiScript : MonoBehaviour{

    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;
    [SerializeField] private TMP_InputField maxRoundsInputField;
    [SerializeField] private TMP_InputField maxTimeInputField;

    [SerializeField] private Toggle isPrivateToggle;

    [SerializeField] GameObject createLobbyPanel;
    [SerializeField] Transform lobbyCardContainer;
    [SerializeField] Transform lobbyCard;
    [SerializeField] GameObject freeDoodleCard;

    //TODO: Add invisible layer to prevent multiple operations

    private void Start() {
        RefreshLobbyList();
    }

    public void JoinLobbyByCode(){
        string lobbyCode = lobbyCodeInputField.text;
        lobbyCodeInputField.text = "";
        LobbyManager.Instance.JoinLobbyByCode(lobbyCode);
    }

    public void RefreshLobbyList(){
        LobbyManager.Instance.RefreshLobbyList();

        foreach(Transform c in lobbyCardContainer){
            Destroy(c.gameObject);
        }
        Instantiate(freeDoodleCard,lobbyCardContainer);
        StartCoroutine(TimerCoroutine(2)); //wait 2 sec to fetch lobby list
        foreach(Lobby lobby in LobbyManager.Instance.lobbyList){
            Transform card =Instantiate(lobbyCard,lobbyCardContainer);
            string _players = (lobby.MaxPlayers-lobby.AvailableSlots).ToString()+"/"+(lobby.MaxPlayers).ToString();
            card.GetComponent<LobbyCardScript>().setData(lobby.Name,_players,lobby);
        }

    }

    public void CreateNewLobby(){
        createLobbyPanel.SetActive(true);
    }

    public void CancelCrateLobby(){
        createLobbyPanel.SetActive(false);
    }

    public void QuickJoinLobby(){
        LobbyManager.Instance.QuickJoinLobby();
    }

    public void EnterLobby(){
        string _name = lobbyNameInputField.text;
        int _maxPlayers,_maxRounds,_maxTime;
        int.TryParse(maxPlayersInputField.text, out _maxPlayers);
        int.TryParse(maxRoundsInputField.text, out _maxRounds);
        int.TryParse(maxTimeInputField.text, out _maxTime);
        bool isPrivate = isPrivateToggle.isOn;

        if(_maxPlayers > 16) _maxPlayers=16;
        else if (_maxPlayers <2 )_maxPlayers = 8;

        if(_maxRounds>5) _maxRounds=5;
        else if ( _maxRounds<1)_maxRounds =2;

        if(_maxTime >300) _maxTime=300;
        else if (_maxTime <30) _maxTime=90;

        LobbyManager.Instance.CreateLobby(_name,_maxPlayers,isPrivate,_maxRounds,_maxTime);
    }

    private IEnumerator TimerCoroutine(int timerDuration){
        yield return new WaitForSeconds(timerDuration);
    }
}
