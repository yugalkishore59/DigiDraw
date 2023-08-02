using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelArtCanvasScript : MonoBehaviour {
    CustomGrid canvasGrid;
    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] GameObject pixel;

    //will be needing in future
    Touch touch;
    bool isUITouched = false;
    public bool isClearedCanvas = true;

    void Start(){
        Vector3 origin = new Vector3(transform.position.x-7.5f,transform.position.y-7.5f,0);
        canvasGrid = new CustomGrid(width,height,pixel,origin);
    }
}
