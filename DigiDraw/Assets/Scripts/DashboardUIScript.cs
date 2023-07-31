using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class DashboardUIScript : MonoBehaviour
{
    [SerializeField] GameObject avatar;
    [SerializeField] TextMeshProUGUI welcomeTxt,rankTxt,sketchesTxt;
    Image image;


    private void Awake() {
        image = avatar.GetComponent<Image>();

        welcomeTxt.text = "Welcome "+FirebaseAndGPGS.Instance.userName;

        string url = FirebaseAndGPGS.Instance.avatarUrl;
        StartCoroutine(GetTexture(url));
    }

    IEnumerator GetTexture(string url) {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            image.overrideSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    public void ShowLeaderboard(){
        FirebaseAndGPGS.Instance.ShowLeaderboard();
    }
}
