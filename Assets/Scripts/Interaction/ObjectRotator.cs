using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectRotator : MonoBehaviour
{

    private bool canRotate = false;
    public bool rotationModeEnabled = false;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private bool inverted = false;
    [SerializeField] private bool enableXAxisRotation = true;
    [SerializeField] private bool enableYAxisRotation = true;
    [SerializeField] private float maxXRotation = 80f;
    [SerializeField] private bool lockToSingleAxis = true;
    [SerializeField] private float axisLockThreshold = 0.1f; // Threshold to lock an axis
    
    private Quaternion initialRotation;
    private float currentXRotation = 0f;

    #region Axis locking state
    private bool isAxisLocked = false;
    private bool isXAxisLocked = false;
    private bool isYAxisLocked = false;
    #endregion

    [Header("Input")]
    [SerializeField] InputActionReference RightClickAction;
    [Header("Cursor Textures")]
    [SerializeField] private Texture2D rotateCursorTexture;
    [SerializeField] private Texture2D rotateXCursorTexture;
    [SerializeField] private Texture2D rotateYCursorTexture;

    private void Awake()
    {
        if(RightClickAction != null)
        {
            RightClickAction.action.started += OnRightClickPressed;
            RightClickAction.action.performed += OnRightClickPressed;
            RightClickAction.action.canceled += OnRightClickPressed;
        }
    }

    private void Start()
    {
        initialRotation = transform.rotation;
        currentXRotation = transform.eulerAngles.x;
    }

    private void Update()
    {
        if(!canRotate) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float rotationDirection = inverted ? -1f : 1f;

        if (lockToSingleAxis)
        {
            // Determine axis lock 
            if (!isAxisLocked)
            {
                float absMouseX = Mathf.Abs(mouseX);
                float absMouseY = Mathf.Abs(mouseY);

                if (absMouseX > axisLockThreshold || absMouseY > axisLockThreshold)
                {
                    if (absMouseX > absMouseY)
                    {
                        isYAxisLocked = true; // Lock to Yaxis rotation
                        isAxisLocked = true;
                        Cursor.SetCursor(rotateYCursorTexture, Vector2.zero, CursorMode.Auto);
                    }
                    else
                    {
                        isXAxisLocked = true; // Lock to Xaxis rotation
                        isAxisLocked = true;
                        Cursor.SetCursor(rotateXCursorTexture, Vector2.zero, CursorMode.Auto);
                    }
                }
            }

            // Rotate based on locked axis
            if (isYAxisLocked && enableYAxisRotation)
            {
                transform.Rotate(Vector3.up, mouseX * rotationSpeed * rotationDirection, Space.World);
            }
            else if (isXAxisLocked && enableXAxisRotation)
            {
                float rotationAmount = -mouseY * rotationSpeed * rotationDirection;
                currentXRotation += rotationAmount;
                currentXRotation = Mathf.Clamp(currentXRotation, -maxXRotation, maxXRotation);
                
                Vector3 eulerAngles = transform.eulerAngles;
                eulerAngles.x = currentXRotation;
                transform.eulerAngles = eulerAngles;
            }
        }
        else
        {
            // Original dual-axis behavior
            if (enableYAxisRotation && Mathf.Abs(mouseX) > 0.01f)
            {
                transform.Rotate(Vector3.up, mouseX * rotationSpeed * rotationDirection, Space.World);
            }

            if (enableXAxisRotation && Mathf.Abs(mouseY) > 0.01f)
            {
                float rotationAmount = -mouseY * rotationSpeed * rotationDirection;
                currentXRotation += rotationAmount;
                currentXRotation = Mathf.Clamp(currentXRotation, -maxXRotation, maxXRotation);
                
                Vector3 eulerAngles = transform.eulerAngles;
                eulerAngles.x = currentXRotation;
                transform.eulerAngles = eulerAngles;
            }
        }
    }

    protected virtual void OnRightClickPressed(InputAction.CallbackContext context)
    {
        if(!rotationModeEnabled) return;

        if (context.performed)
        {
            canRotate = true;
            // Reset axis lock when starting new rotation
            isAxisLocked = false;
            isXAxisLocked = false;
            isYAxisLocked = false;
            
            Cursor.SetCursor(rotateCursorTexture, Vector2.zero, CursorMode.Auto);
        }
        else if (context.canceled)
        {
            canRotate = false;
            // Reset axis lock when stopping rotation
            isAxisLocked = false;
            isXAxisLocked = false;
            isYAxisLocked = false;
            
            ResetCursor();
        }
    }

    public void ResetRotation()
    {
        transform.rotation = initialRotation;
        currentXRotation = initialRotation.eulerAngles.x;
        Debug.Log("Reset rotation to " + initialRotation.eulerAngles);
    }

    private void ResetCursor()
    {
        ObjectDragger dragger = GetComponent<ObjectDragger>();

        if (dragger != null && dragger.canDrag)
        {
            Cursor.SetCursor(dragger.dragCursorTexture, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
