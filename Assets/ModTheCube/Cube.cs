using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public MeshRenderer Renderer;


    // Transform Properties
    public Vector3 minPosition;
    public Vector3 maxPosition;
    public Vector3 minScale;
    public Vector3 maxScale;
    public Vector3 minRotationSpeed;
    public Vector3 maxRotationSpeed;
    Vector3 rotationSpeed;

    // Material Properties
    public Color color = Color.white;
    [Range(0f, 1f)]
    public float opacity = 1f;


    void Start()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
        Material material = Renderer.material;
        material.color = new Color(Random.value, Random.value, Random.value, opacity);
        
        RandomizeALL();
        
    }

    private void RandomizeALL()
    {
        // Randomize Position
        transform.position = new Vector3(
            Random.Range(minPosition.x, maxPosition.x),
            Random.Range(minPosition.y, maxPosition.y),
            Random.Range(minPosition.z, maxPosition.z)
        );

        // Randomize Scale
        transform.localScale = new Vector3(
            Random.Range(minScale.x, maxScale.x),
            Random.Range(minScale.y, maxScale.y),
            Random.Range(minScale.z, maxScale.z)
        );

        // Randomize Rotation Speed
        rotationSpeed = new Vector3(
            Random.Range(minRotationSpeed.x, maxRotationSpeed.x),
            Random.Range(minRotationSpeed.y, maxRotationSpeed.y),
            Random.Range(minRotationSpeed.z, maxRotationSpeed.z)
        );

    }

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
    void RandomizeColor()
    {
        color = new Color(Random.value, Random.value, Random.value);
    }
}
