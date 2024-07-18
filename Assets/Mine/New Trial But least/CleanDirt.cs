using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanDirt : MonoBehaviour
{
    int cleaningItemsCounter;
    int cleaningItems = 2;

    Texture2D wipes;
    int cursorWidth = 200;
    int cursorHeight = 200;
    Vector2 hotspot;
    Vector2Int lastPaintPixelPosition;

    Camera _camera;
    Texture2D dirtMaskTextureBase;
    Texture2D dirtBrush;
    Material material;
    Texture2D dirtMaskTexture;
    float dirtAmountTotal;
    float dirtAmount;
    int dirtCleaned;


    GameObject targetGameObject;
    Renderer targetRenderer;
    Material targetMaterial;

    private void Awake()
    {

    }
    private void Start()
    {
        StartCoroutine(StartCleaningProcess());
    }
    private IEnumerator StartCleaningProcess()
    {
        // Wait for the next frame before continuing the loop
        yield return null;
    }
}
