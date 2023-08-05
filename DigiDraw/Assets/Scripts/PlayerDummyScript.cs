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
        RoomManager.Instance.AddMeToQueueServerRpc(OwnerClientId);
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
        log.text += "tryed in roommanager";
    }

    [ServerRpc]
    void DebugServerRpc(){
        log.text += "\nplayer touched";
    }
}

