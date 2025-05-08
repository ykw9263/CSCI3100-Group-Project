using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.U2D;

public class Territory : MonoBehaviour
{
    public int territoryID;
    public int max_hp { get; private set; }
    public int hp;
    public Entity owner;
    public List<int> forces;
    public List<int> neibours;
    public Vector3 coordinates;
    // private GameObject territoryGameObject;
    Color color = Color.white;

/*    public Territory(int territoryID, int max_hp, Vector3 coordinates, GameObject territoryGameObject, List<int> neibours) {
        this.territoryID = territoryID;
        this.max_hp = max_hp;
        this.hp = max_hp;
        this.owner = null;
        this.coordinates = coordinates;
        this.forces = new List<int>();
        this.neibours = neibours;
        this.territoryGameObject = territoryGameObject;
        this.territoryGameObject.GetComponent<TerritoryGameObj>()?.SetTerritory(this);
    }*/

    public void InitTerritory(int territoryID, int max_hp, Vector3 coordinates, List<int> neibours)
    {
        this.territoryID = territoryID;
        this.max_hp = max_hp;
        this.hp = max_hp;
        this.owner = null;
        this.coordinates = coordinates;
        this.forces = new List<int>();
        this.neibours = neibours;

/*        this.territoryGameObject;
        this.territoryGameObject.GetComponent<TerritoryGameObj>()?.SetTerritory(this);*/
    }


    public void TakeDamage(Entity inflictor,int damage) {
        this.hp -= damage;
        if (this.hp < 0) {
            FallTo(inflictor);
        }
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        color = Color.HSVToRGB(h, s, v);
    }
    public void Repair(int recover)
    {

        hp += recover;
        if (hp >= max_hp)
        {
            hp = max_hp;
            color = owner.color;
        }
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        color = Color.HSVToRGB(h, s, v);
    }

    public void FallTo(Entity ent) {
        owner?.LoseControl(this);
        ent.TakeControl(this);
        owner = ent;
        color = ent.color;
        hp = max_hp;
        GetComponent<SpriteShapeRenderer>().color = color;
    }

}
