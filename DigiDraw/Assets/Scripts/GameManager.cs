using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class GameManager : MonoBehaviour{
    public static GameManager Instance {get; private set;}

    private bool isSignedIn = false;
    public Image profileImage;

    [Header("lobby")]
    public bool isLobbyHost = true;
    public string LobbyTheme = "Regular";
    public int maxPlayers = 8;
    public bool isInLobby = false; //check when to make it true and false
    public Lobby hostLobby = null, clientLobby=null;
    float heartbeatTimer=0;

    private void Awake() {
         if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else Destroy(gameObject);

        FirebaseAndGPGS.Instance.InitializeFirebase();
        //logs+="isFirebaseReady = "+isFireBaseReady+"\n";
        //if(isFireBaseReady) SignIntoGPGS();
        FirebaseAndGPGS.Instance.SignIntoGPGS();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)){
            GoToPreviousScene();
        }

        if(isInLobby){
            LobbyHeartbeat();
        }
    }

    private async void LobbyHeartbeat(){
        if (hostLobby!=null) {
            heartbeatTimer-=Time.deltaTime;
            if(heartbeatTimer<0f){
                heartbeatTimer = 15;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    
    public void SetScene(string scene){
        SceneManager.LoadScene(scene);
    }

    void GoToPreviousScene(){
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;
        if (previousSceneIndex >= 0){
            SceneManager.LoadScene(previousSceneIndex);
        }else{
            QuitApplication();
        }
    }

    public void QuitApplication(){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
