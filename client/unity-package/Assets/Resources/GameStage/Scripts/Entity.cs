using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Entity
{
    public int entityID;
    //public Army[] armies;
    public List<Territory> territories_in_Controls = new () ;
    public List<Army> army = new () ;
    public Territory home ;

    public int money = 0;
    public Color color;

    public Entity(int entityID, Color color) {
        this.entityID = entityID;
        this.color = color;
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
        //Debug.Log($"1: {troop.ownerID}");
        //Debug.Log(this.army[0]);
        troop.ownerID = this.entityID ; 
        this.army.Add(troop);
        //Debug.Log($"2: {troop.ownerID}");
        //troop.isDead = false ;
        //troop.cur_pos = home.coordinates ; 
    }

    public void TakeControl(Territory terr) {
        territories_in_Controls.Add(terr);
    }
    public void LoseControl(Territory terr)
    {
        
        territories_in_Controls.Remove(terr);
        if (terr == home) 
        {    
            Debug.Log($"Enemy {this.entityID} falls");
            foreach (Territory territory in this.territories_in_Controls) {
                territory.FallTo(-1);
            }
            foreach (Army troops in this.army) {
                troops.OwnerFall();
            }
            GameState.GetGameState().RemoveEntities(this); 
        }  
        
    }
    

}
