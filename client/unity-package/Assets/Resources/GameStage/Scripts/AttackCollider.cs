using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter Collison") ;
        Army enemy = collision.gameObject.GetComponent<Army>() ;
        Army myself = this.GetComponentInParent<Army>() ;
        if (enemy == null || myself == null )
        {
            //Debug.Log($"enemy: {enemy},myself {myself}" ) ;
            return;
        }
        myself.AddAttackTarget(enemy);

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //Debug.Log("Exit Collison");
        //Army enemy = collision.gameObject.GetComponentInParent<Army>()
        Army enemy = collision.gameObject.GetComponent<Army>();
        Army myself = this.GetComponentInParent<Army>();
        if (enemy == null || myself == null )
        {
            return;
        }
        myself.RemoveAttackTarget(enemy);

    }
}
