using System.Collections;
using System.Collections.Generic;
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
    public Entity Owner;
    public stats info;
    public int count;
    protected bool is_traveling = false;
    public Vector2 cur_pos , des_pos ;
    public List<Army> attackTarget;
    public bool isDead = false ;  
    //protected bool selected = false;

    protected void travel(){
        
        //Debug.Log("traveling");
        //target = territory.transform.position;    
        //Debug.Log(des_pos);
        //transform.position = transform.position + Vector3.left * info.speed ;
        transform.position = Vector2.MoveTowards(transform.position, des_pos , info.speed*Time.deltaTime);
        cur_pos = transform.position ;
        if (Vector2.Distance(cur_pos, des_pos) < 0.01 ){
            is_traveling = false ; 
        }
    }
    protected void SetDestination(Territory terr){
        des_pos = terr.coordinates ; 
    }

    
    public void AddAttackTarget(Army army){
        Debug.Log("Add Target");
        attackTarget.Add(army) ;
    }

    public void RemoveAttackTarget(Army army){
        Debug.Log("Remove Target");
        attackTarget.Remove(army) ;
    }
    public void Attack(Army army) {
        if (army.isDead) {
            RemoveAttackTarget(army);
        }
        Debug.Log("Army take damage") ;
        army.TakeDamage(info.attack) ;
        
    }
    public void TakeDamage(int damage){
        Debug.Log("Taking Damage") ;
        info.hp -= damage ; 
        if(info.hp <= 0){
            isDead = true ;
            Destroy(this.gameObject) ; 
        } 
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
    
          
    
