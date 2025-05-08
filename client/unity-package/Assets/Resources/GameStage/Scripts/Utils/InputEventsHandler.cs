using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Windows;


[RequireComponent(typeof(Physics2DRaycaster))]

public class InputEventsHandler : MonoBehaviour
{
    [SerializeField] InputAction movement;
    InputSystem_Actions input = null;

    public Vector2 Navigate { get; private set; }
    public int eventCounter { get; private set; }
    public (int, Vector2) pointed { get; private set; }

    enum EventStates
    {
        free, // can click [UI, army -> dragArmy]; hover [UI]
        selectArmy, // can click [UI, army -> dragArmy]; hover [UI]
        dragArmy, // can release; hover [UI, territory -> sendArmy(), cancel-> free]
    }


    private LayerMask layermask;
    private Dictionary<EventStates, LayerMask> clickLayermasks = new ();
    private Dictionary<EventStates, LayerMask> pointLayermasks = new();

    private Army selectedArmy = null;

    private EventStates cur_state = EventStates.free;
    

    // Start is called before the first frame update
    void Start()
    {
        layermask = LayerMask.GetMask("UserLayerA", "UserLayerB");

        clickLayermasks.Add(EventStates.free, LayerMask.GetMask("UI", "ArmyObj"));
        pointLayermasks.Add(EventStates.free, LayerMask.GetMask("UI"));

        clickLayermasks.Add(EventStates.dragArmy, LayerMask.GetMask("UI"));
        pointLayermasks.Add(EventStates.dragArmy, LayerMask.GetMask("UI", "Map"));
        
            
    }

    // Update is called once per frame
    void Update()
    {
    }

    RaycastHit2D PointerRaycast(LayerMask? layerMask = null)
    {
        Vector2 pointer = this.pointed.Item2;
        return Physics2D.Raycast(
            Camera.main.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, 0)),
            Vector2.down,
            0,
            layerMask.HasValue ? layerMask.Value: Physics2D.DefaultRaycastLayers
        ); ;
    }

    RaycastHit2D[] PointerRaycastAll(LayerMask? layerMask = null) {
        Vector2 pointer = this.pointed.Item2;
        return Physics2D.RaycastAll(
            Camera.main.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, 0))
            ,
            Vector2.down,
            0,
            layerMask.HasValue ? layerMask.Value : Physics2D.DefaultRaycastLayers
        );
    }

    private void OnEnable()
    {
        input = new InputSystem_Actions();
        input.UI.Enable();
        input.UI.Point.performed += HandlePoint;
        // input.UI.Point.canceled += HandlePoint;

        input.UI.Navigate.performed += HandleNavigate;
        input.UI.Navigate.canceled += HandleNavigate;


        input.UI.Click.performed += HandleClick;
        input.UI.Click.canceled += HandleClick;

        input.UI.ScrollWheel.performed += HandleScroll;

    }

    private void OnDisable()
    {
        input.UI.Disable();
    }



    private void HandlePoint(InputAction.CallbackContext context)
    {
        pointed = (++eventCounter, context.ReadValue<Vector2>());
        RaycastHit2D[] hits = PointerRaycastAll();
        foreach (RaycastHit2D hit in hits) {
            GameObject hitObj = hit.collider.gameObject;
            switch (hitObj.tag)
            {
                case "CameraBorder":
                    PointEventSubcriptor cameraBorder = hit.collider.gameObject.GetComponent<PointEventSubcriptor>();
                    cameraBorder.HandlePointedEvent(pointed.Item1);
                    
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleClick(InputAction.CallbackContext context) {
        float clicked = context.ReadValue<float>();   // 1: down, 0 up

        switch (cur_state) {
            case EventStates.free: 
                if (clicked == 1) {
                    RaycastHit2D hit = PointerRaycast(clickLayermasks[EventStates.free]);
                    if (!hit) {
                        setSelectedArmy(null);
                        break;
                    }
                    GameObject hitObj = hit.collider.gameObject;
                    switch (hitObj.tag)
                    {
                        case "ArmyObj":
                            Army tgtArmy = hitObj.GetComponent<Army>();
                            setSelectedArmy(tgtArmy);
                            cur_state = EventStates.selectArmy;
                            break;
                        default:
                            setSelectedArmy(null);
                            break;
                    }
                }
                break;

            case EventStates.dragArmy:
                if (clicked == 0)
                {
                    RaycastHit2D hit = PointerRaycast(clickLayermasks[EventStates.dragArmy]);
                    if (!hit)
                    {
                        setSelectedArmy(null);
                        break;
                    }
                    GameObject hitObj = hit.collider.gameObject;
                    switch (hitObj.tag)
                    {
                        case "TerritoryTile":
                            selectedArmy?.SetDestination(hitObj.GetComponent<Territory>());
                            Debug.Log("hello");
                            break;
                        default:
                            break;
                    }
                }
                cur_state = EventStates.free;
                break;

            default:
                {
                    cur_state = EventStates.free;
                    break;
                }

        
        }

        // RaycastHit2D hit = PointerRaycast();
        // GameObject hitObj = hit.collider.gameObject;

        /*switch (hitObj.tag)
        {
            case "TerritoryTile":
                //PointEventSubcriptor cameraBorder = hit.collider.gameObject.GetComponent<ClickEventSubcriptor>();
                //cameraBorder.HandlePointedEvent(pointed.Item1);
                Debug.Log("hello");
                break;
            default:
                break;
        }*/

    }

    private void HandleScroll(InputAction.CallbackContext context)
    {
        Vector2 scroll = context.ReadValue<Vector2>();
        float camsize = Camera.main.orthographicSize - scroll.y * 0.1f;
        if (camsize > 500) {  }
        camsize = (camsize < 10)? 10 : (camsize < 500)? camsize : 500;
        if (scroll.y > 0)
            Camera.main.transform.position += (Camera.main.ScreenToWorldPoint(new Vector3(pointed.Item2.x, pointed.Item2.y, 0)) - Camera.main.transform.position) / 2;
        Camera.main.orthographicSize = camsize;
    }

    private void HandleNavigate(InputAction.CallbackContext context)
    {
        Navigate = context.ReadValue<Vector2>();

    }

    private void setSelectedArmy(Army army) {
        selectedArmy?.HandleSelect(false);
        selectedArmy = army;
        selectedArmy?.HandleSelect(true);
    }
}
