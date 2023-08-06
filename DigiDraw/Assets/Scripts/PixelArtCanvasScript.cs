using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PixelArtCanvasScript : MonoBehaviour {
    public static PixelArtCanvasScript Instance {get; private set;}
    CustomGrid customGrid;
    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] GameObject pixel;

    //will be needing in future
    Touch touch;
    bool isUITouched = false;
    public bool isClearedCanvas = true;

    private void Awake() {
        Instance=this;
    }

    void Start(){
        Vector3 origin = new Vector3(transform.position.x-7.5f,transform.position.y-7.5f,0);
        customGrid = new CustomGrid(width,height,pixel,origin);
    }

    private void Update() {
        if(GameHandler.Instance.gameMode == 2){
            EnableDrawing();
        }
    }

    private void EnableDrawing(){
        if (Input.touchCount > 0) {
            touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began){
                isUITouched = false;
            }
            if(EventSystem.current.IsPointerOverGameObject(touch.fingerId) || isUITouched){
                if(touch.phase == TouchPhase.Began){
                    isUITouched = true; //touched ui
                }
                Debug.Log("Clicked on the UI");
                return;
            }
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            Collider2D collider = Physics2D.OverlapPoint(touchPosition);

            if (collider != null && collider.gameObject.tag == "Pixel"){
                PixelScript pixelScript = collider.gameObject.GetComponent<PixelScript>();
                PlayerDummyScript playerScript = RoomManager.Instance.GetPlayerDummyScript();
                switch(GameHandler.Instance.currentTool){
                    case 0 : // pen
                        if(pixelScript.pixelColor.Equals(GameHandler.Instance.currentColor)) break;
                        if(touch.phase == TouchPhase.Began) customGrid.SaveColors();
                        
                        pixelScript.SetColor(GameHandler.Instance.currentColor); //setting own color
                        //syncing color with other
                        playerScript.SetColorServerRpc(pixelScript.yIndex,pixelScript.xIndex,GameHandler.Instance.currentColor);
                        break;
                    case 1 : // eraser
                        if(isClearedCanvas) break;
                        if(touch.phase == TouchPhase.Began) customGrid.SaveColors();

                        pixelScript.SetColor(new Color32(255, 255, 255, 0)); //setting own color
                        //syncing color with other
                        playerScript.SetColorServerRpc(pixelScript.yIndex,pixelScript.xIndex,new Color32(255, 255, 255, 0));
                        break;
                    case 2 : // fill
                        if(touch.phase == TouchPhase.Ended)
                            if(pixelScript.pixelColor.Equals(GameHandler.Instance.currentColor)) break;
                            if(touch.phase == TouchPhase.Began) customGrid.SaveColors();
                            customGrid.Fill(pixelScript.xIndex,pixelScript.yIndex,GameHandler.Instance.currentColor,pixelScript.pixelColor);
                        break;
                    case 3 : // this case is handeled in CameraControl script sitting on main camera  
                        break;
                }
            }
            if(touch.phase == TouchPhase.Ended){
                isClearedCanvas = customGrid.isClearedCanvas();
            }
        }
    }

    public void ClearCanvas(){
        if(!isClearedCanvas) customGrid.SaveColors();
        customGrid.ClearCanvas();
        isClearedCanvas = true;
    }

    public void SavePNG(){
        byte[] bytes = customGrid.SavePNG();
        //TODO : add android plugin
        //AndroidPlugin.Instance.SavePNG(bytes);
    }

    public void Undo(){
        customGrid.Undo();
        isClearedCanvas = customGrid.isClearedCanvas();
    }

    public CustomGrid GetGrid(){
        return customGrid;
    }

}
