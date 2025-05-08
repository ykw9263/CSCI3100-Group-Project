using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public Player()
    {
        entityID = 0 ;
        //public Army[] armies;
        //territories_in_Controls = new List<int>() ;
        territories_in_Controls.Add(entityID) ;
        
        GameState gameState = GameState.GetGameState() ;
        gameState.player = this ;
        //Debug.Log(gameState.player) ; 
        //Debug.Log(gamestate.territories.Count);
        //home = gamestate.territories[entityID];
        //Debug.Log(this);*/
    }
    private void Start()
    { 
        Debug.Log(GameState.GetGameState().player) ;
    }

}
