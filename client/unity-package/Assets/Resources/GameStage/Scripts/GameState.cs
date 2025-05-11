using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

//using System.Diagnostics;

public class GameState : MonoBehaviour
{
    [SerializeField] GameObject gameMapPrefab;
    [SerializeField] GameObject skillPrefab;
    [SerializeField] GameObject PopUpParent;
    [SerializeField] TextMeshProUGUI time ;
    public Player player;
    public Dictionary<int, Entity> entities;
    public Dictionary<int, Territory> territories;
    private float waitTime = 2.0f;
    private float timer = 0.0f;
    private float visualTime = 0.0f;
    //[SerializeField] Text time;
    private float elapsedTime;
    private TimeSpan timePlaying  ; 
    //private DateTime startTime;
    //private DateTime endTime ;
    private bool timerGoing ;
    public EndGamePanel panel ;
    public ArmySpawner enemySpawner;

    private static GameState gamestate;
    MapGeneration gameMap;

    [SerializeField] List<float> enemyHues = new List<float>{ 0f, 0.044f , 0.088f, 0.177f, 0.35f, 0.700f, 0.777f, 0.888f};

    const int MAX_ENEMY_COUNT = 8;
    public int enemyCount = 2;
    public int seed = 0;

    void Start()
    {
        gamestate = this;
        this.entities = new Dictionary<int, Entity>() ;
        this.territories = new Dictionary<int, Territory>() ;
        
        InitGame(UserData.gameSetting.enemyCount);
    }
    void Update()
    {
        if (timerGoing)
        {
            this.elapsedTime += Time.deltaTime;
            this.timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingStr = "Time: " + timePlaying.ToString("mm':'ss");
            time.SetText(timePlayingStr) ;
        }
    }

    public static GameState GetGameState() 
    {
        return gamestate;
    }

    public void InitGame(int enemy_count) {
        if (seed != 0)
            UnityEngine.Random.InitState(seed);
        Debug.Log(seed);

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

        entities.Add(0, player = new Player(0, Color.HSVToRGB(h, s, v))) ;
        GameObject skillObj = Instantiate<GameObject>(skillPrefab) ;
        player.skill = skillObj.GetComponent<Skill>() ;
        skillObj.transform.SetParent(PopUpParent.transform, false);
        //skillObj.transform.localPosition = new Vector3(-50, -115, 0);

        for (int i = 0; i < enemy_count; i++) {
            // h = UnityEngine.Random.Range(1f, 160f) / 360f;
            // entities.Add(new Enemy(i+1, Color.HSVToRGB(h, s, v)));
            entities.Add(i+1, new Enemy(i + 1, Color.HSVToRGB(enemyHues[i], s, v)));
            
        }

        List<Territory> shuffledterr = new List<Territory>(territories.Values) ;
        for (int i = 0; i < shuffledterr.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffledterr.Count);
            var tmp = shuffledterr[i];
            shuffledterr[i] = shuffledterr[r];
            shuffledterr[r] = tmp;
        }
        int counter = 0;
        foreach (Entity entity in this.entities.Values) {
            shuffledterr[counter].FallTo(entity.entityID);
            shuffledterr[counter].Repair(99999);
            entity.home = shuffledterr[counter];
            counter++;
        }
        this.timerGoing = true;
        this.elapsedTime = 0.0f;
        this.timePlaying = new TimeSpan();
    }

    public void EndGame() {
        this.timerGoing = false;
        player.skill.start = false ;
        this.timePlaying = TimeSpan.FromSeconds(elapsedTime);
        string timeText = "Conquer the World in " + timePlaying.ToString("mm':'ss'.'ff");
        
        UserData.SetGameStat(timePlaying.TotalMilliseconds, player.skill.maxHp, player.skill.maxSpeed, player.skill.maxAtk);
        UserData.IncrementPlayCount();
        panel.EndGame(timeText);    
        //time.SetText("You Win!") ; 
        Debug.Log("You Win !") ;
    }

    public void EnemyMove() {
        foreach(var ent in entities)
        {
            if (ent.Key == 0) continue;
            enemySpawner.spawnEnemy(ent.Key);

        }
    }

    public GameState ResetGameState()
    {
        this.entities = new Dictionary<int, Entity>();
        this.player = new Player(0, Color.HSVToRGB(210 / 360f, 0.7f, 0.9f));
        this.territories = new Dictionary<int, Territory>();
        return this;
    }
    public GameState ImportGameState(
        Player player, Dictionary<int, Entity> entities, Dictionary<int, Territory> territories
        )
    {
        this.player = player;
        this.entities = entities;
        this.territories = territories;
        return this;
    }

    public void AddTerritory(Territory territory) 
    {
        territories.Add(territory.territoryID, territory);
        
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity.entityID, entity);
    }

    public void RemoveEntities(Entity entity) {
        entities.Remove(entity.entityID) ;
        if (this.entities.Count == 1) {
            Debug.Log(this.entities.Count);
            EndGame(); 
        }    
    }

    public Territory GetTerrByID(int terrID)
    {
        return territories.GetValueOrDefault(terrID, null);
    }
    public Entity GetEntityByID(int entityID)
    {
        return entities.GetValueOrDefault(entityID, null);
    }
}
