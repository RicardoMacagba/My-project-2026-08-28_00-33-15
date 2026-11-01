using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Add 'Unhealthy' to your structural category types
public enum FoodCategory1 { Go, Grow, Glow, Water, Unhealthy }
public enum RopeState { Idle, Stretching, PullingHome, Completed, Failed }

public class RopeMagnetGameManager : MonoBehaviour
{
    public static RopeMagnetGameManager Instance;

    [Header("UI Goal Configurations")]
    public Text goalTextDisplay;
    public Text timerTextDisplay;
    public float levelTimer = 60f;

    [Header("Widescreen Plate Targets")]
    public RectTransform goPlateSlice;    
    public RectTransform growPlateSlice;  
    public RectTransform glowPlateSlice;  
    public RectTransform secondGlowPlateSlice;
    public RectTransform waterPlateSlice; 

    [Header("UI Rope Configuration")]
    public RectTransform uiRopeElement;
    public RectTransform ropeOriginPoint;
    public float ropeExtendSpeed = 500f;
    public float ropeRetractSpeed = 400f;
    public Button playAgainButton;

    [HideInInspector] public RopeState currentRopeState = RopeState.Idle;
    [HideInInspector] public FoodCategory1 currentActiveGoal;
    
    private float startingLevelTimer;
    private Vector3 currentTargetCoordinates;
    private Transform currentlyAttachedFood;
    private MagnetFoodCard currentlyActiveCard;
    private Coroutine activeRopeCoroutine;
    private Canvas uiCanvas;
    private Image uiRopeImage;
    private RectTransform magnetRect;
    private Quaternion magnetRestRotation;
    private static Sprite ropeSprite;
    private readonly Dictionary<FoodCategory1, int> placedFoodCounts = new Dictionary<FoodCategory1, int>();
    private readonly List<MagnetFoodCard> foodCards = new List<MagnetFoodCard>();
    private readonly List<GameObject> plateFoodDisplays = new List<GameObject>();
    private readonly FoodCategory1[] goalSequence =
    {
        FoodCategory1.Go,
        FoodCategory1.Grow,
        FoodCategory1.Glow,
        FoodCategory1.Glow,
        FoodCategory1.Water
    };
    private int currentGoalIndex = -1;

    void Awake()
    {
        Instance = this;
        uiCanvas = GetComponentInParent<Canvas>();
        startingLevelTimer = levelTimer;
        if (uiCanvas != null)
        {
            foodCards.AddRange(uiCanvas.GetComponentsInChildren<MagnetFoodCard>(true));
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
            playAgainButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Assign the existing Play Again button to RopeMagnetGameManager.");
        }

        magnetRect = transform as RectTransform;
        if (magnetRect != null)
        {
            magnetRestRotation = magnetRect.localRotation;
        }

        if (uiRopeElement != null)
        {
            uiRopeImage = uiRopeElement.GetComponent<Image>();
            if (uiRopeImage != null)
            {
                if (ropeSprite == null)
                {
                    ropeSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0, 0.5f));
                }

                uiRopeElement.anchorMin = new Vector2(0.5f, 0.5f);
                uiRopeElement.anchorMax = new Vector2(0.5f, 0.5f);
                uiRopeElement.pivot = new Vector2(0, 0.5f);
                uiRopeElement.sizeDelta = new Vector2(0, 18f);
                uiRopeImage.sprite = ropeSprite;
                uiRopeImage.raycastTarget = false;
                uiRopeImage.enabled = false;
            }
            else
            {
                Debug.LogError("The assigned UI Rope Element needs an Image component.");
            }
        }
        else
        {
            Debug.LogError("Assign the UI Rope Element on RopeMagnetGameManager.");
        }

        ClearRopeVisuals();
    }

    void Start()
    {
        GenerateNextRandomGoal();
    }

    void Update()
    {
        if (currentRopeState == RopeState.Completed || currentRopeState == RopeState.Failed)
        {
            return;
        }

        if (levelTimer > 0)
        {
            levelTimer = Mathf.Max(0, levelTimer - Time.deltaTime);
        }

        if (timerTextDisplay != null)
        {
            timerTextDisplay.text = "Time: " + Mathf.CeilToInt(levelTimer).ToString() + "s";
        }

        if (levelTimer <= 0)
        {
            EndGameForTimeout();
        }
    }

    public void GenerateNextRandomGoal()
    {
        currentGoalIndex++;
        if (currentGoalIndex >= goalSequence.Length)
        {
            currentRopeState = RopeState.Completed;
            SetFoodCardsInteractable(false);
            UpdateGoalUI();
            ShowPlayAgainButton();
            return;
        }

        currentRopeState = RopeState.Idle;
        currentActiveGoal = goalSequence[currentGoalIndex];
        UpdateGoalUI();
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
        }
    }

    void UpdateGoalUI()
    {
        if (goalTextDisplay == null) return;
        switch (currentGoalIndex)
        {
            case 0: goalTextDisplay.text = "GOAL: CATCH 1 GO FOOD! (YELLOW)"; break;
            case 1: goalTextDisplay.text = "GOAL: CATCH 1 GROW FOOD! (RED)"; break;
            case 2: goalTextDisplay.text = "GOAL: CATCH 1 GLOW FRUIT! (GREEN)"; break;
            case 3: goalTextDisplay.text = "GOAL: CATCH 1 GLOW VEGETABLE! (GREEN)"; break;
            case 4: goalTextDisplay.text = "GOAL: CATCH 1 WATER! (BLUE)"; break;
            default: goalTextDisplay.text = "ALL GOALS COMPLETE!"; break;
        }
    }

    void EndGameForTimeout()
    {
        if (activeRopeCoroutine != null)
        {
            StopCoroutine(activeRopeCoroutine);
            activeRopeCoroutine = null;
        }

        if (currentlyActiveCard != null)
        {
            currentlyActiveCard.ResetToShelfHome();
        }

        ClearRopeVisuals();
        currentRopeState = RopeState.Failed;
        SetFoodCardsInteractable(false);

        if (timerTextDisplay != null)
        {
            timerTextDisplay.text = "Time: 0s";
        }

        if (goalTextDisplay != null)
        {
            goalTextDisplay.text = "TIME'S UP!.";
        }

        ShowPlayAgainButton();
    }

    void ShowPlayAgainButton()
    {
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(true);
            playAgainButton.transform.SetAsLastSibling();
        }
    }

    void SetFoodCardsInteractable(bool isInteractable)
    {
        foreach (MagnetFoodCard card in foodCards)
        {
            if (card == null) continue;

            Button cardButton = card.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.interactable = isInteractable;
            }
        }
    }

    void PlayAgain()
    {
        foreach (GameObject display in plateFoodDisplays)
        {
            if (display != null)
            {
                Destroy(display);
            }
        }
        plateFoodDisplays.Clear();
        placedFoodCounts.Clear();

        foreach (MagnetFoodCard card in foodCards)
        {
            if (card == null) continue;

            card.gameObject.SetActive(true);
            card.ResetToShelfHome();
            Button cardButton = card.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.interactable = true;
            }
        }

        levelTimer = startingLevelTimer;
        currentGoalIndex = -1;
        currentRopeState = RopeState.Idle;
        activeRopeCoroutine = null;
        currentlyActiveCard = null;
        currentlyAttachedFood = null;
        if (timerTextDisplay != null)
        {
            timerTextDisplay.text = "Time: " + Mathf.CeilToInt(levelTimer).ToString() + "s";
        }

        if (magnetRect != null)
        {
            magnetRect.localRotation = magnetRestRotation;
        }

        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
        }

        GenerateNextRandomGoal();
    }

    public void TriggerRopeLaunch(MagnetFoodCard clickedCard)
    {
        if (currentRopeState != RopeState.Idle) return; 
        if (clickedCard == null || ropeOriginPoint == null)
        {
            Debug.LogError("Cannot launch the magnet rope: the clicked food or rope origin is not assigned.");
            return;
        }

        if (uiRopeElement == null || uiRopeImage == null || uiCanvas == null ||
            uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogError("Cannot launch the magnet rope: assign a UI Rope Image and place the manager under a Screen Space - Overlay Canvas.");
            return;
        }

        if (clickedCard.foodGroupCategory != FoodCategory1.Unhealthy &&
            (GetDestinationSlice(clickedCard.foodGroupCategory, GetPlacedFoodCount(clickedCard.foodGroupCategory)) == null))
        {
            Debug.LogError("Cannot launch the magnet rope: the plate slice for " + clickedCard.foodGroupCategory + " is not assigned.");
            return;
        }
        
        currentlyActiveCard = clickedCard;
        currentlyAttachedFood = clickedCard.transform;
        currentTargetCoordinates = clickedCard.transform.position;
        
        activeRopeCoroutine = StartCoroutine(RopeMovementCycleSequence());
    }

    IEnumerator RopeMovementCycleSequence()
    {
        currentRopeState = RopeState.Stretching;
        bool removeCardAfterPickup = false;
        bool goalCompleted = false;
        
        Color targetRopeColor = GetCategoryColor(currentlyActiveCard.foodGroupCategory);
        SetRopeVisible(true);

        Vector3 dynamicTipPos = ropeOriginPoint.position;
        UpdateRopeVisual(ropeOriginPoint.position, dynamicTipPos, targetRopeColor);
        while (Vector3.Distance(dynamicTipPos, currentTargetCoordinates) > 0.1f)
        {
            dynamicTipPos = Vector3.MoveTowards(dynamicTipPos, currentTargetCoordinates, ropeExtendSpeed * Time.deltaTime);
            UpdateRopeVisual(ropeOriginPoint.position, dynamicTipPos, targetRopeColor);
            yield return null;
        }

        // --- NEW RULES FOR PROCESSING UNHEALTHY OBSTACLES ---
        if (currentlyActiveCard.foodGroupCategory == FoodCategory1.Unhealthy)
        {
            removeCardAfterPickup = true;
            levelTimer = Mathf.Max(0, levelTimer - 10f); // Bigger penalty for junk food (e.g., -10 seconds)
            Debug.Log("Unhealthy food caught: deducted 10 seconds. Retracting rope.");
            
            // Instead of pulling junk food to the plate, the rope retracts empty back to the magnet hook
            Vector3 retractTarget = ropeOriginPoint.position;
            while (Vector3.Distance(dynamicTipPos, retractTarget) > 0.1f)
            {
                dynamicTipPos = Vector3.MoveTowards(dynamicTipPos, retractTarget, ropeRetractSpeed * Time.deltaTime);
                UpdateRopeVisual(retractTarget, dynamicTipPos, targetRopeColor, false);
                yield return null;
            }
        }
        else if (!IsCorrectForCurrentGoal(currentlyActiveCard))
        {
            levelTimer = Mathf.Max(0, levelTimer - 5f);
            Debug.Log("Wrong food for current goal. Goal: " + GetCurrentGoalDescription() +
                "; selected: " + currentlyActiveCard.gameObject.name + " (" +
                currentlyActiveCard.foodGroupCategory + ", " + currentlyActiveCard.glowFoodType +
                "). Deducted 5 seconds; returning food to shelf.");

            currentRopeState = RopeState.PullingHome;
            Vector3 shelfHomePosition = currentlyActiveCard.ShelfHomePosition;
            Image cardImage = currentlyActiveCard.GetComponent<Image>();
            if (cardImage != null) cardImage.raycastTarget = false;

            while (Vector3.Distance(currentlyAttachedFood.position, shelfHomePosition) > 0.1f)
            {
                currentlyAttachedFood.position = Vector3.MoveTowards(
                    currentlyAttachedFood.position,
                    shelfHomePosition,
                    ropeRetractSpeed * Time.deltaTime);
                UpdateRopeVisual(ropeOriginPoint.position, currentlyAttachedFood.position, targetRopeColor);
                yield return null;
            }

            currentlyActiveCard.ResetToShelfHome();
        }
        else 
        {
            currentRopeState = RopeState.PullingHome;
            removeCardAfterPickup = true;
            int placedFoodCount = GetPlacedFoodCount(currentlyActiveCard.foodGroupCategory);
            RectTransform targetPlateSlice = GetDestinationSlice(currentlyActiveCard.foodGroupCategory, placedFoodCount);
            Vector3 sliceWorldDestination = GetFoodPlacementPosition(currentlyActiveCard.foodGroupCategory, targetPlateSlice, placedFoodCount);

            Image cardImage = currentlyActiveCard.GetComponent<Image>();
            if (cardImage != null) cardImage.raycastTarget = false;

            while (Vector3.Distance(currentlyAttachedFood.position, sliceWorldDestination) > 0.1f)
            {
                currentlyAttachedFood.position = Vector3.MoveTowards(currentlyAttachedFood.position, sliceWorldDestination, ropeRetractSpeed * Time.deltaTime);
                UpdateRopeVisual(ropeOriginPoint.position, currentlyAttachedFood.position, targetRopeColor);
                yield return null;
            }

            if (CreatePlateFoodDisplay(currentlyActiveCard, sliceWorldDestination))
            {
                placedFoodCounts[currentlyActiveCard.foodGroupCategory] = placedFoodCount + 1;
                goalCompleted = true;
            }
        }

        // --- PHASE 3: CLEANUP & RESET ---
        ClearRopeVisuals();
        if (removeCardAfterPickup)
        {
            currentlyActiveCard.gameObject.SetActive(false);
        }

        if (goalCompleted)
        {
            currentRopeState = RopeState.PullingHome;
        }
        
        yield return new WaitForSeconds(0.3f);
        if (goalCompleted)
        {
            GenerateNextRandomGoal();
        }

        activeRopeCoroutine = null;
    }

    void ClearRopeVisuals()
    {
        SetRopeVisible(false);
        if (magnetRect != null)
        {
            magnetRect.localRotation = magnetRestRotation;
        }
        currentRopeState = RopeState.Idle;
    }

    void SetRopeVisible(bool isVisible)
    {
        if (uiRopeImage != null)
        {
            uiRopeImage.enabled = isVisible;
            if (isVisible)
            {
                uiRopeElement.SetAsLastSibling();
            }
        }

    }

    void UpdateRopeVisual(Vector3 start, Vector3 end, Color color, bool rotateMagnet = true)
    {
        if (uiRopeImage != null && uiCanvas != null)
        {
            if (rotateMagnet)
            {
                SetMagnetFacing(end);
                start = ropeOriginPoint.position;
            }

            RectTransform canvasRect = uiCanvas.transform as RectTransform;
            Vector2 startScreen = RectTransformUtility.WorldToScreenPoint(null, start);
            Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(null, end);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, startScreen, null, out Vector2 startLocal) ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, endScreen, null, out Vector2 endLocal))
            {
                Debug.LogError("Could not convert the rope endpoints to Screen Space - Overlay Canvas coordinates.");
                return;
            }

            Vector2 direction = endLocal - startLocal;
            uiRopeElement.anchoredPosition = startLocal;
            uiRopeElement.sizeDelta = new Vector2(direction.magnitude, 18f);
            uiRopeElement.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            uiRopeImage.color = color;
        }
    }

    void SetMagnetFacing(Vector3 targetPosition)
    {
        if (magnetRect == null || magnetRect.parent == null)
        {
            return;
        }

        Vector3 targetDirection = magnetRect.parent.InverseTransformVector(targetPosition - magnetRect.position);
        if (targetDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector3 restHookDirection = magnetRestRotation * Vector3.down;
        float rotationAngle = Vector2.SignedAngle(restHookDirection, targetDirection);
        magnetRect.localRotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward) * magnetRestRotation;
    }

    Color GetCategoryColor(FoodCategory1 cat)
    {
        switch (cat)
        {
            case FoodCategory1.Go: return Color.yellow;
            case FoodCategory1.Grow: return Color.red;
            case FoodCategory1.Glow: return Color.green;
            case FoodCategory1.Water: return Color.blue;
            case FoodCategory1.Unhealthy: return Color.magenta; // Distinguishable error color tag
            default: return Color.white;
        }
    }

    int GetPlacedFoodCount(FoodCategory1 category)
    {
        return placedFoodCounts.TryGetValue(category, out int count) ? count : 0;
    }

    bool IsCorrectForCurrentGoal(MagnetFoodCard card)
    {
        if (card.foodGroupCategory != currentActiveGoal)
        {
            return false;
        }

        if (currentGoalIndex == 2)
        {
            return card.glowFoodType == GlowFoodType.Fruit;
        }

        if (currentGoalIndex == 3)
        {
            return card.glowFoodType == GlowFoodType.Vegetable;
        }

        return true;
    }

    string GetCurrentGoalDescription()
    {
        switch (currentGoalIndex)
        {
            case 0: return "Go";
            case 1: return "Grow";
            case 2: return "Glow fruit";
            case 3: return "Glow vegetable";
            case 4: return "Water";
            default: return "Complete";
        }
    }

    RectTransform GetDestinationSlice(FoodCategory1 cat, int placedFoodCount)
    {
        switch (cat)
        {
            case FoodCategory1.Go: return goPlateSlice;
            case FoodCategory1.Grow: return growPlateSlice;
            case FoodCategory1.Glow:
                return placedFoodCount == 1 ? secondGlowPlateSlice : glowPlateSlice;
            case FoodCategory1.Water: return waterPlateSlice;
            default: return goPlateSlice;
        }
    }

    Vector3 GetFoodPlacementPosition(FoodCategory1 category, RectTransform target, int placedFoodCount)
    {
        if (category == FoodCategory1.Glow && placedFoodCount < 2)
        {
            return target.position;
        }

        if (category != FoodCategory1.Glow && placedFoodCount == 0)
        {
            return target.position;
        }

        int extraIndex = category == FoodCategory1.Glow
            ? placedFoodCount - 2
            : placedFoodCount - 1;
        int column = extraIndex % 3;
        int row = extraIndex / 3;
        float horizontalOffset = (column - 1) * 44f;
        float verticalOffset = -row * 44f;
        if (category == FoodCategory1.Glow)
        {
            horizontalOffset = (extraIndex % 2) * 44f;
            verticalOffset = -(extraIndex / 2 + 1) * 44f;
        }

        return target.position + new Vector3(horizontalOffset, verticalOffset, 0);
    }

    bool CreatePlateFoodDisplay(MagnetFoodCard card, Vector3 worldPosition)
    {
        Image cardImage = card.GetComponent<Image>();
        if (cardImage == null || cardImage.sprite == null)
        {
            Debug.LogError("Cannot display collected food on the plate: the food card has no Image sprite.");
            return false;
        }

        GameObject foodDisplay = new GameObject(
            "Collected " + card.gameObject.name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        RectTransform displayRect = foodDisplay.GetComponent<RectTransform>();
        displayRect.SetParent(uiCanvas.transform, false);
        displayRect.anchorMin = new Vector2(0.5f, 0.5f);
        displayRect.anchorMax = new Vector2(0.5f, 0.5f);
        displayRect.pivot = new Vector2(0.5f, 0.5f);
        displayRect.sizeDelta = cardImage.rectTransform.rect.size * 2f;
        displayRect.position = worldPosition;

        Image displayImage = foodDisplay.GetComponent<Image>();
        displayImage.sprite = cardImage.sprite;
        displayImage.color = cardImage.color;
        displayImage.preserveAspect = true;
        displayImage.raycastTarget = false;
        displayRect.SetAsLastSibling();
        plateFoodDisplays.Add(foodDisplay);
        return true;
    }
}
