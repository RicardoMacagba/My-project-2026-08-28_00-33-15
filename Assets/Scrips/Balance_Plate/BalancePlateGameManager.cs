using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum WeightCategory { TooLittle, Balance, TooMuch }

public class BalancePlateGameManager : MonoBehaviour
{
    [Header("UI Goal Text Configuration")]
    public Text goalTextDisplay;

    [Header("Timer and Score")]
    public Text timerTextDisplay;
    public Text scoreTextDisplay;
    public float startingTimeSeconds = 60f;
    public float incorrectAnswerPenaltySeconds = 10f;
    [Min(0)] public int tooMuchGoalTarget = 2;
    [Min(0)] public int balanceGoalTarget = 1;
    [Min(0)] public int tooLittleGoalTarget = 2;
    public Text tooMuchGoalProgressText;
    public Text balanceGoalProgressText;
    public Text tooLittleGoalProgressText;
    private float timeRemaining;
    private int score;
    private int tooMuchGoalsAchieved;
    private int balanceGoalsAchieved;
    private int tooLittleGoalsAchieved;
    private bool isGameOver;
    
    [Header("Swaying Meter Pointer Setup")]
    public RectTransform meterPointer;
    public float maxTopYLimit = 180f;     // Top edge of the Red Zone
    public float maxBottomYLimit = -180f; // Bottom edge of the Blue Zone
    public float pointerSwaySpeed = 3f;   // How fast the needle sweeps up and down

    [Header("Scale Zone Markers")]
    public RectTransform tooMuchMarker;
    public RectTransform balanceMarker;
    public RectTransform tooLittleMarker;

    [Header("Pointer Control")]
    public Button stopPointerButton;

    [Header("Plate Spawn Animation Nodes")]
    public Transform scaleAnchorPoint; 
    public Transform entryStartPoint;  
    public Transform exitEndPoint;     
    public float animationDuration = 0.5f;

    [Header("Food Plates Configuration Pool")]
    public GameObject tooMuchPlatePrefab;
    public GameObject balancePlatePrefab;
    public GameObject tooLittlePlatePrefab;
    private GameObject currentActivePlate;
    private GameObject currentPlatePrefab;

    private WeightCategory currentPlateCategory;
    private bool isPointerSwaying = false;
    private bool shouldEvaluateStoppedPointer;
    private float swayTimeFactor = 0f;

    void Start()
    {
        if (meterPointer == null || meterPointer.parent == null ||
            tooMuchMarker == null || balanceMarker == null || tooLittleMarker == null ||
            tooMuchPlatePrefab == null || balancePlatePrefab == null || tooLittlePlatePrefab == null)
        {
            Debug.LogError("BalancePlateGameManager requires the pointer, all scale markers, and all three categorized plate prefabs.", this);
            enabled = false;
            return;
        }

        timeRemaining = startingTimeSeconds;
        UpdateTimerText();
        UpdateScoreText();
        UpdateGoalCountTexts();

        if (stopPointerButton != null) stopPointerButton.onClick.AddListener(OnStopPointerButtonClicked);

        StartNewRoundSystem();
    }

    void Update()
    {
        if (!isGameOver)
        {
            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            UpdateTimerText();

            if (timeRemaining <= 0f)
            {
                EndGame();
            }
        }

        // Smoothly oscillate the needle up and down using a sine wave if active
        if (isPointerSwaying && meterPointer != null)
        {
            swayTimeFactor += Time.deltaTime * pointerSwaySpeed;
            // Mathf.PingPong or Sinewave moves values cleanly back and forth between 0 and 1
            float t = (Mathf.Sin(swayTimeFactor) + 1f) / 2f; 
            
            Vector2 pos = meterPointer.anchoredPosition;
            pos.y = Mathf.Lerp(maxBottomYLimit, maxTopYLimit, t);
            meterPointer.anchoredPosition = pos;
        }

        if (shouldEvaluateStoppedPointer && !isPointerSwaying && !isGameOver)
        {
            shouldEvaluateStoppedPointer = false;
            EvaluateStoppedPointer();
        }
    }

    void StartNewRoundSystem()
    {
        if (isGameOver) return;

        List<WeightCategory> remainingCategories = GetRemainingGoalCategories();
        if (remainingCategories.Count == 0)
        {
            CompleteGoal();
            return;
        }

        currentPlateCategory = remainingCategories[Random.Range(0, remainingCategories.Count)];
        currentPlatePrefab = GetPlatePrefab(currentPlateCategory);
        UpdateGoalTextGraphic();

        StartCoroutine(AnimatePlateTransitionIn());
    }

    void UpdateGoalTextGraphic()
    {
        if (goalTextDisplay == null) return;

        string categoryName;
        switch (currentPlateCategory)
        {
            case WeightCategory.TooMuch: categoryName = "TOO MUCH (RED)"; break;
            case WeightCategory.Balance: categoryName = "BALANCE (GREEN)"; break;
            case WeightCategory.TooLittle: categoryName = "TOO LITTLE (BLUE)"; break;
            default: return;
        }

        goalTextDisplay.text = $"PLATE: {categoryName}\nStop at its matching scale color.";
    }

    void UpdateGoalCountTexts()
    {
        UpdateGoalCountText(tooMuchGoalProgressText, tooMuchGoalsAchieved, tooMuchGoalTarget);
        UpdateGoalCountText(balanceGoalProgressText, balanceGoalsAchieved, balanceGoalTarget);
        UpdateGoalCountText(tooLittleGoalProgressText, tooLittleGoalsAchieved, tooLittleGoalTarget);
    }

    void UpdateGoalCountText(Text display, int achieved, int target)
    {
        if (display != null)
        {
            display.text = $"{Mathf.Min(achieved, target)}/{target}";
        }
    }

    List<WeightCategory> GetRemainingGoalCategories()
    {
        List<WeightCategory> remaining = new List<WeightCategory>(3);
        if (tooMuchGoalsAchieved < tooMuchGoalTarget) remaining.Add(WeightCategory.TooMuch);
        if (balanceGoalsAchieved < balanceGoalTarget) remaining.Add(WeightCategory.Balance);
        if (tooLittleGoalsAchieved < tooLittleGoalTarget) remaining.Add(WeightCategory.TooLittle);
        return remaining;
    }

    GameObject GetPlatePrefab(WeightCategory category)
    {
        switch (category)
        {
            case WeightCategory.TooMuch: return tooMuchPlatePrefab;
            case WeightCategory.Balance: return balancePlatePrefab;
            case WeightCategory.TooLittle: return tooLittlePlatePrefab;
            default: return null;
        }
    }

    void UpdateTimerText()
    {
        if (timerTextDisplay == null) return;

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        timerTextDisplay.text = $"TIME: {totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    void UpdateScoreText()
    {
        if (scoreTextDisplay != null)
        {
            scoreTextDisplay.text = $"SCORE: {score}";
        }
    }

    void EndGame()
    {
        isGameOver = true;
        isPointerSwaying = false;
        if (timerTextDisplay != null)
        {
            timerTextDisplay.text = "TIME: 00:00";
        }

        if (stopPointerButton != null) stopPointerButton.interactable = false;
    }

    void OnStopPointerButtonClicked()
    {
        if (!isGameOver && isPointerSwaying)
        {
            isPointerSwaying = false;
        }
    }

    void EvaluateStoppedPointer()
    {
        WeightCategory detectedPointerZone = EvaluatePointerCurrentZone();

        if (detectedPointerZone == currentPlateCategory)
        {
            score++;
            IncrementCategoryGoal(currentPlateCategory);
            UpdateScoreText();
            UpdateGoalCountTexts();

            if (GetRemainingGoalCategories().Count == 0)
            {
                CompleteGoal();
            }

            Debug.Log($"CORRECT! {currentPlateCategory} plate matched the {detectedPointerZone} scale zone.");
        }
        else
        {
            timeRemaining = Mathf.Max(0f, timeRemaining - incorrectAnswerPenaltySeconds);
            UpdateTimerText();
            Debug.Log($"MISSED! {currentPlateCategory} plate stopped in the {detectedPointerZone} scale zone.");

            if (timeRemaining <= 0f)
            {
                EndGame();
            }
        }

        // STEP 4: Cycle out old plate and advance stage round
        StartCoroutine(CycleRoundSequenceCleanly());
    }

    void IncrementCategoryGoal(WeightCategory category)
    {
        switch (category)
        {
            case WeightCategory.TooMuch: tooMuchGoalsAchieved++; break;
            case WeightCategory.Balance: balanceGoalsAchieved++; break;
            case WeightCategory.TooLittle: tooLittleGoalsAchieved++; break;
        }
    }

    WeightCategory EvaluatePointerCurrentZone()
    {
        Transform pointerParent = meterPointer.parent;
        float currentPointerY = meterPointer.localPosition.y;
        float redDistance = Mathf.Abs(currentPointerY - pointerParent.InverseTransformPoint(tooMuchMarker.position).y);
        float greenDistance = Mathf.Abs(currentPointerY - pointerParent.InverseTransformPoint(balanceMarker.position).y);
        float blueDistance = Mathf.Abs(currentPointerY - pointerParent.InverseTransformPoint(tooLittleMarker.position).y);

        if (redDistance <= greenDistance && redDistance <= blueDistance) return WeightCategory.TooMuch;
        if (greenDistance <= blueDistance) return WeightCategory.Balance;
        return WeightCategory.TooLittle;
    }

    void CompleteGoal()
    {
        isGameOver = true;
        isPointerSwaying = false;
        if (goalTextDisplay != null)
        {
            goalTextDisplay.text = "GOAL ACHIEVED! All plate targets reached.";
        }
        if (stopPointerButton != null) stopPointerButton.interactable = false;
    }

    IEnumerator AnimatePlateTransitionIn()
    {
        if (currentPlatePrefab != null)
        {
            currentActivePlate = Instantiate(currentPlatePrefab, entryStartPoint.position, Quaternion.identity, scaleAnchorPoint);
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                currentActivePlate.transform.position = Vector3.Lerp(entryStartPoint.position, scaleAnchorPoint.position, elapsed / animationDuration);
                yield return null;
            }
            currentActivePlate.transform.position = scaleAnchorPoint.position;
        }

        if (!isGameOver)
        {
            // Once the tray lands securely on the weighing base, activate the scale pointer.
            shouldEvaluateStoppedPointer = true;
            isPointerSwaying = true;
        }
    }

    IEnumerator CycleRoundSequenceCleanly()
    {
        yield return new WaitForSeconds(1.0f); // Leave stopped needle visible for feedback momentarily

        if (currentActivePlate != null)
        {
            Vector3 startPos = currentActivePlate.transform.position;
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                currentActivePlate.transform.position = Vector3.Lerp(startPos, exitEndPoint.position, elapsed / animationDuration);
                yield return null;
            }
            Destroy(currentActivePlate);
        }

        // Start next game step loop configuration setup
        StartNewRoundSystem();
    }
}
