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
    [SerializeField] SpriteRenderer renderObject;
    [SerializeField] GameObject curPathObject;
    [SerializeField] GameObject InfoObject;
    [SerializeField] GameObject newPathObject;

    private int counter; 
    public Entity owner;
    [SerializeField] public stats info;

    public int count;
    public Vector3 cur_pos, des_pos;
    public bool is_traveling = false;
    public bool finished_traveling = true;

    public List<Army> attackTarget;
    public bool isDead = false ;
    //protected bool selected = false;



    // Update is called once per frame
    protected void Update()
    {
        counter += 1;
        if (is_traveling)
        {
            travel();
        }

        if (counter % 30 == 0)
        {
            if (attackTarget.Count != 0)
            {
                is_traveling = false;
                Attack(attackTarget[0]);
            }
            else if (!is_traveling && !finished_traveling)
            {
                is_traveling = true;
            }
        }

    }

    protected void OnDestroy()
    {
        owner?.army.Remove(this);
    }

    public void travel(){
        
        //Debug.Log("traveling");
        //target = territory.transform.position;    
        //Debug.Log(des_pos);
        //transform.position = transform.position + Vector3.left * info.speed ;
        transform.position = Vector3.MoveTowards(transform.position, des_pos , info.speed*Time.deltaTime);
        cur_pos = transform.position ;
        if (Vector3.Distance(cur_pos, des_pos) <= 0.01){
            //Debug.Log("finish traveling") ;
            finished_traveling = true;
            is_traveling = false ; 
        }
    }
    public void SetDestination(Territory terr){
        des_pos = terr.coordinates ; 
        is_traveling = true ;
        finished_traveling = false;
    }
    public void SetDestination(Vector2 coordinates)
    {
        des_pos = new Vector3(coordinates.x, coordinates.y, 0);
        is_traveling = true;
        finished_traveling = false;
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
        if (!army || army.isDead) {
            RemoveAttackTarget(army);
        }
        //Debug.Log("Enemy take damage") ;
        army.TakeDamage(info.attack) ;
        
    }
    public void TakeDamage(int damage){
        Debug.Log($"{this.owner.entityID} : Taking Damage") ;
        info.hp -= damage ; 
        if(info.hp <= 0 && this)
        {
            isDead = true ;
            //owner.army.Remove(this) ;
            Destroy(this.gameObject) ; 
        } 
    }

    public void HandleSelect(bool select)
    {
        if (select) {
            InfoObject?.SetActive(true);
            curPathObject?.SetActive(true);
        }
        else
        {
            InfoObject?.SetActive(false);
            curPathObject?.SetActive(false);
        }
    }
/*
    protected void OnMouseDown(){
        //selected = true ; 
        //Debug.Log("selected");
        SetDestination(GameState.GetGameState().territories[0]);
        is_traveling = true;
    }
*/

    public void SetColor(Color color) {
        if (renderObject) renderObject.color = color;
    }
}
    
          
    
