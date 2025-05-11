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

    public override void TakeControl(Territory terr)
    {
        territories_in_Controls.Add(terr);
    }
    public override void FullyTakeControl(Territory terr)
    {
        this.Conquer();
    }

    public void Conquer() {
        foreach (Army troop in this.army) {
            int random = UnityEngine.Random.Range(0, GameState.GetGameState().territories.Count);
            troop.SetDestination(GameState.GetGameState().territories[random]);
        }
        
    }
}
