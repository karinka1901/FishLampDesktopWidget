using UnityEngine;

public class CursorSetter : MonoBehaviour
{
 public Texture2D normalCursorTexture;

    private void OnMouseEnter()
    {
        Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
