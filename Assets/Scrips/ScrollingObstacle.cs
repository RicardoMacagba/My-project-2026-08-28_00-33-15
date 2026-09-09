using UnityEngine;

public class ScrollingObstacle : MonoBehaviour
{
    [Header("Movement Speed")]
    public float scrollSpeed = 4f; // Small single digits work best for world coordinates!

    [Header("Cleanup Boundaries")]
    public float destroyLineY = -6f; // Deletes once it passes below the camera frame window

    void Update()
    {
        // Smoothly move downward in world coordinates every frame over time
        transform.Translate(Vector2.down * scrollSpeed * Time.deltaTime);

        // Cleanup engine to keep your game running smooth
        if (transform.position.y <= destroyLineY)
        {
            Destroy(gameObject);
        }
    }
}
