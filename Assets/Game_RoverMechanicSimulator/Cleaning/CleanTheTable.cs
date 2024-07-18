
using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CleanTheTable : MonoBehaviour
{
    [SerializeField] Texture2D wipes; //would be a list of items if needed
    public int cursorWidth = 200;
    public int cursorHeight = 200;
    public Vector2 hotspot;
    public UnityEvent onComplete;
    public bool endCollectingCleanningComponents = false;
    public bool endCleaning = false;
    [SerializeField] private int CleaningItemsCounter = 0; // Counter for collected locker items.
    [SerializeField] private int CleaningItems = 6; // Total number of locker items in the hallway.
    public GameObject targetGameObject;
    [SerializeField] private Camera _Camera;
    [SerializeField] private Texture2D _dirtMaskBase;
    [SerializeField] private Texture2D dirtBrush;
    [SerializeField] private Material _material;
    [SerializeField] private Texture2D dirtMaskTexture;
    [SerializeField] private float dirtAmountTotal;
    [SerializeField] private float dirtAmount;
    [SerializeField] private Vector2Int lastPaintPixelPosition;
    [SerializeField] private int dirtCleaned;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material targetMaterial;
    public string maskTextureName = "BlankGreen";
    public Action OnUpdate;
    public bool dialogueTrigger = false;


    private void Awake()
    {
        //EnableMission();
        Debug.Log("Awake called"); // Add debug statement
    }

    //public override void Start()
    //{
    //    //base.Start();
    //    // Subscribe to the CleaningItemsCollected event
    //    //if (UI_Inventory.Instance != null)
    //    {
    //        //UI_Inventory.Instance.CleaningItemsCollected -= PickedUpItems; // Unsubscribe first to prevent multiple subscriptions
    //        //UI_Inventory.Instance.CleaningItemsCollected += PickedUpItems;
    //    }
    //}
    private void Update()
    {
        if (OnUpdate != null) OnUpdate();
        
    }

    ////public override void ONStartMission()
    //{
    //    base.ONStartMission();
    //Debug.Log("ONStartMission called"); // Add debug statement
    //}

    //public override void Complete()
    //{
    //    base.Complete();
    //    onComplete?.Invoke();
    //    Debug.Log("Complete called"); // Add debug statement
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    StartConversation();
    //    StartMission();
    //    GetComponent<Collider>().enabled = false;
    //    Debug.Log("OnTriggerEnter called"); // Add debug statement
    //}

    public void PickedUpItems()
    {
        CleaningItemsCounter++;
        Debug.Log("CleaningItemsCounter: " + CleaningItemsCounter); // Add debug statement
        if (CleaningItemsCounter == CleaningItems)
        {
            endCollectingCleanningComponents = true;
        }
    }

    public void StartCleaning()
    {
        SetCursorTexture();
        CreateDirtMaskTexture();
        DirtCalculations();
        StartCoroutine(StartCleaningProcess());
    }
    private void SetCursorTexture()
    {
        wipes = ResizeTexture(wipes, cursorWidth, cursorHeight);

        //TableManager.Instance.tableMode.ChangeMode(TableMode.TableModeType.ItemPlacement);

        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(wipes, hotspot, CursorMode.ForceSoftware);
    } // need to be fixed as it's stuck at the middle

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(width, height, source.format, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }


    private IEnumerator StartCleaningProcess()
    {
        while (!IsDirtCleaned())
        {
            if (Input.GetMouseButton(0) && targetRenderer != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Use a raycast to determine the position on the targetGameObject that was clicked
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red); // Add debug statement
                    Debug.Log("Raycast hit: " + hit.collider.gameObject.name); // Add debug statement
        

                    // Check if the hit object is the target game object
                    if (hit.collider.gameObject != targetGameObject)
                    {
                        //fix return as we are using Coroutine
                        continue; // If it's not the target game object, do nothing
                    }
                    // Get the texture coordinates of the hit point
                    Vector2 textureCoord = hit.textureCoord;

                    // Calculate the pixel position on the dirtMaskTexture texture based on the texture coordinates
                    int pixelX = (int)(textureCoord.x * dirtMaskTexture.width);
                    int pixelY = (int)(textureCoord.y * dirtMaskTexture.height);

                    Vector2Int paintPixelPosition = new Vector2Int(pixelX, pixelY);//new

                    int paintPixelDistance = Mathf.Abs(paintPixelPosition.x - lastPaintPixelPosition.x)
                        + Mathf.Abs(paintPixelPosition.y - lastPaintPixelPosition.y);//new
                    int maxPaintDistance = 7;//new
                    if (paintPixelDistance < maxPaintDistance)
                    {
                        // Painting too close to last position
                        continue;//fix Coroutine
                    }
                    lastPaintPixelPosition = paintPixelPosition;
                    // Calculate pixel offset
                    int pixelXOffset = pixelX - (dirtBrush.width / 2);
                    int pixelYOffset = pixelY - (dirtBrush.height / 2);

                    // Apply the cleaning effect to the targetGameObject's texture
                    for (int x = 0; x < dirtBrush.width; x++)
                    {
                        for (int y = 0; y < dirtBrush.height; y++)
                        {
                            Color pixelDirt = dirtBrush.GetPixel(x, y);
                            Color pixelDirtMask = dirtMaskTexture.GetPixel(pixelXOffset + x, pixelYOffset + y);
                            float removedAmount = pixelDirtMask.g - (pixelDirtMask.g * pixelDirt.g);
                            dirtAmount -= removedAmount;
                            dirtMaskTexture.SetPixel(
                            pixelXOffset + x,
                            pixelYOffset + y,
                            new Color(0, pixelDirtMask.g * pixelDirt.g, 0));
                        }
                    }
                    dirtMaskTexture.Apply();
                }
            }
            // Wait for the next frame before continuing the loop
            yield return null;
        }
        // this should return true if the amount of dirt cleaned is 100%
        EndCleaning();
    }

    public void EndCleaning()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        endCleaning = true;
    }


    private void CreateDirtMaskTexture()
    {
        dirtMaskTexture = new Texture2D(_dirtMaskBase.width, _dirtMaskBase.height);
        dirtMaskTexture.SetPixels(_dirtMaskBase.GetPixels());
        dirtMaskTexture.Apply();

        _material.SetTexture(maskTextureName, dirtMaskTexture);
    }

    private void DirtCalculations()
    {
        dirtAmountTotal = 0f;
        for (int x = 0; x < _dirtMaskBase.width; x++)
        {
            for (int y = 0; y < _dirtMaskBase.height; y++)
            {
                dirtAmountTotal += _dirtMaskBase.GetPixel(x, y).g;
            }
        }
        dirtAmount = dirtAmountTotal;
    }

    private float GetDirtAmount()
    {
        return this.dirtAmount / dirtAmountTotal;
    }

    private bool IsDirtCleaned()
    {
        dirtCleaned = Mathf.RoundToInt((1 - GetDirtAmount()) * 100f); // Update calculation to reflect cleaned percentage
        return dirtCleaned >= 100;
    }
    public Func<bool> CollectingCleaningComponentsEnded()
    {
        return () => endCollectingCleanningComponents == true;
    }

    public Func<bool> CleaningEnded()
    {
        return () => endCleaning == true;
    }

    //public Func<bool> GetDialogueTriggerDelegate()
    //{
    //    Debug.Log("GetDialogueTriggerDelegate called"); // Add debug statement
    //    return () => DialogueTrigger() == true;
    //}

    //private bool DialogueTrigger()
    //{
    //    Debug.Log("DialogueTrigger called"); // Add debug statement
    //    //return DialogueLua.GetVariable("Trigger").AsBool;
    //}
}
