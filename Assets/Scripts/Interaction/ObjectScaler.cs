using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectScaler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference ScrollDownAction;
    public InputActionReference ScrollUpAction;
    public InputActionReference RighClickAction;

    [Header("Scaling Settings")]
    [SerializeField] private float scaleSpeed = 0.1f;
    [SerializeField] private bool canScale = false;

    [Header("CursorSettings")]
    [SerializeField] private Texture2D scaleCursorTexture;

    [Header("Reset Settings")]
    public Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;

        if (ScrollDownAction != null)
        {
            ScrollDownAction.action.started += OnScrollDown;
            ScrollDownAction.action.performed += OnScrollDown;
            ScrollDownAction.action.canceled += OnScrollDown;
        }
        if (ScrollUpAction != null)
        {
            ScrollUpAction.action.started += OnScrollUp;
            ScrollUpAction.action.performed += OnScrollUp;
            ScrollUpAction.action.canceled += OnScrollUp;
        }
        if (RighClickAction != null)
        {
            RighClickAction.action.started += OnRightClickPressed;
            RighClickAction.action.performed += OnRightClickPressed;
            RighClickAction.action.canceled += OnRightClickPressed;
        }
    }

    private void OnRightClickPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            canScale = true;
        }
        else if (context.canceled)
        {
            canScale = false;
        }
    }

    private void OnScrollDown(InputAction.CallbackContext context)
    {
        if (!canScale) return;
        Vector3 newScale = transform.localScale - Vector3.one * scaleSpeed;
        if (newScale.x > 0.1f && newScale.y > 0.1f && newScale.z > 0.1f) 
        {
            transform.localScale = newScale;
        }
        Cursor.SetCursor(scaleCursorTexture, Vector2.zero, CursorMode.Auto);
        if (context.canceled)
        {
            ResetCursor();
        }
    }
    private void OnScrollUp(InputAction.CallbackContext context)
    {
        if (!canScale) return;
        Vector3 newScale = transform.localScale + Vector3.one * scaleSpeed;
        if(newScale.x < 3f && newScale.y < 3f && newScale.z < 3f) 
        {
            transform.localScale = newScale;
        }
        Cursor.SetCursor(scaleCursorTexture, Vector2.zero, CursorMode.Auto);
        if(context.canceled)
        {
            ResetCursor();
        }
    }

    public void Reset()
    {
        transform.localScale = initialScale;
        Debug.Log("Reset scale to " + initialScale);
    }

    private void ResetCursor()
    {
       ObjectRotator rotator = GetComponent<ObjectRotator>();
       ObjectDragger dragger = GetComponent<ObjectDragger>();

        if (rotator != null && rotator.rotationModeEnabled)
         {
              Cursor.SetCursor(scaleCursorTexture, Vector2.zero, CursorMode.Auto);
         }
         else if (dragger != null && dragger.canDrag)
         {
              Cursor.SetCursor(dragger.dragCursorTexture, Vector2.zero, CursorMode.Auto);
         }
         else if(!canScale)
         {
              Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
