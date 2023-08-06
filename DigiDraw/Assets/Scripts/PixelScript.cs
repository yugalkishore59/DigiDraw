using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelScript : MonoBehaviour{
    public Color32 pixelColor = new Color32(255, 255, 255, 0); //Color32(byte r, byte g, byte b, byte a); this is overrided by editor color, so initialize in editor
    SpriteRenderer spriteRenderer;
    public int yIndex=0; //height
    public int xIndex=0; //width

    private void Start() {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = pixelColor;
    }

    public void SetColor(Color32 _color){
        //TODO : Sync color by server rpc
        pixelColor = _color;
        spriteRenderer.color = pixelColor;
    }
}
