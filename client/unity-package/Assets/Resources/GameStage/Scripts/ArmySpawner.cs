using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public void spawn(int owner){
        GameObject knight = Instantiate(this.knight_prefab) ;
        knight.transform.parent = this.transform;
        Army soldier = knight.GetComponent<Army>() ;
        if (owner == 0)
        {
            Debug.Log("spawnStats");
            soldier.setStats(sp.hp, sp.atk, sp.speed);
        }
        soldier.count = 1;
        Entity ownerEnt = GameState.GetGameState().entities.GetValueOrDefault(ownerID);
        ownerEnt.AddArmy(soldier);
        Debug.Log(soldier.owner.entityID) ;        
        //Debug.Log($"Owner : {soldier.owner.entityID}, type:{soldier} , owner :{soldier.owner} ") ;
        soldier.cur_pos = ownerEnt.home.coordinates ;
        knight.transform.position = soldier.cur_pos ;
        soldier.SetColor(ownerEnt.color);

        soldier.SetDestination(GameState.GetGameState().territories[debug_dest]) ;
        //Debug.Log(GameState.GetGameState().territories[2].coordinates) ;
    }
}
