using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct stats {
        public int hp ;
        public int attack ;
        public int speed ;
}   
public abstract class Army 
{
    public int count ; 
    public Entity owner;
    public stats info;
    public Army()
    {
        
    }
    public void travel(Territory territory){
    
    }
    public void attack(Army enemy){
        
    }
}