using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyCardScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] TextMeshProUGUI noOfPlayers;
    private Lobby lobby;

    private void Awake() {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(joinLobby);
    }

    public void setData(string _name, string _players,Lobby _lobby){
        nameTxt.text = _name;
        noOfPlayers.text = _players;
        lobby = _lobby;
    }

    public void joinLobby(){
        LobbyManager.Instance.JoinLobby(lobby);
    }
}
