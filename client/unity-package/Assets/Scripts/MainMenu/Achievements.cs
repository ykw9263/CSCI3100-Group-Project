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
        playCount.SetText( $"Play Count : {UserData.gameStat.playCount.ToString()}" ) ;
        finishTime.SetText($"Finish Time : {UserData.gameStat.fastestEndTime.ToString("mm':'ss")}" ) ;
        maxHP.SetText($"Max HP : {UserData.gameStat.maxHp.ToString()}" ) ;
        maxSpeed.SetText($"Max Speed : {UserData.gameStat.maxSpeed.ToString()}") ;
        maxAtk.SetText($"Max ATK : {UserData.gameStat.maxAtk.ToString()}") ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
