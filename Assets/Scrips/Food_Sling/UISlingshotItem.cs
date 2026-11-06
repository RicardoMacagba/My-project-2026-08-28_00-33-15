using UnityEngine;
using UnityEngine.EventSystems;

public class UISlingshotItem : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private Vector3 startPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Rigidbody2D rb2d;
    private bool isLaunched = false;
    private SlingGameManager gameManager;

    public bool IsLaunched => isLaunched;

    [Header("Sling Physics Settings")]
    public float launchForceMultiplier = 5f;
    public float maxDragDistance = 150f;

    [Header("Rubber Band References")]
    private UILineRenderer rubberBandLine;
    private Transform leftProng;
    private Transform rightProng;

    void Awake()
    {
        if (GetComponent<UISlingshotItem>() != this)
        {
            enabled = false;
            Destroy(this);
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (rectTransform == null || canvas == null)
        {
            Debug.LogError("Slingshot food needs a RectTransform under a Canvas.", this);
            enabled = false;
            return;
        }

        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null) rb2d = gameObject.AddComponent<Rigidbody2D>();

        CircleCollider2D foodCollider = GetComponent<CircleCollider2D>();
        if (foodCollider == null) foodCollider = gameObject.AddComponent<CircleCollider2D>();
        foodCollider.radius = Mathf.Max(0.1f, Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.4f);

        rb2d.bodyType = RigidbodyType2D.Kinematic; 
        rb2d.simulated = false;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(SlingGameManager manager)
    {
        gameManager = manager;
    }

    public void SetupInitialPosition(Vector3 slingPos, UILineRenderer line, Transform left, Transform right)
    {
        rectTransform.position = slingPos;
        startPosition = rectTransform.anchoredPosition;
        
        rubberBandLine = line;
        leftProng = left;
        rightProng = right;

        if (rubberBandLine != null) rubberBandLine.gameObject.SetActive(true);

        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.simulated = false;
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0f;
        isLaunched = false;
        
        UpdateRubberBand();
    }

    void Update()
    {
        if (!isLaunched || gameManager == null)
            return;

        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, transform.position);
        const float padding = 100f;
        if (screenPosition.x < -padding || screenPosition.x > Screen.width + padding ||
            screenPosition.y < -padding || screenPosition.y > Screen.height + padding)
        {
            gameManager.ResetMissedFood(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLaunched || gameManager == null || gameManager.IsGameOver) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLaunched || gameManager == null || gameManager.IsGameOver) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out Vector2 localPoint
        );

        Vector2 dragDir = localPoint - (Vector2)startPosition;
        
        if (dragDir.magnitude > maxDragDistance)
        {
            dragDir = dragDir.normalized * maxDragDistance;
        }

        rectTransform.anchoredPosition = (Vector2)startPosition + dragDir;
        UpdateRubberBand();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLaunched || gameManager == null || gameManager.IsGameOver) return;

        isLaunched = true;
        if (rubberBandLine != null) rubberBandLine.gameObject.SetActive(false); // Hide line on release

        Vector2 dragForce = (Vector2)startPosition - rectTransform.anchoredPosition;

        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.simulated = true;
        rb2d.gravityScale = 180f; // High gravity scale optimized for Canvas pixel units
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        rb2d.AddForce(dragForce * launchForceMultiplier, ForceMode2D.Impulse);
    }

    void UpdateRubberBand()
    {
        if (rubberBandLine == null || leftProng == null || rightProng == null || isLaunched) return;

        RectTransform canvasRect = canvas.transform as RectTransform;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, leftProng.position, canvas.worldCamera, out Vector2 leftLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, rightProng.position, canvas.worldCamera, out Vector2 rightLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, transform.position, canvas.worldCamera, out Vector2 fruitLocal);

        rubberBandLine.points[0] = leftLocal;
        rubberBandLine.points[1] = fruitLocal;
        rubberBandLine.points[2] = rightLocal;
    }
}
