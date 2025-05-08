using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] GameObject gameMapPrefab;
    public Player player;
    public List<Entity> entities;
    public List<Territory> territories;

    private static GameState gamestate;
    MapGeneration gameMap;

    [SerializeField] List<float> enemyHues = new List<float>{ 0f, 0.044f , 0.088f, 0.177f, 0.35f, 0.700f, 0.777f, 0.888f};

    const int MAX_ENEMY_COUNT = 8;

    void Start()
    {
        gamestate = this;
        this.entities = new List<Entity>() ;
        this.territories = new List<Territory>() ;
        InitGame(2);
    }

    public static GameState GetGameState() 
    {
        return gamestate;
    }

    public void InitGame(int enemy_count) {
        Destroy(gameMap?.gameObject);
        GameObject gameMapObj = Instantiate<GameObject>(gameMapPrefab);
        gameMapObj.transform.parent = transform;

        gameMap = gameMapObj.GetComponent<MapGeneration>();

        gameMap.generateMap();

        if (enemy_count >= territories.Count || enemy_count > MAX_ENEMY_COUNT) {
            Debug.LogError("Enemy count too high! Abort");
            return;
        }

        float h = 210 / 360f, s = 0.7f, v = 0.9f;

        entities.Add(player = new Player(0, Color.HSVToRGB(h, s, v)));
        for (int i = 0; i < enemy_count; i++) {
            // h = UnityEngine.Random.Range(1f, 160f) / 360f;
            // entities.Add(new Enemy(i+1, Color.HSVToRGB(h, s, v)));
            entities.Add(new Enemy(i + 1, Color.HSVToRGB(enemyHues[i], s, v)));
            
        }

        List<Territory> shuffledterr = new List<Territory>(territories) ;
        for (int i = 0; i < shuffledterr.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffledterr.Count);
            var tmp = shuffledterr[i];
            shuffledterr[i] = shuffledterr[r];
            shuffledterr[r] = tmp;
        }
        int counter = 0;
        foreach (Entity entity in this.entities) {
            shuffledterr[counter].FallTo(entity);
            entity.home = shuffledterr[counter];
            counter++;
        }
    }
    public GameState ResetGameState()
    {
        this.entities = new List<Entity>();
        this.player = new Player(0, Color.HSVToRGB(210 / 360f, 0.7f, 0.9f));
        this.territories = new List<Territory>();
        return this;
    }
    public GameState ImportGameState(
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
        
        //Debug.Log($" {territory.territoryID} : {territory.coordinates} ");
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
        //Debug.Log($" Add entity{entity.EntityID} : {territory.coordinates} ");
    }
}
