using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmySpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject knight_prefab ;
    
    void Start()
    {
        knight_prefab = Resources.Load<GameObject>("GameStage/Miniature Army 2D V.1/Prefab/Knight");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void spawn(){
        GameObject knight = Instantiate(this.knight_prefab);
        knight.transform.parent = this.transform;
    }
}
