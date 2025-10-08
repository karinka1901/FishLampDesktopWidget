using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AppMenuController : MonoBehaviour
{
    public GameObject menuCnavas;
    public Button quitButton;
    public Button rotateButton;
    public GameObject buttonsPanel;
    public GameObject rotateSettingsPanel;

    private Vector3 offset;
    private float zCoord;
    public bool isMouseOver = false;

    private void Start()
    {
        ResetSettings();
        quitButton.onClick.AddListener(QuitApp);
       // rotateButton.onClick.AddListener(OpenRotateSettings);
    }

    public void QuitApp()
    {
        Application.Quit();
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            menuCnavas.SetActive(true);
        }
        
        if (Input.GetMouseButtonDown(0) && menuCnavas.activeSelf)
        {
            if (!IsMouseOverUI())
            {
                ResetSettings();
            }
            else
            {
                return;
            }
            
        }
    }

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OpenRotateSettings()
    {
      buttonsPanel.SetActive(false);
        rotateSettingsPanel.SetActive(true);
    }

    private void ResetSettings()
    {
        menuCnavas.SetActive(false);
        buttonsPanel.SetActive(true);
        rotateSettingsPanel.SetActive(false);
    }
}
