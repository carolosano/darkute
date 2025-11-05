using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("Movimiento vertical")]
    [SerializeField] private float floatAmplitude = 0.2f;  
    [SerializeField] private float floatFrequency = 1.5f;  

    [Header("Rotación opcional")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 35f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        if (rotate)
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}

