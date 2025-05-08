using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointEventSubcriptor : MonoBehaviour
{
    [SerializeField] InputEventsHandler inputs;

    private int lastPointedID;
    public bool isPointed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPointed)
        {
            if (lastPointedID != inputs.pointed.Item1)
            {
                isPointed = false;
            }
            else
            {
                
            }
        }
    }

    public void HandlePointedEvent(int pointedID) {
        lastPointedID = pointedID;
        isPointed = true;
    }
}
