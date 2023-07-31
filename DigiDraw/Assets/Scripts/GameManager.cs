using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    
    public void SetScene(string scene){
        SceneManager.LoadScene(scene);
    }

}
