using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ArmySpawner : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public GameObject knight_prefab ;
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
        GameObject knight = Instantiate(this.knight_prefab);
        knight.transform.parent = this.transform;

        Army soldier = knight.GetComponent<Knight>() ;
        soldier.count = 1; 
        Entity ownerEnt = GameState.GetGameState().entities.GetValueOrDefault(ownerID) ;
        ownerEnt.AddArmy(soldier) ;
        Debug.Log(ownerEnt.entityID) ;

        soldier.cur_pos = ownerEnt.home.coordinates ;
        knight.transform.position = soldier.cur_pos ;
        soldier.SetColor(ownerEnt.color);

        soldier.SetDestination(GameState.GetGameState().territories[debug_dest]) ;
        //Debug.Log(GameState.GetGameState().territories[2].coordinates) ;
    }
}
