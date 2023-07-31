using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
using TMPro;



public class FirebaseAndGPGS : MonoBehaviour{
    public static FirebaseAndGPGS Instance {get; private set;}

    public TextMeshProUGUI logsTxt;
    string logs;

    [Header("Firebase")]
    FirebaseApp firebaseApp;
    FirebaseAuth firebaseAuth;
    FirebaseUser firebaseUser;
    [SerializeField] bool isFireBaseReady = false;


    [Header("GPGS")]
    [SerializeField] bool isGooglePlaySignedIn = false;
    string gpgsServerAuthCode;

    [Header("Game")]
    public string userName="Guest";
    public string userId="0";
    public int sketches=0;
    public int rank=0;

    private void Awake() {
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else Destroy(gameObject);

        PlayGamesPlatform.Activate();
        InitializeFirebase();
        //logs+="isFirebaseReady = "+isFireBaseReady+"\n";
        //if(isFireBaseReady) SignIntoGPGS();
        SignIntoGPGS();
    }

    private void Update() {
        logsTxt.text = logs;
    }

    void InitializeFirebase(){
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>{
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available){
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                firebaseApp = Firebase.FirebaseApp.DefaultInstance;
                firebaseAuth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                isFireBaseReady = true;
            }else{
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                logs+= "Could not resolve all Firebase dependencies: "+dependencyStatus+"\n";
            }
        });
    }
    public void SignIntoGPGS(){
        if(!isGooglePlaySignedIn){
            PlayGamesPlatform.Instance.Authenticate((success) =>{
                if (success == SignInStatus.Success){
                    Debug.Log("Login with Google Play games successful.");
                    logs+="Login with Google Play games successful.\n";

                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>{
                        gpgsServerAuthCode = code;
                        if(gpgsServerAuthCode == "" || gpgsServerAuthCode == null){
                            logs+="Could not get auth code from play games.\n";
                            return;
                        }
                        AuthenticateFirebase(gpgsServerAuthCode);
                        isGooglePlaySignedIn = true;
                    });
                }else{
                    Debug.Log("Login Unsuccessful");
                    logs+="Login Unsuccessful.\n";
                }
            });
        }else{
            Debug.Log("Already Signed into Google Play Games");
            logs+="Already Signed into Google Play Games.\n";
        }
    }
    void AuthenticateFirebase(string authCode){
        Credential credential = Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
        firebaseAuth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
        if (task.IsCanceled) {
            Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
            logs+="SignInAndRetrieveDataWithCredentialAsync was canceled\n";
            return;
        }
        if (task.IsFaulted) {
            Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
            logs+="SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception+"\n";
            return;
        }

        AuthResult result = task.Result;
        Debug.LogFormat("User signed in successfully: {0}", result.User.DisplayName);
            logs+="User signed in successfully: "+ result.User.DisplayName+"\n";
        });
        firebaseUser = firebaseAuth.CurrentUser;
        if (firebaseUser != null && firebaseUser.IsValid()) {
        userName = firebaseUser.DisplayName;

        // The user's Id, unique to the Firebase project.
        // Do NOT use this value to authenticate with your backend server, if you
        // have one; use User.TokenAsync() instead.
        userId = firebaseUser.UserId;
        }
    }
}
