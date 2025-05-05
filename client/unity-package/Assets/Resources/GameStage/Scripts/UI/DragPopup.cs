using UnityEngine;
using System.Collections;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(Collider2D))]


public class DraggablePopup : MonoBehaviour
{

    private Vector3 screenPoint;
    private Vector3 cursorOffset;
    private Vector3 popupOffset;
    private Vector3 worldSize;
    private void Start()
    {
        popupOffset = gameObject.transform.parent.position - gameObject.transform.position;
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 screenOffset = new(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        cursorOffset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(screenOffset);
        worldSize = 
            Camera.main.ScreenToWorldPoint(new(Screen.width, Screen.height, screenPoint.z)) -
            Camera.main.ScreenToWorldPoint(Vector3.zero);
    }
    void OnMouseUp()
    {
        //Debug.Log("released");
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        Vector3 targePosition = curPosition + cursorOffset + popupOffset;

        Vector3 worldZero = Camera.main.ScreenToWorldPoint(Vector3.zero);
        
        targePosition.x = Mathf.Min(Mathf.Max(targePosition.x, worldZero.x), worldSize.x + worldZero.x);
        targePosition.y = Mathf.Min(Mathf.Max(targePosition.y, worldZero.y), worldSize.y + worldZero.y);
/*
        targePosition.y = (targePosition.y > worldZero.y) ? targePosition.y : worldZero.y;
        targePosition.y = (targePosition.y < worldSize.y) ? targePosition.y : worldSize.y;*/

        transform.parent.position = targePosition;

    }

}