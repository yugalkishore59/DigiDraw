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
