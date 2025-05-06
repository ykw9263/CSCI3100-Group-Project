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
        
        this.entities = new List<Entity>() ;
        //this.player = new Player() ;
        this.territories = new List<Territory>() ;
        //Debug.Log(this.player.entityID) ;
    }

    public static GameState GetGameState() 
    {
        return gamestate;
    }

    public void InitGame() {
        foreach (Entity entity in this.entities) {
            entity.home = territories[entity.entityID] ;
        }
    }
    public GameState ResetGameState()
    {
        this.entities = new List<Entity>();
        this.player = new Player();
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
        
        Debug.Log($" {territory.territoryID} : {territory.coordinates} ");
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
        //Debug.Log($" Add entity{entity.EntityID} : {territory.coordinates} ");
    }
}
