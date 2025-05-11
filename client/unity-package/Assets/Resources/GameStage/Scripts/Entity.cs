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
        //Debug.Log(troop);
        troop.des_pos = terr.coordinates ; 
        troop.is_traveling = true ;
    }

    public void RetreatArmy(Army troop){
        //Debug.Log(troop);
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

    public virtual void TakeControl(Territory terr) {
        territories_in_Controls.Add(terr);
    }
    public virtual void FullyTakeControl(Territory terr)
    {
        
    }
    public void LoseControl(Territory terr)
    {
        
        territories_in_Controls.Remove(terr);
        if (terr == home) 
        {
            //Debug.Log($"Enemy {this.entityID} falls");
            int tempArmyCount = army.Count;
            for (int i = 0; i < tempArmyCount; i++) {
                Army tempArmy = army[0];
                tempArmy.OwnerFall();
                army.Remove(tempArmy);
            }
            Debug.Log(army.Count);

            int tempTerrCount = territories_in_Controls.Count;
            for (int i = 0; i < tempTerrCount; i++)
            {
                Territory teempTerr = territories_in_Controls[0];
                teempTerr.FallTo(-1);
            }
            Debug.Log(territories_in_Controls.Count);

            GameState.GetGameState().RemoveEntities(this); 
        }  
        
    }
    

}
