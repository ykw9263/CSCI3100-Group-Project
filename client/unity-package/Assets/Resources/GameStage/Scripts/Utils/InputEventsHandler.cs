using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;



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
        free, // can click [UI, army -> selectArmy]; hover [UI]
        selectArmy, // can click [UI, army -> dragArmy]; hover [UI]     // maybe not needed
        dragArmy, // can release; hover [UI, territory -> sendArmy(), cancel-> free]

    }


    private LayerMask layermask;
    private Dictionary<EventStates, LayerMask> clickLayermasks = new ();
    private Dictionary<EventStates, LayerMask> pointLayermasks = new();

    const int UI_LAYERMASK = 1 << 5; // the LayerMask.GetMask() seems buggy on ui layer so we manually make one

    private Army selectedArmy = null;
    private Territory hoveredTerr = null;

    private EventStates cur_state = EventStates.free;
    

    // Start is called before the first frame update
    void Start()
    {
        layermask = LayerMask.GetMask("UserLayerA", "UserLayerB");

        clickLayermasks.Add(EventStates.free, LayerMask.GetMask("ArmyObj")| UI_LAYERMASK);
        pointLayermasks.Add(EventStates.free, LayerMask.GetMask() | UI_LAYERMASK);

        clickLayermasks.Add(EventStates.selectArmy, LayerMask.GetMask("ArmyObj") | UI_LAYERMASK);
        pointLayermasks.Add(EventStates.selectArmy, LayerMask.GetMask() | UI_LAYERMASK);

        clickLayermasks.Add(EventStates.dragArmy, LayerMask.GetMask("Map") | UI_LAYERMASK);
        pointLayermasks.Add(EventStates.dragArmy, LayerMask.GetMask("Map") | UI_LAYERMASK);

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

    private void UIPointed()
    {
        RaycastHit2D[] uiHits = PointerRaycastAll(UI_LAYERMASK);
        foreach (RaycastHit2D uiHit in uiHits)
        {
            GameObject hitObj = uiHit.collider.gameObject;
            PointEventSubcriptor pointSubscriber = uiHit.collider.gameObject.GetComponent<PointEventSubcriptor>();
            if (pointSubscriber) pointSubscriber.HandlePointedEvent(pointed.Item1);
        }
    }

    private void HandlePoint(InputAction.CallbackContext context)
    {
        pointed = (++eventCounter, context.ReadValue<Vector2>());
        switch (cur_state)
        {
            case EventStates.free:
                {
                    UIPointed();
                }
                break;

            case EventStates.selectArmy:
                UIPointed();
                break;
            case EventStates.dragArmy:
                {
                    UIPointed();
                    RaycastHit2D hit = PointerRaycast(pointLayermasks[EventStates.dragArmy]);
                    Vector2 pointer = this.pointed.Item2;
                    Vector3 worldPoint = Camera.main.ScreenToWorldPoint(pointer);
                    bool valid = hit ? hit.collider.gameObject.CompareTag("Territory"): false;
                    selectedArmy.PlanDestination(worldPoint, valid);
                }
                break;
        }
    }

    private void HandleClick(InputAction.CallbackContext context) {
        float clicked = context.ReadValue<float>();   // 1: down, 0 up

        switch (cur_state) {
            case EventStates.free:
                {
                    if (clicked == 1)
                    {
                        RaycastHit2D hit = PointerRaycast(clickLayermasks[EventStates.free]);
                        if (!hit)
                        {
                            setSelectedArmy(null);
                            break;
                        }
                        GameObject hitObj = hit.collider.gameObject;
                        switch (hitObj.tag)
                        {
                            case "ArmyObj":
                                Army tgtArmy = hitObj.GetComponent<Army>();
                                setSelectedArmy(tgtArmy);
                                cur_state = EventStates.dragArmy;
                                break;
                            default:
                                setSelectedArmy(null);
                                break;
                        }
                    }

                }
                break;
            case EventStates.selectArmy:
                {
                    if (clicked == 1)
                    {
                        RaycastHit2D hit = PointerRaycast(clickLayermasks[EventStates.selectArmy]);
                        if (!hit)
                        {
                            setSelectedArmy(null);
                            cur_state = EventStates.free;
                            break;
                        }
                        Debug.Log("selectArmy hit");
                        GameObject hitObj = hit.collider.gameObject;
                        switch (hitObj.tag)
                        {
                            case "ArmyObj":
                                Army tgtArmy = hitObj.GetComponent<Army>();
                                if(tgtArmy == selectedArmy)
                                    cur_state = EventStates.dragArmy;
                                else
                                    setSelectedArmy(tgtArmy);
                                break;
                            default:
                                setSelectedArmy(null);
                                cur_state = EventStates.free;
                                break;
                        }
                    }
                }
                break;
            case EventStates.dragArmy:
                
                if (clicked == 0)
                {
                    RaycastHit2D hit = PointerRaycast(clickLayermasks[EventStates.dragArmy]);
                    if (!hit)
                    {
                        if (selectedArmy) selectedArmy.CommitPlanDestination(false);
                        setSelectedArmy(null);
                        cur_state = EventStates.free;
                        break;
                    }
                    GameObject hitObj = hit.collider.gameObject;
                    if (selectedArmy) selectedArmy.CommitPlanDestination(hitObj.CompareTag("Territory"));
                }
                // should be holding mouse, so no mouse down should enter here.
                cur_state = EventStates.free;
                break;

            default:
                {
                    cur_state = EventStates.free;
                    break;
                }

            
        }
        // Debug.Log("event state: " + cur_state);
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
        if(selectedArmy) selectedArmy.HandleSelect(false);
        selectedArmy = army;
        if (selectedArmy) selectedArmy.HandleSelect(true);
    }
}
