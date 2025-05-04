using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryGameObj : MonoBehaviour
{
    private Territory territory;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnMouseDown()
    {
        Debug.Log("Clicked! " + territory.territoryID);
        
    }

    public void SetTerritory(Territory territory) {
        this.territory = territory;
        
    } 
    
}
