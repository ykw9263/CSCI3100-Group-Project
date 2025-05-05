using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int entityID;
    //public Army[] armies;
    public List<int> territories_in_Controls = new () ;
    public List<Army> army = new () ;
    public Territory home ;  
    
    public Entity() {
        
    }
    
    public void SendArmy(Territory terr, Army troop){
        Debug.Log(troop);
        troop.des_pos = terr.coordinates ; 
        troop.is_traveling = true ;
    }

    public void RetreatArmy(Army troop){
        Debug.Log(troop);
        troop.des_pos = home.coordinates ;
    }
    public void AddArmy(Army troop){
        Debug.Log(troop);
        this.army.Add(troop) ;
        Debug.Log(this.army[0]);
        //troop.owner = this ;
        //troop.isDead = false ;
        //troop.cur_pos = home.coordinates ; 
    }
    
}
