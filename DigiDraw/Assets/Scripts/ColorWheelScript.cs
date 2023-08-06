using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//debug
using TMPro;

public class ColorWheelScript : MonoBehaviour{
    Texture2D texture; // The texture of the UI Image
    public Color32 output = new Color32(255,255,255,0);
    Image image;
    bool isActive = false;
    Touch touch;
    [SerializeField] GameObject Background;

    //debug
    public TextMeshProUGUI logs;

    public void OnPointerClick(BaseEventData data){
        logs.text += "\nclicked wheel";
        PointerEventData eventData = data as PointerEventData;
        image = GetComponent<Image>();
        if (image == null){
            Debug.LogError("No Image component found on the GameObject.");
            logs.text += "\nNo Image component found";
            return;
        }else{
            logs.text += "\nImage component found";
        }
        logs.text += "\nline 32";
        texture = image.mainTexture as Texture2D;
        logs.text += "\nline 34";
        RectTransform rectTransform = GetComponent<RectTransform>();
        logs.text += "\nline 36";
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out Vector2 localCursor)){
            logs.text += "\nno rect transform utility";
            return;
        }else{
            logs.text += "\nrect transform utility is good";
        }
            
        logs.text += "\nline 44";
        // Normalize localCursor to get UV coordinates (0 to 1)
        Vector2 normalizedCursor = new Vector2((localCursor.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
                                               (localCursor.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height);
        logs.text += "\nline 48";

       // Get the pixel position in the texture using the UV coordinates
        int x = Mathf.RoundToInt(normalizedCursor.x * texture.width);
        int y = Mathf.RoundToInt(normalizedCursor.y * texture.height);

        // Get the color from the clicked pixel
        output = texture.GetPixel(x, y);
        logs.text += "\nline 56";
        if(output.a != 0){
            GameHandler.Instance.SetCurrentColor(output);
            logs.text += "\ncolor set as "+output.ToString();
        }else{
            logs.text += "\ntransparent color";
        }
    }

    private void Update() {
        if(isActive && Input.touchCount > 0){
            touch = Input.GetTouch(0);
            if(touch.phase != TouchPhase.Ended) return;
            isActive = false;
            Background.SetActive(false);
        }
    }

    public void PickColor(){
        isActive = true;
        Background.SetActive(true);
    }
}
