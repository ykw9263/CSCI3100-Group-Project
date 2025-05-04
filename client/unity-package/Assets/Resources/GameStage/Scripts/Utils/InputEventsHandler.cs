using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Windows;

public class InputEventsHandler : MonoBehaviour
{
    // [SerializeField] cameraComp;
    [SerializeField] InputAction movement;
    InputSystem_Actions input = null;
    //public Vector2 pointed { get; private set; }
    public Vector2 Navigate { get; private set; }
    public int eventCounter { get; private set; }
    public (int, Vector2) pointed { get; private set; }

    public System.Action<int> pointEventCallbacks;

    public enum EventRules {
        FreeSelect,
        SelectTerritory, SelectArmy
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    RaycastHit2D[] PointerRaycast() {
        Vector2 pointer = this.pointed.Item2;
        Vector3 ray3d = Camera.main.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, 0));
        return Physics2D.RaycastAll(
            Camera.main.ScreenToWorldPoint(new Vector3(ray3d.x, ray3d.y, 0))
            , 
            Vector2.down, 
            0
        );
        
    }

    private void OnEnable()
    {
        input = new InputSystem_Actions();
        // input.UI.Enable();
        input.UI.Point.performed += SetPointed;
        // input.UI.Point.canceled += SetPointed;

        input.UI.Navigate.performed += SetNavigate;
        input.UI.Navigate.canceled += SetNavigate;
        
    }
    private void SetPointed(InputAction.CallbackContext context)
    {
        pointed = (++eventCounter, context.ReadValue<Vector2>());

        RaycastHit2D[] hits = PointerRaycast();
        foreach (RaycastHit2D hit in hits) {
            GameObject hitObj = hit.collider.gameObject;
            switch (hitObj.tag)
            {
                case "CameraBorder":
                    PointEventSubcriptor cameraBorder = hit.collider.gameObject.GetComponent<PointEventSubcriptor>();
                    cameraBorder.HandlePointedEvent(pointed.Item1);
                    Debug.Log("hello");
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleOnClick() {
        RaycastHit2D[] hits = PointerRaycast();
        foreach (RaycastHit2D hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;
            switch (hitObj.tag)
            {
                case "CameraBorder":
                    PointEventSubcriptor cameraBorder = hit.collider.gameObject.GetComponent<PointEventSubcriptor>();
                    cameraBorder.HandlePointedEvent(pointed.Item1);
                    Debug.Log("hello");
                    break;
                default:
                    break;
            }
        }
    }

    private void SetNavigate(InputAction.CallbackContext context)
    {
        Navigate = context.ReadValue<Vector2>();

    }

    private void OnDisable()
    {
        input.UI.Disable();
    }
}
