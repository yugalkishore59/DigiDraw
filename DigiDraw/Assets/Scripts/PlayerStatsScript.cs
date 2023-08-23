using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStatsScript : MonoBehaviour{
    [SerializeField] TextMeshProUGUI rankTxt;
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] TextMeshProUGUI scoreTxt;

    public string playerRank;
    public string playerName;
    public string playerScore;

    public void SetStats(string _rank, string _name, string _score){
        playerRank = rankTxt.text = _rank;
        playerName = nameTxt.text = _name;
        playerScore = scoreTxt.text = _score;
    }
}
