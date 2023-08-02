using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid {
    private int height, width;
    Vector3 origin;
    GameObject pixel;
    GameObject[,] pixelArray;
    public Color32[,] colorArray;
    PixelScript pixelScript;

    public CustomGrid(int _height, int _width, GameObject _pixel, Vector3 _origin){
        height = _height;
        width = _width;
        pixel = _pixel;
        origin = _origin;
        pixelArray = new GameObject[width,height];
        colorArray = new Color32[width,height];
        CreateGrid();
    }

    private void CreateGrid(){
        for(int i=0;i<height;i++){
            for(int j=0;j<width;j++){
                Vector3 pos = new Vector3(origin.x + j,origin.y +i ,0);
                //need to instantiate on network
                pixelArray[i,j]= GameObject.Instantiate(pixel,pos,Quaternion.identity);
                pixelScript = pixelArray[i, j].GetComponent<PixelScript>();
                colorArray[i,j] = new Color32(255, 255, 255, 0);
                if(pixelScript != null) {
                    pixelScript.xIndex = j;
                    pixelScript.yIndex = i;
                }
                else {
                    Debug.LogError("TileScript component not found on the prefab.");
                }
            }
        }
    }
}
