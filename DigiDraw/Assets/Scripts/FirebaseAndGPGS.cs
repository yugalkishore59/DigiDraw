using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
using TMPro;


public class FirebaseAndGPGS : MonoBehaviour{

    [SerializeField] TextMeshProUGUI txt;
    string logs;

    private void Start() {
        InitializeFirebase();
        InitializeGPGS();
    }
    private void Update() {
        txt.text = logs;
    }
    void InitializeFirebase(){
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>{
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        });
    }
    void InitializeGPGS(){
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate((success) =>{
            if (success == SignInStatus.Success){
                Debug.Log("Login with Google Play games successful.");
                logs+="Login with Google Play games successful, ";
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    if(code == "" || code == null){
                        logs+="did not get code from play games, ";
                        return;
                    }
                    Debug.Log("Authorization code: " + code);
                    logs+="Authorization code: " + code+", ";
                    string Token = code;
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                    AuthenticateFirebase(Token);
                });
            }
            else
            {
                //Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
                logs+="Login Unsuccessful, ";
            }
        });
    }
    void AuthenticateFirebase(string authCode){
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Firebase.Auth.Credential credential = Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
        if (task.IsCanceled) {
            Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
            logs+="SignInAndRetrieveDataWithCredentialAsync was canceled, ";
            return;
        }
        if (task.IsFaulted) {
            Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
            logs+="SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception+", ";
            return;
        }

        Firebase.Auth.AuthResult result = task.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})",
            result.User.DisplayName, result.User.UserId);
            logs+="User signed in successfully: "+ result.User.DisplayName+" "+ result.User.UserId+", ";
        });
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null && user.IsValid()) {
        string playerName = user.DisplayName;

        // The user's Id, unique to the Firebase project.
        // Do NOT use this value to authenticate with your backend server, if you
        // have one; use User.TokenAsync() instead.
        string uid = user.UserId;
        }
    }
}
