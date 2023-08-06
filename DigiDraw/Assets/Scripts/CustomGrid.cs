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

    public void SaveColors(){
        for(int i=0;i<height;i++){
            for(int j=0;j<width;j++){
                pixelScript = pixelArray[i, j].GetComponent<PixelScript>();
                colorArray[i,j] = pixelScript.pixelColor;
            }
        }
        Debug.Log("color saved");
    }

    bool[,] visited;
    public void Fill(int x, int y, Color32 fillColor, Color32 defaultColor){
        visited = new bool[width,height];
        FloodFill(x,y,fillColor,defaultColor);
    }

    void FloodFill(int x, int y, Color32 fillColor, Color32 defaultColor){
        if(x<width && y<height && x>=0 && y>=0){
            if(visited[y,x]) return;
            if(pixelArray[y,x].GetComponent<PixelScript>().pixelColor.Equals(defaultColor)){
                //TODO : Sync color by server rpc
                //pixelArray[y,x].GetComponent<PixelScript>().SetColor(fillColor);
                RoomManager.Instance.GetPlayerDummyScript().SetColorServerRpc(y,x,fillColor);
                visited[y,x] = true;
                FloodFill(x+1,y,fillColor,defaultColor);
                FloodFill(x-1,y,fillColor,defaultColor);
                FloodFill(x,y+1,fillColor,defaultColor);
                FloodFill(x,y-1,fillColor,defaultColor);
            }
        }
    }

    public bool isClearedCanvas(){
        for(int i=0;i<height;i++){
            for(int j=0;j<width;j++){
                pixelScript = pixelArray[i, j].GetComponent<PixelScript>();
                if(pixelScript.pixelColor.a != 0) return false;
            }
        }
        return true;
    }

    public void ClearCanvas(){
        for(int i=0;i<height;i++){
            for(int j=0;j<width;j++){
                //TODO : Sync color by server rpc
                //pixelArray[i,j].GetComponent<PixelScript>().SetColor(new Color32(255,255,255,0));
                RoomManager.Instance.GetPlayerDummyScript().SetColorServerRpc(i,j,new Color32(255,255,255,0));
            }
        }
    }

    public byte[] SavePNG(){
        Texture2D texture2D = new Texture2D(width,height,TextureFormat.RGBA32,false);
        for(int i=0;i<height;i++){
            for(int j=0;j<width;j++){
                Color32 pixelColor = pixelArray[i,j].GetComponent<PixelScript>().pixelColor;
                texture2D.SetPixel(j,i,pixelColor);
            }
        }
        texture2D.Apply();
        byte[] bytes = texture2D.EncodeToPNG();
        return bytes;
    }

    public void Undo(){
        for(int i=0;i<height;i++){
            for(int j=0;j<width;j++){
                //pixelScript = pixelArray[i, j].GetComponent<PixelScript>();
                //TODO : Sync color by server rpc
                //pixelScript.SetColor(colorArray[i,j]);
                RoomManager.Instance.GetPlayerDummyScript().SetColorServerRpc(i,j,colorArray[i,j]);
            }
        }
    }

    public void SetPixelColor(int i, int j, Color32 _color){
        pixelArray[j,i].GetComponent<PixelScript>().SetColor(_color);
    }

}
