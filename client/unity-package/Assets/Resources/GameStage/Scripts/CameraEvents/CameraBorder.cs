using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraBorder : MonoBehaviour
{
    [SerializeField] CameraMove _camera;
    PointEventSubcriptor subscriptor;
    public int direction = 0;
    public float speed = 1; 
    private bool isHovered;
    private int lastPointedID;

    // Start is called before the first frame update
    void Start()
    {
        subscriptor = GetComponent<PointEventSubcriptor>();
    }
    void Update() {
        if (subscriptor != null && subscriptor.isPointed )
        {
            HandleMouseOver();
            //Debug.Log("subscriptor: " + this.subscriptor);
        }
    }

    private void HandleMouseOver()
    {
        if (!_camera || direction == 0)
        {
            return;
        }
        switch (this.direction)
        {
            case 1:
                _camera.MoveCamera(new(0, speed / 30, 0));
                break;
            case 2:
                _camera.MoveCamera(new(0, -speed / 30, 0));
                break;
            case 3:
                _camera.MoveCamera(new(-speed / 30, 0, 0));
                break;
            case 4:
                _camera.MoveCamera(new(speed / 30, 0, 0));
                break;
            default:
                break;
        }
        //Debug.Log("direction: "+ this.direction);
    }

    public void Hello() {
        Debug.Log("hi");
    }
}
