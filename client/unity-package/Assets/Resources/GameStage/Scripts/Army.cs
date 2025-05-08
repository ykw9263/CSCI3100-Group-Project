using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.U2D;


public struct stats {
        public int id ;  
        public int hp ;
        public int attack ;
        public int speed ;
}   
public abstract class Army : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderSprite;
    [SerializeField] GameObject curPathObject;
    [SerializeField] GameObject popupObject;
    [SerializeField] GameObject planPathObject;

    const float PATH_RENDER_THREADSHOLD = 1;

    private int counter; 
    public int ownerID;
    public stats info;

    public int count;
    public Vector3 cur_pos, des_pos, planned_des_pos;
    public int des_territoryID = -1;
    public bool is_traveling = false;
    public bool finished_traveling = true;

    public List<Army> attackTarget;
    public bool isDead = false ;



    // Update is called once per frame
    protected void Update()
    {
        counter += 1;
        if (is_traveling)
        {
            travel();
        }

        if (counter % 30 == 0)
        {
            if (attackTarget.Count != 0)
            {
                is_traveling = false;
                Attack(attackTarget[0]);
            }
            else if (!is_traveling && !finished_traveling)
            {
                is_traveling = true;
            }
        }

    }

    protected void OnDestroy()
    {
        GameState.GetGameState().entities.GetValueOrDefault(ownerID)?.army.Remove(this);
    }

    public void travel(){
        
        //Debug.Log("traveling");
        //target = territory.transform.position;    
        //Debug.Log(des_pos);
        //transform.position = transform.position + Vector3.left * info.speed ;
        transform.position = Vector3.MoveTowards(transform.position, des_pos , (info.speed+9)*Time.deltaTime);
        cur_pos = transform.position ;

        Vector3 disp = des_pos - cur_pos ;
        UpdatePathLine(curPathObject, disp);
        UpdatePathLine(planPathObject, planned_des_pos - cur_pos);

        if (disp.magnitude <= 0.01){
            //Debug.Log("finish traveling") ;
            finished_traveling = true;
            is_traveling = false ; 
        }
    }
    public void setStats(int hp, int atk , int speed) {
        Debug.Log("SetStats");
        info.hp = hp;
        info.attack = atk;
        info.speed = speed ;
    }
    public void SetDestination(Territory terr){
        des_pos = terr.coordinates ; 
        is_traveling = true ;
        finished_traveling = false;
    }
    public void SetDestination(Vector2 des_pos)
    {
        this.des_pos = (Vector3)des_pos;
        is_traveling = true;
        finished_traveling = false;
    }

    public void PlanDestination(Vector2 des_pos, bool valid)
    {
        this.planned_des_pos = (Vector3)des_pos;
        Vector3 displacement = this.planned_des_pos - cur_pos;

        planPathObject.GetComponent<SpriteShapeRenderer>().color =
                    valid ?
                    new Color(0x7C / 256f, 0xCC / 256f, 0xE9 / 256f) :
                    new Color(0xFF / 256f, 0x33 / 256f, 0x67 / 256f);

        planPathObject.SetActive(true);
        UpdatePathLine(planPathObject, planned_des_pos - cur_pos);
    }

    public void CommitPlanDestination(bool commit) {
        Debug.Log("commit plan destination" + commit);
        planPathObject.SetActive(false);
        if (commit)
        {
            SetDestination(planned_des_pos);
        }
    }

    public void AddAttackTarget(Army army){
        if (army.ownerID == this.ownerID)
        {
            return ;
        }
        //Debug.Log("Add Target");
        attackTarget.Add(army) ;
    }

    public void RemoveAttackTarget(Army army){
        //Debug.Log("Remove Target");
        attackTarget.Remove(army) ;
    }
    public void Attack(Army army) {
        if (!army || army.isDead) {
            RemoveAttackTarget(army);
        }
        //Debug.Log("Enemy take damage") ;
        army.TakeDamage(info.attack) ;
        
    }
    public void TakeDamage(int damage){
        Debug.Log($"{this.ownerID} : Taking Damage") ;
        info.hp -= damage ; 
        if(info.hp <= 0 && this)
        {
            isDead = true ;
            //owner.army.Remove(this) ;
            Destroy(this.gameObject) ; 
        } 
    }

    public void HandleSelect(bool select)
    {
        if (select) {
            if (popupObject) popupObject.SetActive(true);
            if (curPathObject && (des_pos - cur_pos).magnitude > PATH_RENDER_THREADSHOLD) curPathObject.SetActive(true);
        }
        else
        {
            if(popupObject) popupObject.SetActive(false);
            if (curPathObject) curPathObject.SetActive(false);
        }
    }
    /*
        protected void OnMouseDown(){
            //selected = true ; 
            //Debug.Log("selected");
            SetDestination(GameState.GetGameState().territories[0]);
            is_traveling = true;
        }
    */
    public void UpdatePathLine(GameObject pathObject, Vector3 disp) {
        if (pathObject)
        {
            if (disp.magnitude > PATH_RENDER_THREADSHOLD)
            {
                pathObject.GetComponent<SpriteShapeController>().spline.SetPosition(1, disp / transform.localScale.y);
            }
            else
            {
                pathObject.SetActive(false);
            }
        }
    }
    public void SetColor(Color color) {
        if (renderSprite) renderSprite.color = color;
    }
}
    
          
    
