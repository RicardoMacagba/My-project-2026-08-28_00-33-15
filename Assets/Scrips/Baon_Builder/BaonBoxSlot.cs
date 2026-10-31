using UnityEngine;
using UnityEngine.EventSystems;

public class BaonBoxSlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    public FoodCategory allowedCategory; // Set this in inspector (Go, Grow, Glow, or Water)
    
    [Header("References")]
    public BaonGameManager gameManager;

    private PinggangPinoy_UIDragDrop occupiedItem = null;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            PinggangPinoy_UIDragDrop draggedItem = eventData.pointerDrag.GetComponent<PinggangPinoy_UIDragDrop>();
            FoodItem_PP food = eventData.pointerDrag.GetComponent<FoodItem_PP>();

            if (draggedItem != null && food != null && food.category == allowedCategory && occupiedItem == null)
            {
                occupiedItem = draggedItem;
                draggedItem.currentSlot = this;

                draggedItem.transform.SetParent(transform, true);
                draggedItem.transform.position = transform.position;

                if (gameManager != null)
                {
                    gameManager.AddFoodToTray(food);
                }
            }
        }
    }

    // Called when a food item gets dragged away or cleared out
    public void ClearSlot()
    {
        if (occupiedItem != null)
        {
            FoodItem_PP food = occupiedItem.GetComponent<FoodItem_PP>();
            if (gameManager != null && food != null)
            {
                gameManager.RemoveFoodFromTray(food);
            }
            if (occupiedItem.currentSlot == this)
            {
                occupiedItem.currentSlot = null;
            }
            occupiedItem = null;
        }
    }
}
