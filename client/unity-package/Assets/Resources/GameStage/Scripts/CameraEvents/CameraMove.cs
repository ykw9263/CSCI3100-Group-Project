
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Vector2 border = new(300, 300);
    public float padding = 10;
    Camera cameraComp;


    // Start is called before the first frame update
    void Start()
    {
        cameraComp = this.GetComponent<Camera>();
        MoveCamera(Vector3.zero);
    }
    // Update is called once per frame
    void Update()
    {
        // HandleMouseOver(pointed);
    }


    public void MoveCamera(Vector3 movement) {
        Vector3 pos = movement + this.transform.position;
        float physcial_width = (Screen.width *1000/Screen.height) / 10;
        float padded_width = physcial_width - this.padding;
        float padded_height = 100 - this.padding;
        padded_width *=  this.cameraComp.orthographicSize / 100;
        padded_height *=  this.cameraComp.orthographicSize / 100;

        pos.x = (pos.x < border.x - padded_width) ? pos.x : border.x - padded_width;
        pos.x = pos.x > padded_width ? pos.x : padded_width;

        pos.y = (pos.y < border.y - padded_height) ? pos.y : border.y - padded_height;
        pos.y = (pos.y > padded_height) ? pos.y : padded_height;


        this.transform.position = pos;

        // Debug.Log("camera: "+this.transform.position + "; Screen.height: " + Screen.height);
    }

}
