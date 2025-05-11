using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;

public class Enemy : Entity
{
    public Enemy(int entityID, Color color) : base (entityID, color)
    {
        GameState gameState = GameState.GetGameState() ;
    }
    private void Start()
    {
        Debug.Log(GameState.GetGameState().entities[1]);
    }

}
