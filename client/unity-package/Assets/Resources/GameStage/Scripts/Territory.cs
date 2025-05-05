using System.Collections.Generic;
using UnityEngine;

public class Territory
{
    public int territoryID;
    public int hp;
    public int controlledBy;
    public List<int> forces;
    public List<int> neibours;
    public Vector3 coordinates;
    private GameObject territoryGameObject;

    public Territory(int territoryID, int hp, Vector3 coordinates, GameObject territoryGameObject, List<int> neibours) {
        this.territoryID = territoryID;
        this.hp = territoryID;
        this.controlledBy = -1;
        this.coordinates = coordinates;
        this.forces = new List<int>();
        this.neibours = neibours;
        this.territoryGameObject = territoryGameObject;
        this.territoryGameObject.GetComponent<TerritoryGameObj>()?.SetTerritory(this);
    }


}
