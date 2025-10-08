using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampSwitcher : MonoBehaviour
{
    public GameObject lampOn;
    public GameObject lampOff;

    public bool isOn = false;

    public Material outerLampMaterial;
    public Material innerLampMaterial;

    public Texture2D hoverCursorTexture;
   // public Texture2D defaultCursorTexture;

    private void Start()
    {
        outerLampMaterial.GetTextureOffset("_BaseMap");
         innerLampMaterial.GetTextureOffset("_BaseMap");
        SetLampState(false); 
    }

    public void SetLampState(bool isOn)
    {
        if (lampOn != null)
            lampOn.SetActive(isOn);
        if (lampOff != null)
            lampOff.SetActive(!isOn);

        this.isOn = isOn;

        outerLampMaterial.SetTextureOffset("_BaseMap", FishAnimator.outerOffset);
        innerLampMaterial.SetTextureOffset("_BaseMap", FishAnimator.innerOffset);
    }

    private void OnMouseDown()
    {
        SetLampState(!isOn);
    }

    #region Mouse Hover Cursor Change

    private void OnMouseEnter()
    {
        Cursor.SetCursor(hoverCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    #endregion Mouse Hover Cursor Change
}

