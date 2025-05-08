using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;


public struct stats {
        public int id ;  
        public int hp ;
        public int attack ;
        public int speed ;
}   
public abstract class Army : MonoBehaviour
{
    private int counter; 
    public Entity owner;
    protected stats info;
    public int count;
    public bool is_traveling = false;
    public Vector3 cur_pos , des_pos ;
    public List<Army> attackTarget;
    public bool isDead = false ;  
    //protected bool selected = false;

    public void travel(){
        
        //Debug.Log("traveling");
        //target = territory.transform.position;    
        //Debug.Log(des_pos);
        //transform.position = transform.position + Vector3.left * info.speed ;
        transform.position = Vector3.MoveTowards(transform.position, des_pos , (info.speed+9)*Time.deltaTime);
        cur_pos = transform.position ;
        if (Vector3.Distance(cur_pos, des_pos) <= 0.01 ){
            //Debug.Log("finish traveling") ;
            is_traveling = false ; 
        }
    }
    public void setStats(int hp, int atk , int speed) {
        Debug.Log("SetStats");
        info.hp = hp;
        info.attack = atk;
        info.speed = speed ;
    }
    public void SetDestination(Territory terr){
        des_pos = terr.coordinates ; 
        is_traveling=true ;
    }

    
    public void AddAttackTarget(Army army){
        if (army.owner.entityID == this.owner.entityID)
        {
            return ;
        }
        //Debug.Log("Add Target");
        attackTarget.Add(army) ;
    }

    public void RemoveAttackTarget(Army army){
        //Debug.Log("Remove Target");
        attackTarget.Remove(army) ;
    }
    public void Attack(Army army) {
        if (army.isDead) {
            RemoveAttackTarget(army);
        }
        //Debug.Log("Enemy take damage") ;
        army.TakeDamage(info.attack) ;
        
    }
    public void TakeDamage(int damage){
        Debug.Log($"{this.owner.entityID} : Taking Damage") ;
        info.hp -= damage ; 
        if(info.hp <= 0){
            isDead = true ;
            //owner.army.Remove(this) ;
            Destroy(this.gameObject) ; 
        } 
    }
    protected void OnDestroy()
    {       
            owner?.army.Remove(this) ;  
    }
    // Update is called once per frame
    protected void Update()
    {   
        counter += 1 ;
        if(is_traveling){
            travel() ;  
        }
        if(counter % 30 == 0) {
            if (attackTarget.Count != 0)
            {
                Attack(attackTarget[0]) ;
            }
        }

    }
    protected void OnMouseDown(){
        //selected = true ; 
        //Debug.Log("selected");
        SetDestination(GameState.GetGameState().territories[0]);
        is_traveling = true;
    }    
}
    
          
    
