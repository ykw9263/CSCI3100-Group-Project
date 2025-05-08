using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmySpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject knight_prefab ;
    [SerializeField] Skill sp ;
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
        
        Army soldier = knight.GetComponent<Army>() ;
        if (owner == 0) {
            Debug.Log("spawnStats"); 
            soldier.setStats( sp.hp, sp.atk, sp.speed ) ; 
        }
        soldier.count = 2; 
        soldier.owner = GameState.GetGameState().entities[owner] ;
        soldier.owner.AddArmy(soldier) ;
        Debug.Log(soldier.owner.entityID) ;
        soldier.cur_pos = soldier.owner.home.coordinates ;
        
        //Debug.Log($"Owner : {soldier.owner.entityID}, type:{soldier} , owner :{soldier.owner} ") ;
        knight.transform.parent = this.transform ;

        knight.transform.position = soldier.cur_pos ;

        soldier.SetDestination(GameState.GetGameState().territories[2]) ;
        //Debug.Log(GameState.GetGameState().territories[2].coordinates) ;
    }
}
