using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public Enemy()
    {
        entityID = 1 ;
        //public Army[] armies;
        //territories_in_Controls = new List<int>();
        territories_in_Controls.Add(entityID) ;
        GameState gameState = GameState.GetGameState() ;
        gameState.entities.Add(this) ;
        //Debug.Log(gameState.player);
        //home = GameState.GetGameState().territories[entityID] ; 
        //Debug.Log(this);
    }
    private void Start()
    {
        Debug.Log(GameState.GetGameState().entities[1]);
    }

}
