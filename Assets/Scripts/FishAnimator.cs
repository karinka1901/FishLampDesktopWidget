using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAnimator : MonoBehaviour
{
    public Material fishOuterMaterial;
    public Material fishInnerMaterial;

    public static Vector2 outerOffset;
    public static Vector2 innerOffset;

    public float outerScrollSpeed = 0.03f;
    public float innerScrollSpeed = 0.01f; 

    public bool isAnimating = false;

    private void Start()
    {
        if (fishOuterMaterial != null)
            outerOffset = fishOuterMaterial.GetTextureOffset("_BaseMap");

        if (fishInnerMaterial != null)
            innerOffset = fishInnerMaterial.GetTextureOffset("_BaseMap");
    }

    private void Update()
    {
        if (!isAnimating) return;

        // accumulate offset over time
        outerOffset.x -= outerScrollSpeed * Time.deltaTime;
        innerOffset.x += innerScrollSpeed * Time.deltaTime;

        // apply to materials
        if (fishOuterMaterial != null)
            fishOuterMaterial.SetTextureOffset("_BaseMap", outerOffset);

        if (fishInnerMaterial != null)
            fishInnerMaterial.SetTextureOffset("_BaseMap", innerOffset);
    }
}
