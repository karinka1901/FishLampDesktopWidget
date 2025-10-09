using UnityEngine;

public class ObjectDragger : MonoBehaviour
{
    public bool canDrag = false;

    private Vector3 offset;
    private float zCoord;

    public Texture2D dragCursorTexture;

    public Vector3 initialPosition;
    //public Texture2D defaultCursorTexture;

    private void Start()
    {
        initialPosition = transform.position;
    }
    private void OnMouseDown()
    {
        zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        offset = gameObject.transform.position - GetMouseWorldPos();

    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
        
    }


    #region Mouse Hover Cursor Change
    private void OnMouseEnter()
    {
        Cursor.SetCursor(dragCursorTexture, Vector2.zero, CursorMode.Auto);
        canDrag = true;
    }
    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        canDrag = false;
    }
    #endregion Mouse Hover Cursor Change

    public void Reset()
    {
        transform.position = initialPosition;
        Debug.Log("Reset position" + initialPosition);
    }
}
