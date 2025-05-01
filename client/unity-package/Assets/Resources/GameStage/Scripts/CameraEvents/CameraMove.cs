using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CameraMove : MonoBehaviour
{
    public Vector2 border;
    public float padding = 10;
    Camera cameraComp;


    [SerializeField] InputAction movement;
    InputSystem_Actions input = null;
    private Vector2 pointed;
    private Vector2 Navigate;

    // Start is called before the first frame update
    void Start()
    {
        cameraComp = this.GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        // HandleMouseOver(pointed);
    }

    private void OnEnable()
    {
        input = new InputSystem_Actions();
        //input.UI.Enable();
        input.UI.Point.performed += SetPointed;
        input.UI.Point.canceled += SetPointed;
        input.UI.Click.performed += SetPointed;
        input.UI.Click.canceled += SetPointed;
    }

    private void OnDisable()
    {
        input.UI.Disable();
    }

    private void HandleClick() { 
    }

    private void SetPointed(InputAction.CallbackContext context)
    {
        this.pointed = context.ReadValue<Vector2>();
        //Debug.Log(pointed);
        //HandleMouseOver(this.pointed);
    }

    public void HandleMouseOver(Vector2 pointed) {
        Vector3 ray3d = cameraComp.ScreenToWorldPoint(new Vector3(pointed.x, pointed.y, 0));
        Vector2 ray2d = new(ray3d.x, ray3d.y);

        RaycastHit2D[] hitsInfo;
        hitsInfo = Physics2D.RaycastAll(ray2d, Vector2.down, 0);
        foreach (var hit in hitsInfo)
        {
            if (hit.collider.gameObject.CompareTag("CameraBorder"))
            {
                CameraBorder cameraBorder= hit.collider.gameObject.GetComponent<CameraBorder>();
                cameraBorder.HandleMouseOver();
            }
        }

    }

    public void MoveCamera(Vector3 movement) {
        Vector3 pos = movement + this.transform.position;
        float physcial_width = (Screen.width *1000/Screen.height) / 10;
        float padded_width = physcial_width - this.padding;
        float padded_height = 100 - this.padding;

        pos.x = pos.x > padded_width ? pos.x : padded_width;
        pos.x = (pos.x < border.x - padded_width) ? pos.x : border.x - padded_width;

        pos.y = (pos.y > 100 - this.padding) ? pos.y : padded_height;
        pos.y = (pos.y < border.y - padded_height) ? pos.y : border.y - padded_height;


        this.transform.position = pos;

        // Debug.Log("camera: "+this.transform.position + "; Screen.height: " + Screen.height);
    }

}
