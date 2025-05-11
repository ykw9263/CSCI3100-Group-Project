using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerManagement : MonoBehaviour
{
    private Text textTimer   ;
    public static TimerManagement instance ;
    public float timer = 0.0f ;
    private bool isTimer; 

    void Awake()
    {
        instance = this ;
    }
    void Start()
    {
        textTimer.text = "00:00" ;
            isTimer = false;
    }
    void Update() 
    {
        if (isTimer) 
        {
            timer += Time.deltaTime;
            DisplayTime() ;
        }
    }

    void DisplayTime() 
    { 
        int minutes = Mathf.FloorToInt(timer / 60) ;
        int seconds = Mathf.FloorToInt(timer - minutes * 60) ;
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds) ;
    }
       

    public void BeginTimer()
    {
        instance = this ;
        isTimer = true ; 
    }

    public void EndTimer()
    {
        isTimer = false ;
    }

    
}
