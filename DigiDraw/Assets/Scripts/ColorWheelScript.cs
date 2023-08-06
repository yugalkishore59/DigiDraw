using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorWheelScript : MonoBehaviour{
    Texture2D texture; // The texture of the UI Image
    public Color32 output = new Color32(255,255,255,0);
    Image image;
    bool isActive = false;
    Touch touch;
    [SerializeField] GameObject Background;

    public void OnPointerClick(BaseEventData data){
        // to make this work, Turn on Read/write in advanced options of image texture source
        try{
            PointerEventData eventData = data as PointerEventData;
            image = GetComponent<Image>();
            if (image == null){
                Debug.LogError("No Image component found on the GameObject.");
                return;
            }
            texture = image.mainTexture as Texture2D;
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out Vector2 localCursor))
                return;       
            // Normalize localCursor to get UV coordinates (0 to 1)
            Vector2 normalizedCursor = new Vector2((localCursor.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
                                                (localCursor.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height);

            // Get the pixel position in the texture using the UV coordinates
            int x = Mathf.RoundToInt(normalizedCursor.x * texture.width);
            int y = Mathf.RoundToInt(normalizedCursor.y * texture.height);

            // Get the color from the clicked pixel
            output = texture.GetPixel(x, y);

            if(output.a != 0)
                GameHandler.Instance.SetCurrentColor(output);
        }catch(System.Exception ex){
            Debug.Log("\nError: " + ex.Message);
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
