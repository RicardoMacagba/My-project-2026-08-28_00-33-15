using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlateDashManager : MonoBehaviour
{
    public static PlateDashManager Instance;

    [Header("Hexagon UI Buttons")]
    public Button redButton;    // GROW
    public Button yellowButton; // GO
    public Button greenButton;  // GLOW
    public Button blueButton;   // WATER

    [Header("Timer")]
    [Min(1f)] public float gameDuration = 60f;
    public Text timerText;

    [Header("Goal Targets")]
    public int targetGo = 1;
    public int targetGrow = 1;
    public int targetGlow = 2;
    public int targetWater = 1;

    [Header("Goal Progress UI")]
    public Text goProgressText;
    public Text growProgressText;
    public Text glowProgressText;
    public Text waterProgressText;

    [Header("Gameplay Settings")]
    public float targetHitRadiusX = 150f; // Pixel distance from center to allow a hit

    // Tracks all active foods currently moving on both conveyor belts
    [HideInInspector] public List<PlateDashFood> activeFoods = new List<PlateDashFood>();

    private float timeRemaining;
    private int goCount;
    private int growCount;
    private int glowCount;
    private int waterCount;

    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        timeRemaining = Mathf.Max(0f, gameDuration);
        UpdateGoalUI();
        UpdateTimerUI();

        // Hook up Hexagon buttons to check their respective food categories
        if (redButton) redButton.onClick.AddListener(() => VerifyPlayerInput(FoodCategory.Grow));
        if (yellowButton) yellowButton.onClick.AddListener(() => VerifyPlayerInput(FoodCategory.Go));
        if (greenButton) greenButton.onClick.AddListener(() => VerifyPlayerInput(FoodCategory.Glow));
        if (blueButton) blueButton.onClick.AddListener(() => VerifyPlayerInput(FoodCategory.Water));
    }

    void Update()
    {
        if (IsGameOver) return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
        UpdateTimerUI();

        if (timeRemaining <= 0f)
        {
            FinishGame(false);
        }
    }

    void VerifyPlayerInput(FoodCategory pressedColor)
    {
        if (IsGameOver) return;

        PlateDashFood targetFood = GetClosestFoodToCenter();

        // If no food is near the center target zone, pressing a button does nothing
        if (targetFood == null) return;

        // RULE 1: Filter out unhealthy trap items first
        if (targetFood.isUnhealthy)
        {
            Debug.Log("💥 Penalty! Attempted to sort junk food onto the plate!");
            targetFood.TriggerSplatFailure();
            return;
        }

        // RULE 2: Validate identity match for healthy options
        if (targetFood.category == pressedColor)
        {
            Debug.Log($"🎯 Perfect! Matched {pressedColor} item successfully.");
            AddGoalProgress(pressedColor);
            targetFood.AnimateFlyToPlate();
        }
        else
        {
            Debug.Log($"❌ Wrong Color! Pressed {pressedColor} but item was {targetFood.category}.");
            targetFood.TriggerSplatFailure();
        }
    }

    void AddGoalProgress(FoodCategory category)
    {
        switch (category)
        {
            case FoodCategory.Go:
                goCount = Mathf.Min(goCount + 1, targetGo);
                break;
            case FoodCategory.Grow:
                growCount = Mathf.Min(growCount + 1, targetGrow);
                break;
            case FoodCategory.Glow:
                glowCount = Mathf.Min(glowCount + 1, targetGlow);
                break;
            case FoodCategory.Water:
                waterCount = Mathf.Min(waterCount + 1, targetWater);
                break;
        }

        UpdateGoalUI();
        if (goCount >= targetGo && growCount >= targetGrow &&
            glowCount >= targetGlow && waterCount >= targetWater)
        {
            FinishGame(true);
        }
    }

    void UpdateGoalUI()
    {
        UpdateProgressText(goProgressText, goCount, targetGo);
        UpdateProgressText(growProgressText, growCount, targetGrow);
        UpdateProgressText(glowProgressText, glowCount, targetGlow);
        UpdateProgressText(waterProgressText, waterCount, targetWater);
    }

    static void UpdateProgressText(Text text, int current, int target)
    {
        if (text == null) return;

        text.text = $"{current}/{target}";
        text.fontSize = 30;
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    void FinishGame(bool goalsMet)
    {
        IsGameOver = true;
        if (!goalsMet)
        {
            timeRemaining = 0f;
            UpdateTimerUI();
        }

        if (redButton) redButton.interactable = false;
        if (yellowButton) yellowButton.interactable = false;
        if (greenButton) greenButton.interactable = false;
        if (blueButton) blueButton.interactable = false;

        Debug.Log(goalsMet
            ? "Plate Dash goal complete!"
            : "Plate Dash time is up. The goal was not completed.");
    }

    public PlateDashFood GetClosestFoodToCenter()
    {
        PlateDashFood closestItem = null;
        float minimumDistance = float.MaxValue;

        // Iterate through all moving plates to find the one closest to screen center (X = 0)
        foreach (var food in activeFoods)
        {
            if (food == null) continue;
            
            // Get local position relative to the Canvas center
            float currentX = Mathf.Abs(food.GetComponent<RectTransform>().anchoredPosition.x);
            
            if (currentX < minimumDistance)
            {
                minimumDistance = currentX;
                closestItem = food;
            }
        }

        // Only return the item if it has entered the playable target threshold zone
        if (minimumDistance <= targetHitRadiusX)
        {
            return closestItem;
        }

        return null;
    }
}
