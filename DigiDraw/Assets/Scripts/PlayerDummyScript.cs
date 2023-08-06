using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerDummyScript : NetworkBehaviour {

    private void Start() {
        if(IsOwner){
            RoomManager.Instance.SetPlayerScript(gameObject.GetComponent<PlayerDummyScript>());
        }
    }

    [ServerRpc]
    public void AddMeToListServerRpc(){
        RoomManager.Instance.AddMeToList(OwnerClientId);
    }

    [ClientRpc]
    public void ChangeGameModeClientRpc(){
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

    [ServerRpc]
    public void SetColorServerRpc(int i, int j, Color32 _color){
        SetColorClientRpc(i,j,_color);
    }

    [ClientRpc]
    private void SetColorClientRpc(int i, int j, Color32 _color){
        CustomGrid grid = PixelArtCanvasScript.Instance.GetGrid();
        grid.SetPixelColor(i,j,_color);
    }

    //old script
    /*TextMeshProUGUI log;
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
    }*/
}

