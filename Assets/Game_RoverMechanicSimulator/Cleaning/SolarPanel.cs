using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CodeMonkey.Utils;
using UnityEngine.Rendering.HighDefinition;

public class SolarPanel : MonoBehaviour
{

    [SerializeField] private Texture2D dirtMaskTextureBase;
    [SerializeField] private Texture2D dirtBrush;
    [SerializeField] private Material material;
    [SerializeField] private TextMeshProUGUI uiText;

    [SerializeField] private Texture2D dirtMaskTexture;
    [SerializeField] private bool isFlipped;
    [SerializeField] private Animation solarAnimation;
    [SerializeField] private float dirtAmountTotal;
    [SerializeField] private float dirtAmount;
    [SerializeField] private Vector2Int lastPaintPixelPosition;
    [SerializeField] public GameObject targetGameObject;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material targetMaterial;

    private void Awake()
    {
        

        dirtMaskTexture = new Texture2D(dirtMaskTextureBase.width, dirtMaskTextureBase.height);
        dirtMaskTexture.SetPixels(dirtMaskTextureBase.GetPixels());
        dirtMaskTexture.Apply();
        material.SetTexture("_DirtMask", dirtMaskTexture);
        targetGameObject.GetComponent<Renderer>().material = material;
        solarAnimation = GetComponent<Animation>();
        if (targetGameObject != null)
        {
            targetRenderer = targetGameObject.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError("Missing Renderer component on targetGameObject.");
                enabled = false; // Disable this script if Renderer is missing
                return;
            }
            targetMaterial = targetRenderer.material;
        }
        else
        {
            Debug.LogError("Target game object is not assigned.");
            enabled = false; // Disable this script if targetGameObject is not assigned
        }
        dirtAmountTotal = 0f;
        for (int x = 0; x < dirtMaskTextureBase.width; x++)
        {
            for (int y = 0; y < dirtMaskTextureBase.height; y++)
            {
                dirtAmountTotal += dirtMaskTextureBase.GetPixel(x, y).g;
            }
        }
        dirtAmount = dirtAmountTotal;

        FunctionPeriodic.Create(() =>
        {
            uiText.text = Mathf.RoundToInt(GetDirtAmount() * 100f) + "%";
        }, .03f);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && targetRenderer != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the hit object is the target game object
                if (hit.collider.gameObject != targetGameObject)
                {
                    return; // If it's not the target game object, do nothing
                }

                Vector2 textureCoord = hit.textureCoord;
                int pixelX = (int)(textureCoord.x * dirtMaskTexture.width);
                int pixelY = (int)(textureCoord.y * dirtMaskTexture.height);
                Vector2Int paintPixelPosition = new Vector2Int(pixelX, pixelY);

                int paintPixelDistance = Mathf.Abs(paintPixelPosition.x - lastPaintPixelPosition.x) + Mathf.Abs(paintPixelPosition.y - lastPaintPixelPosition.y);
                int maxPaintDistance = 7;
                if (paintPixelDistance < maxPaintDistance)
                {
                    // Painting too close to last position
                    return;
                }
                lastPaintPixelPosition = paintPixelPosition;

                // Calculate pixel offset
                int pixelXOffset = pixelX - (dirtBrush.width / 2);
                int pixelYOffset = pixelY - (dirtBrush.height / 2);

                // Paint brush logic
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
                            new Color(0, pixelDirtMask.g * pixelDirt.g, 0)
                        );
                    }
                }

                dirtMaskTexture.Apply();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isFlipped = !isFlipped;
            if (isFlipped)
            {
                solarAnimation.Play("SolarPanelFlip");
            }
            else
            {
                solarAnimation.Play("SolarPanelFlipBack");
            }
        }
    }

    private float GetDirtAmount()
    {
        return this.dirtAmount / dirtAmountTotal;
    }

}
