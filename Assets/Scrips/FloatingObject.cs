using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float speed = 1.5f;     // How fast it bobs up and down
    [SerializeField] private float height = 15f;     // How high/low it floats

    private Vector3 startPosition;
    private float randomOffset;

    private void Start()
    {
        // Remember where you placed the fruit on the Canvas screen
        startPosition = transform.localPosition;
        
        // Gives each fruit a random start time so they don't bob in a uniform robotic grid
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        // Uses a mathematical sine wave to make the fruit smoothly drift up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * speed + randomOffset) * height;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
