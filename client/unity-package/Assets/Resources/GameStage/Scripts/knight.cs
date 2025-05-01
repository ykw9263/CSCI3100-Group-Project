using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class knight : MonoBehaviour
{
    // Start is called before the first frame update
    public struct stats {
        public int hp ;
        public int attack ;
        public int speed ;
        
    }   
    public Entity Owner;
    public stats info;
    public int count;
    private bool is_traveling = false;
    //private bool selected = false;
    void Start()
    {
        count = 5;
        info.hp = 5;
        info.attack = 1;
        info.speed = 2;
    }

    void travel(){
        Debug.Log("traveling");
        //target = territory.transform.position;
        Vector2 target = GameState.GetGameState().territories[0].coordinates;
        Debug.Log(target);
        //transform.position = transform.position + Vector3.left * info.speed ;

        transform.position = Vector2.MoveTowards(transform.position, target , info.speed*Time.deltaTime);
    }
    
    void attack() {
        Debug.Log("attacking");
        //enemy.info.hp -= info.attack;
    }
    
    // Update is called once per frame
    void Update()
    {   
        if(is_traveling){
            travel();
        }
    }
    void OnMouseDown(){
        //selected = true ; 
        Debug.Log("selected");
        is_traveling = true;
        Update();
    }
    void options(){
        Debug.Log("options");
        
    }
}
