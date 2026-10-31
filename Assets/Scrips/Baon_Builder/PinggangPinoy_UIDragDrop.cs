using UnityEngine;
using UnityEngine.EventSystems;

public class PinggangPinoy_UIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    [HideInInspector] public Vector2 originalPosition;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public BaonBoxSlot currentSlot = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        // Store initial spawning spot as baseline backup
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform); // Move to top visual layer during drag

        // If dragged OUT of an existing slot, clear it immediately
        if (currentSlot != null)
        {
            currentSlot.ClearSlot();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        // If it wasn't caught by a valid slot during drop, return it to the selection menu
        if (currentSlot == null)
        {
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        currentSlot = null;
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}
