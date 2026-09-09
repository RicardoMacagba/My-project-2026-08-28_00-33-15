using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 300f;       

    private RectTransform rectTransform;
    private float backgroundHeight;
    private float safetyOverlap = 4f; // 🚨 SAFETY BUFFER: Overlaps edges by 4 pixels

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        backgroundHeight = rectTransform.rect.height;
    }

    void Update()
    {
        // Move this map segment down the screen over time
        rectTransform.anchoredPosition += Vector2.down * scrollSpeed * Time.deltaTime;

        // 🚨 THE LINE FIX: Teleport back up 4 pixels EARLY before the bottom edge gets exposed!
        if (rectTransform.anchoredPosition.y <= (-backgroundHeight + safetyOverlap))
        {
            // Snap it perfectly back to the top, maintaining the overlap buffer
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, backgroundHeight - safetyOverlap);
        }
    }
}
