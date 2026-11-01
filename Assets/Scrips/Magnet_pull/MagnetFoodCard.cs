using UnityEngine;
using UnityEngine.UI;

public enum GlowFoodType { None, Fruit, Vegetable }

[RequireComponent(typeof(Button))]
public class MagnetFoodCard : MonoBehaviour
{
    public FoodCategory1 foodGroupCategory; // Choose Go, Grow, Glow, or Water per asset inspector node
    public GlowFoodType glowFoodType;
    
    private Button interactionButton;
    private Image shelfCardImage;
    public Vector3 ShelfHomePosition { get; private set; }

    void Awake()
    {
        ShelfHomePosition = transform.position;
        shelfCardImage = GetComponent<Image>();
        interactionButton = GetComponent<Button>();
        interactionButton.onClick.AddListener(OnCardClickedAction);
    }

    void OnCardClickedAction()
    {
        // Only allow launch if the rope state machinery is currently sitting idle
        if (RopeMagnetGameManager.Instance != null && RopeMagnetGameManager.Instance.currentRopeState == RopeState.Idle)
        {
            RopeMagnetGameManager.Instance.TriggerRopeLaunch(this);
        }
    }

    public void ResetToShelfHome()
    {
        transform.position = ShelfHomePosition;
        if (shelfCardImage != null) shelfCardImage.raycastTarget = true; // Restore interactive clicking features
    }
}
