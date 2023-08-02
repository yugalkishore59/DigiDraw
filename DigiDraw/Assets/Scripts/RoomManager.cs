using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour{
    public static RoomManager Instance {get; private set;}
    [SerializeField] int mode=0; // 0 waiting, 1 guessing, 2 drawing

    public int currentTool = 0; // 0 pen, 1 eraser, 2 fill, 3 lens
    public Color32 currentColor = new Color32(255, 255, 255, 255);

    //will be needing in future
    [SerializeField] GameObject selected; // selected box image
    [SerializeField] Image currentColorImage; // image to show current color

    private void Awake() {
        Instance = this;
    }
}
