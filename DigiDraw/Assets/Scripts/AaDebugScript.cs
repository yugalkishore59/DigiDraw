using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AaDebugScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI indextxt;
    [SerializeField] private TextMeshProUGUI artisttxt;
    [SerializeField] private TextMeshProUGUI wordtxt;
    [SerializeField] private TextMeshProUGUI array;
    [SerializeField] private TextMeshProUGUI myid;


    void Start()
    {

    }

    void Update()
    {
        indextxt.text = RoomManager.Instance.currentArtistIndex.ToString();
        artisttxt.text = RoomManager.Instance.currentArtist.Value.ToString();
        wordtxt.text = RoomManager.Instance.currentWord.Value.ToString();
        if(RoomManager.Instance.GetPlayerDummyScript()!=null)
            myid.text = RoomManager.Instance.GetPlayerDummyScript().OwnerClientIdPlayer().ToString();
        setArray();
    }

    void setArray(){
        array.text="";
        foreach (ulong i in RoomManager.Instance.clientIdList){
            array.text+= i.ToString() + " ";
        }
    }
}
