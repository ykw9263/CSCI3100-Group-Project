using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.U2D;

public class Territory : MonoBehaviour
{
    public int territoryID;
    public int max_hp { get; private set; }
    public int hp;
    public int ownerID;
    public List<int> forces;
    public List<int> neibours;
    public Vector3 coordinates;

    Color color = Color.white;
    Color ownerColor = Color.white;


    public void InitTerritory(int territoryID, int max_hp, Vector3 coordinates, List<int> neibours)
    {
        this.territoryID = territoryID;
        this.max_hp = max_hp;
        this.hp = 0;
        this.ownerID = -1;
        this.coordinates = coordinates;
        this.forces = new List<int>();
        this.neibours = neibours;
        UpdateHPColor();
        
/*        this.territoryGameObject;
        this.territoryGameObject.GetComponent<TerritoryGameObj>()?.SetTerritory(this);*/
    }


    public void TakeDamage(Entity inflictor,int damage) {
        // Debug.Log("attack terr");
        this.hp -= damage;
        if (this.hp < 0)
        {
            //Debug.Log("Fall");
            FallTo(inflictor.entityID);
            return;
        }
        UpdateHPColor();
    }
    public void Repair(int recover)
    {
        Entity owner = GameState.GetGameState().GetEntityByID(ownerID);
        hp += recover;
        if (hp >= max_hp)
        {
            hp = max_hp;
            color = (owner!=null)? owner.color: Color.white;
            SetColor(color);
            return;
        }
        UpdateHPColor();
    }

    public void FallTo(int entID) {
        Entity oldOwner = GameState.GetGameState().entities.GetValueOrDefault(ownerID);
        GameState.GetGameState().GetEntityByID(ownerID)?.LoseControl(this);
        Entity newowner = GameState.GetGameState().GetEntityByID(entID);
        newowner?.TakeControl(this);
        ownerID = entID;
        hp = 0 ;
        color = ownerColor = (newowner != null) ? newowner.color : Color.white;
        UpdateHPColor();

        
        
    }
    public void SetColor(Color color) {
        GetComponent<SpriteShapeRenderer>().color = color;
    }
    public void UpdateHPColor()
    {
        float h, s, v;
        Color.RGBToHSV(ownerColor, out h, out s, out v);
        s *= hp / (float)max_hp;
        s *= 0.8f; v *= 0.8f;
        color = Color.HSVToRGB(h, s, v);
        SetColor(color);
    }
}
