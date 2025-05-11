using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using TMPro ;

public class Achievements : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finishTime ;
    [SerializeField] TextMeshProUGUI playCount ;
    [SerializeField] TextMeshProUGUI maxHP ;
    [SerializeField] TextMeshProUGUI maxSpeed ;
    [SerializeField] TextMeshProUGUI maxAtk;
    // Start is called before the first frame update
    void Start()
    {
        UserData.GameStat gameStat = UserData.GetGameStat();
        playCount.SetText( $"Play Count : {gameStat.playcount.ToString()}" ) ;
        string finishTimeStr = (gameStat.fastestEndTime> 1e9)? "N/A": TimeSpan.FromMilliseconds(gameStat.fastestEndTime).ToString("mm':'ss'.'ff");
        finishTime.SetText($"Finish Time : {finishTimeStr}" ) ;
        maxHP.SetText($"Max HP : {gameStat.maxHp.ToString()}" ) ;
        maxSpeed.SetText($"Max Speed : {gameStat.maxSpeed.ToString()}") ;
        maxAtk.SetText($"Max ATK : {gameStat.maxAtk.ToString()}") ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
