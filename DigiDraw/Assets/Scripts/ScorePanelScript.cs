using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePanelScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyCodeTxt;
    [SerializeField] TextMeshProUGUI lobbyNameTxt;

    private void Start() {
        lobbyCodeTxt.text += LobbyManager.Instance.joinedLobby.LobbyCode;
        lobbyNameTxt.text += LobbyManager.Instance.joinedLobby.Name;
    }
    
    public void ShowScoreBoard(){
        gameObject.SetActive(true);
    }

    public void HideScoreBoard(){
        gameObject.SetActive(false);
    }
}
