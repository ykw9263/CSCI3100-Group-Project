using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    private float waitTime = 2.0f;
    private float timer = 0.0f;
    private float visualTime = 0.0f;
    public int skillpoints ;
    public int hp ;
    public int atk ;
    public int speed ;
    public int minHp ;
    public int minAtk;
    public int minSpeed;
    public int maxHp ;
    public int maxAtk;
    public int maxSpeed;
    public int counter ;
    public bool start=true ;

    //public ArmySpawner enemy ;

    public int gold ;
    [SerializeField] TextMeshProUGUI textHp ;
    [SerializeField] TextMeshProUGUI textAtk ;
    [SerializeField] TextMeshProUGUI textSpeed ;
    [SerializeField] TextMeshProUGUI textSkillPoint;
    [SerializeField] TextMeshProUGUI textGold;
    [SerializeField] GameObject army; 

    // Start is called before the first frame update
    public void AddSp() 
    {
        skillpoints++ ;
            //Debug.Log(skillpoints) ;
    }
    public void MinusGold()
    {
        if (gold <= 0) {
            return;
        }
        gold--;
        textGold.SetText($"Gold: {gold}");
        //Debug.Log("Minus Gold");
    }
    public void AddHp() 
    {
        if (skillpoints < 1 || hp >= maxHp) {
            //Debug.Log("No SP");
            return ;
        }
        //Debug.Log("Add HP"); 
        skillpoints-- ;
        textSkillPoint.SetText($"SP: {skillpoints}");
        hp ++ ;
        if (hp == maxHp)
        {
            textHp.fontSize = 7;
            textHp.SetText("Max");
        }
        else {
            textHp.fontSize = 12;
            textHp.SetText($"{hp}"); 
        }
    }
    public void AddSpeed()
    {
        if (skillpoints < 1 || speed >= maxSpeed)
        {
            //Debug.Log("No SP");
            return;
        }
        //Debug.Log("Add Speed");
        skillpoints--;
        textSkillPoint.SetText($"SP: {skillpoints}");
        speed++;
        if(speed == maxSpeed) {
            textSpeed.fontSize = 7;
            textSpeed.SetText("Max");
        }
        else {
            textSpeed.fontSize = 12 ;
            textSpeed.SetText($"{speed}") ; 
        }
        
    }
    public void AddAtk()
    {
        if (skillpoints < 1 || atk >= maxAtk)
        {
            //Debug.Log("No SP");
            return;
        }
        //Debug.Log("Add ATK");
        skillpoints--;
        textSkillPoint.SetText($"SP: {skillpoints}");
        atk++;
        if (atk == maxAtk) {
            textAtk.fontSize = 7 ;
            textAtk.SetText("Max" );
        }
        else{
            textAtk.fontSize = 12 ;
            textAtk.SetText($"{atk}") ; 
        }
        
    }
    public void MinusHp()
    {
        if (hp == minHp)
        {
            return;
        }
        hp--;
        skillpoints++;
        textSkillPoint.SetText($"SP: {skillpoints}");
        textHp.fontSize = 12;
        textHp.SetText($"{hp}");
        //Debug.Log("Minus HP");
    }
    public void MinusSpeed()
    {
        if (speed == minSpeed)
        {
            return;
        }
        speed--;
        skillpoints++;
        textSkillPoint.SetText($"SP: {skillpoints}");
        textSpeed.fontSize = 12;
        textSpeed.SetText($"{speed}");
        //Debug.Log("Minus Speed");
    }
        
        
    public void MinusAtk()
    {
        if (atk == minAtk )
        {
            return;
        }
        atk--;
        skillpoints++;
        textSkillPoint.SetText($"SP: {skillpoints}");
        textAtk.fontSize = 12;
        textAtk.SetText($"{atk}");
        //Debug.Log("Minus ATK");
    }
    void Start()
    {
        skillpoints = 10 ;
        counter = 0; 
        minAtk = 1 ;
        minSpeed = 2 ;
        minHp = 3 ; 
        maxAtk = 99 ;
        maxSpeed = 99 ;
        maxHp = 99 ;
        hp = minHp ;
        atk = minAtk ;
        speed = minSpeed ;
        textAtk.SetText($"{atk}");
        textHp.SetText($"{hp}");
        textSpeed.SetText($"{speed}");
        textSkillPoint.SetText($"SP: {skillpoints}");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        // Check if we have reached beyond 2 seconds.
        // Subtracting two is more accurate over time than resetting to zero.
        if (timer > waitTime && this.start == true)
        {
            counter++ ;
            skillpoints++ ; 
            textSkillPoint.SetText($"SP: {skillpoints}");
            gold++;
            textGold.SetText($"Gold: {gold}");
            visualTime = timer ;
            if (counter >= 2) {
                GameState.GetGameState().EnemyMove();
                counter = 0 ;
            }
            
            // Remove the recorded 2 seconds.
            timer = timer - waitTime ;
        }
    }

    
}
