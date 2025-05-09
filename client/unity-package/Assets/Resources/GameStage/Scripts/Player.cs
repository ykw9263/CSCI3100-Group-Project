using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public Skill skill ;
    public Player(int entityID, Color color) : base(entityID, color)
    {
        GameState gameState = GameState.GetGameState() ;
        // gameState.player = this ;
    }
    private void Start()
    { 
        Debug.Log(GameState.GetGameState().player) ;
    }

}
