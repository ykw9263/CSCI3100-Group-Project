using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ArmySpawner : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public GameObject knight_prefab;
    public int debug_dest = 2;

    void Start()
    {
        //knight_prefab = Resources.Load<GameObject>("GameStage/Miniature Army 2D V.1/Prefab/Knight");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void spawn(int ownerID){

        Entity ownerEnt = GameState.GetGameState().entities.GetValueOrDefault(ownerID);
        GameObject knight = Instantiate(this.knight_prefab);
        Army soldier = knight.GetComponent<Army>();
        soldier.ownerID = ownerID;
        if (ownerEnt is Player) {
            if ( ((Player)ownerEnt).skill.gold <= 0 )
            {
                Debug.Log("No Gold");
                Destroy(knight);
                return;
            }
            ((Player)ownerEnt).skill.MinusGold();
            if (ownerID == 0)
            {
                Debug.Log("spawnStats");
                soldier.setStats(((Player)ownerEnt).skill);
            }
        }
        knight.transform.parent = this.transform;

        soldier.count = 1;
        
        ownerEnt.AddArmy(soldier);     
        //Debug.Log($"Owner : {soldier.owner.entityID}, type:{soldier} , owner :{soldier.owner} ") ;
        soldier.cur_pos = ownerEnt.home.coordinates ;
        knight.transform.position = soldier.cur_pos ;
        soldier.SetColor(ownerEnt.color);

        soldier.SetDestination(GameState.GetGameState().territories[debug_dest]);
        //Debug.Log(GameState.GetGameState().territories[2].coordinates) ;
    }
    public void spawnEnemy(int ownerID)
    {

        Entity ownerEnt = GameState.GetGameState().entities.GetValueOrDefault(ownerID);
        GameObject knight = Instantiate(this.knight_prefab);
        knight.transform.parent = this.transform;
        Army soldier = knight.GetComponent<Army>();
        soldier.ownerID = ownerID ;
        soldier.info.attack = 8 ;
        soldier.info.hp = 3;
        soldier.info.speed =2; 
        soldier.count = 1;
        foreach (Territory terr in ownerEnt.territories_in_Controls) 
        {
            Debug.Log(terr.coordinates);
        }
        
        ownerEnt.AddArmy(soldier);
        //Debug.Log($"Owner : {soldier.owner.entityID}, type:{soldier} , owner :{soldier.owner} ") ;
        soldier.cur_pos = ownerEnt.home.coordinates;
        knight.transform.position = soldier.cur_pos;
        soldier.SetColor(ownerEnt.color);

        soldier.SetDestination(GameState.GetGameState().territories[debug_dest]);
        //Debug.Log(GameState.GetGameState().territories[2].coordinates) ;
    }
}
