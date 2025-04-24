using System.Collections.Generic;
using UnityEngine;

public class Territory
{
    public int territoryID;
    public int hp;
    public int controlledBy;
    public List<int> forces;
    public Vector2 coordinates;
    private GameObject territoryGameObject;

    Territory(int territoryID, int hp, Vector2 coordinates, GameObject territoryGameObject) {
        this.territoryID = territoryID;
        this.hp = territoryID;
        this.controlledBy = -1;
        this.coordinates = coordinates;
        this.forces = new List<int>();
        this.territoryGameObject = territoryGameObject;
    }

}
