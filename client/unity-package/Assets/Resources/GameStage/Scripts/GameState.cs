using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameState
{
    public Player player;
    public List<Entity> entities;
    public List<Territory> territories;
    private static readonly GameState gamestate = new();

    GameState()
    {
        this.player = new Player();
        this.entities = new List<Entity>();
        this.territories = new List<Territory>();
    }

    public static GameState GetGameState() 
    {
        return gamestate;
    }

    public GameState ResetGameState()
    {
        this.player = new Player();
        this.entities = new List<Entity>();
        this.territories = new List<Territory>();
        return this;
    }
    public  GameState ImportGameState(
        Player player, List<Entity> entities, List<Territory> territories
        )
    {
        this.player = player;
        this.entities = entities;
        this.territories = territories;
        return this;
    }

    public void AddTerritory(Territory territory) 
    {
        territories.Add(territory);
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }
}
