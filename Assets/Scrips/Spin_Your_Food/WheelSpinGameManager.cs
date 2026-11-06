using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WheelSpinGameManager : MonoBehaviour
{
    [System.Serializable]
    public class WheelSlice
    {
        public string foodName;
        public bool isHealthy; // True = Everyday Food, False = Enjoy Less Often
        public RectTransform foodIcon;
    }

    [Header("Wheel Configuration")]
    public RectTransform wheelSpinner; // Drag 'wheel_spinner' here
    public RectTransform wheelPointer;
    public WheelSlice[] slices;

    [Header("Spin Parameters")]
    public float spinDuration = 3.5f;
    public int minimumFullCircles = 5;
    // Animation curve to make the wheel start fast and decelerate slowly to a gentle stop
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI Control Buttons")]
    public Button spinButton;          // Drag 'button_spin' here
    public Button everydayFoodButton;  // Drag 'button_everyday_food' here
    public Button enjoyLessOftenButton; // Drag 'button_enjoy_less_oft' here

    [Header("Score Settings")]
    public WheelScoreManager scoreManager; // Drag your ScoreManager object here

    private bool isSpinning = false;
    private bool canAnswer = false;
    private WheelSlice selectedSlice;
    private int selectedSliceIndex = -1;
    private bool[] completedSlices;

    void Start()
    {
        if (wheelSpinner == null || wheelPointer == null || slices == null || slices.Length == 0)
        {
            Debug.LogError("WheelSpinGameManager requires a wheel, pointer, and at least one configured food slice.", this);
            enabled = false;
            return;
        }

        completedSlices = new bool[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            if (slices[i] == null || slices[i].foodIcon == null)
            {
                Debug.LogError("Every configured food slice must have a food icon assigned.", this);
                enabled = false;
                return;
            }
        }

        // Hook up listener actions to the UI buttons
        if (spinButton != null) spinButton.onClick.AddListener(StartSpin);
        if (everydayFoodButton != null) everydayFoodButton.onClick.AddListener(() => CheckAnswer(true));
        if (enjoyLessOftenButton != null) enjoyLessOftenButton.onClick.AddListener(() => CheckAnswer(false));

        // Start with the categorization buttons locked
        ToggleAnswerButtons(false);
        if (spinButton != null) spinButton.interactable = true;
    }

    public void StartSpin()
    {
        if (isSpinning || HasNoRemainingFoods()) return;
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        selectedSliceIndex = GetRandomRemainingSliceIndex();
        if (selectedSliceIndex < 0)
        {
            CompleteGame();
            yield break;
        }

        selectedSlice = slices[selectedSliceIndex];
        isSpinning = true;
        canAnswer = false;
        
        // Lock EVERYTHING mid-spin so children can't double-click or break sequence loops
        ToggleAnswerButtons(false);
        if (spinButton != null) spinButton.interactable = false;

        float elapsed = 0f;
        float startingAngle = wheelSpinner.localEulerAngles.z;
        Vector3 pointerDirection = wheelSpinner.InverseTransformPoint(wheelPointer.position);
        Vector3 foodDirection = wheelSpinner.InverseTransformPoint(selectedSlice.foodIcon.position);
        float pointerAngle = Mathf.Atan2(pointerDirection.y, pointerDirection.x) * Mathf.Rad2Deg;
        float foodAngle = Mathf.Atan2(foodDirection.y, foodDirection.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Repeat(startingAngle + Mathf.DeltaAngle(foodAngle, pointerAngle), 360f);
        float clockwiseRotationToTarget = Mathf.Repeat(startingAngle - targetAngle, 360f);
        float totalRotationDegrees = (minimumFullCircles * 360f) + clockwiseRotationToTarget;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / spinDuration;
            
            float currentEvaluation = spinCurve.Evaluate(percent);
            // Rotate backwards (clockwise) to match standard wheel directions
            float nextAngle = Mathf.Lerp(startingAngle, startingAngle - totalRotationDegrees, currentEvaluation);

            wheelSpinner.localEulerAngles = new Vector3(0, 0, nextAngle);
            yield return null;
        }

        // Clean up final tracking angle to sit perfectly inside a 0-360 loop matrix
        float finalNormalizedAngle = Mathf.Repeat(startingAngle - totalRotationDegrees, 360f);
        wheelSpinner.localEulerAngles = new Vector3(0, 0, finalNormalizedAngle);

        isSpinning = false;
        DetermineLandedSlice();
    }

    void DetermineLandedSlice()
    {
        if (selectedSlice != null)
        {
            Debug.Log("Landed on: " + selectedSlice.foodName);
            canAnswer = true;
            ToggleAnswerButtons(true); // Unlock choice buttons for the kid!
        }
        else
        {
            if (spinButton != null) spinButton.interactable = true;
        }
    }

    void CheckAnswer(bool playerChoseHealthy)
    {
        if (!canAnswer || selectedSlice == null) return;

        if (playerChoseHealthy == selectedSlice.isHealthy)
        {
            Debug.Log("Correct!");
            if (scoreManager != null) scoreManager.AddScore(10);

            completedSlices[selectedSliceIndex] = true;
            bool hasRemainingFoods = !HasNoRemainingFoods();
            ResetTurnState(hasRemainingFoods);
            if (!hasRemainingFoods) CompleteGame();
        }
        else
        {
            Debug.Log("Incorrect! Try spinning again.");
            if (scoreManager != null) scoreManager.AddScore(-5); // Optional penalty point deduction
            
            ResetTurnState(true);
        }
    }

    int GetRandomRemainingSliceIndex()
    {
        List<int> remainingIndices = new List<int>();
        for (int i = 0; i < slices.Length; i++)
        {
            if (!completedSlices[i]) remainingIndices.Add(i);
        }

        return remainingIndices.Count > 0
            ? remainingIndices[Random.Range(0, remainingIndices.Count)]
            : -1;
    }

    bool HasNoRemainingFoods()
    {
        for (int i = 0; i < completedSlices.Length; i++)
        {
            if (!completedSlices[i]) return false;
        }

        return true;
    }

    void CompleteGame()
    {
        canAnswer = false;
        ToggleAnswerButtons(false);
        if (spinButton != null) spinButton.interactable = false;
        Debug.Log("All foods have been categorized correctly.");
    }

    void ResetTurnState(bool allowSpinAgain)
    {
        canAnswer = false;
        ToggleAnswerButtons(false);
        if (spinButton != null) spinButton.interactable = allowSpinAgain;
    }

    void ToggleAnswerButtons(bool interactiveState)
    {
        if (everydayFoodButton != null) everydayFoodButton.interactable = interactiveState;
        if (enjoyLessOftenButton != null) enjoyLessOftenButton.interactable = interactiveState;
    }
}
