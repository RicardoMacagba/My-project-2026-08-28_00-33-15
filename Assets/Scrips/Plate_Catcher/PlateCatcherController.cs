using UnityEngine;
using UnityEngine.UI;

public class PlateCatcherController : MonoBehaviour
{
    public static PlateCatcherController Instance { get; private set; }

    [Header("Horizontal Assembly Assets")]
    public RectTransform rollerAssembly; // Drag your 'roller' object here
    public RectTransform catcherArea;
    public float moveSpeed = 400f;
    public float maxLeft = -300f;
    public float maxRight = 300f;

    [Header("On-Screen Input Buttons")]
    public Button leftButton;
    public Button rightButton;

    private float horizontalTargetPosition;
    private readonly Vector3[] catcherCorners = new Vector3[4];
    private readonly Vector3[] foodCorners = new Vector3[4];

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (rollerAssembly != null) horizontalTargetPosition = rollerAssembly.anchoredPosition.x;
        if (rollerAssembly == null || catcherArea == null)
            Debug.LogError("PlateCatcherController needs both the roller assembly and catcher area assigned.", this);

        if (leftButton != null) leftButton.onClick.AddListener(() => MoveHorizontal(-150f));
        if (rightButton != null) rightButton.onClick.AddListener(() => MoveHorizontal(150f));
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // Smoothly slide the tray assembly to the destination point
        if (rollerAssembly != null)
        {
            Vector2 currentPos = rollerAssembly.anchoredPosition;
            currentPos.x = Mathf.MoveTowards(currentPos.x, horizontalTargetPosition, moveSpeed * Time.deltaTime);
            rollerAssembly.anchoredPosition = currentPos;
        }
    }

    public bool TryCatchFood(FallingFoodItem food)
    {
        if (food == null || catcherArea == null || food.RectTransform == null)
            return false;

        if (!Overlaps(catcherArea, food.RectTransform))
            return false;

        if (PlateCatcherManager.Instance == null)
        {
            Debug.LogError("PlateCatcherManager is missing from the scene.", this);
        }
        else if (food.isUnhealthy)
        {
            PlateCatcherManager.Instance.PenaltyUnhealthyFood();
        }
        else
        {
            PlateCatcherManager.Instance.AddScore(food.healthyCategory);
        }

        Destroy(food.gameObject);
        return true;
    }

    bool Overlaps(RectTransform first, RectTransform second)
    {
        first.GetWorldCorners(catcherCorners);
        second.GetWorldCorners(foodCorners);

        Rect firstBounds = Rect.MinMaxRect(
            catcherCorners[0].x,
            catcherCorners[0].y,
            catcherCorners[2].x,
            catcherCorners[2].y);
        Rect secondBounds = Rect.MinMaxRect(
            foodCorners[0].x,
            foodCorners[0].y,
            foodCorners[2].x,
            foodCorners[2].y);

        return firstBounds.Overlaps(secondBounds);
    }

    void MoveHorizontal(float distanceOffset)
    {
        horizontalTargetPosition = Mathf.Clamp(horizontalTargetPosition + distanceOffset, maxLeft, maxRight);
    }
}
