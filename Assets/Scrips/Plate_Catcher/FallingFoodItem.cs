using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FallingFoodItem : MonoBehaviour
{
    [Header("Food Type Identity")]
    public bool isUnhealthy = false;
    public FoodCategory healthyCategory; 
    public float dropVelocity = 200.0f;

    public RectTransform RectTransform { get; private set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        Vector2 position = RectTransform.anchoredPosition;
        position.y -= dropVelocity * Time.deltaTime;
        RectTransform.anchoredPosition = position;

        if (PlateCatcherController.Instance != null &&
            PlateCatcherController.Instance.TryCatchFood(this))
        {
            return;
        }

        RectTransform parentRect = RectTransform.parent as RectTransform;
        if (parentRect != null && position.y < parentRect.rect.yMin - RectTransform.rect.height)
        {
            Destroy(gameObject);
        }
    }
}
