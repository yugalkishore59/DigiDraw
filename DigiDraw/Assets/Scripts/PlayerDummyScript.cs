using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerDummyScript : NetworkBehaviour {


    TextMeshProUGUI log;
    private void Start() {
        log = GameObject.FindWithTag("log").GetComponent<TextMeshProUGUI>();
        transform.position = new Vector3(Random.Range(-7,7),Random.Range(-7,7));
        if(IsOwner){
            RoomManager.Instance.SetPlayer(gameObject);
        }
    }

    public void AddMeToQueue(){
        AddToQueueServerRpc();
    }
    [ServerRpc]
    public void AddToQueueServerRpc(){
        RoomManager.Instance.AddMeToQueue(OwnerClientId);
    }

    private void Update() {
        if(!IsOwner) return;
        if(Input.touchCount > 0){
            AddLogServerRpc("\ntouched");
            DebugServerRpc();
        } 
    }

    [ServerRpc]
    void AddLogServerRpc(string s){
        RoomManager.Instance.AddLog(s);
    }

    [ServerRpc]
    void DebugServerRpc(){
        log.text += "\nplayer touched";
    }

    public void ChangeGameMode(){
        ChangeGameModeClientRpc();
    }

    [ClientRpc]
    void ChangeGameModeClientRpc(){
        RoomManager.Instance.ChangeGameMode();
    }

    public bool IsClientPlayer(){
        return IsClient?true:false;
    }
    public bool IsHostPlayer(){
        return IsHost?true:false;
    }
    public ulong OwnerClientIdPlayer(){
        return OwnerClientId;
    }
}

