using UnityEngine;

public class ObjectDragger : MonoBehaviour
{
    [SerializeField] private bool isDragging = false;

    private Vector3 offset;
    private float zCoord;

    public Texture2D dragCursorTexture;
    //public Texture2D defaultCursorTexture;


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
    }
    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    #endregion Mouse Hover Cursor Change
}
