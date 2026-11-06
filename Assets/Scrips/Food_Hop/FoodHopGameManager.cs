using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.EventSystems;
using System.Collections;

public class FoodHopGameManager : MonoBehaviour
{
    [System.Serializable]
    public class FoodItem
    {
        public string foodName;
        public GameObject foodObject; // The Button or Clickable UI Food Object
        public bool isHealthy;       // True = Everyday, False = Enjoy Less Often
        [HideInInspector] public Vector3 originalFoodPosition;
    }

    [Header("Game Sequence")]
    public FoodItem[] foodSequence;
    private int currentIndex = -1; // -1 means starting at the initial position
    private Vector3 initialCharacterPosition;
    private bool isMoving = false;
    private bool standardSelectionAllowed = false;

    [Header("Visual Asset Groups")]
    public GameObject idleGroup;  // Drag 'idle_pos' here
    public GameObject jumpGroup;  // Drag 'jump_pos' here
    private Transform idleCharacterAnchor;
    private Transform jumpCharacterAnchor;
    
    [Header("Movement Settings")]
    public float jumpHeight = 120f; // Curve peak adjustment for UI space
    public float jumpDuration = 0.4f;

    [Header("UI Control Buttons")]
    public Button everydayFoodButton;
    public Button enjoyLessOftenButton;

    [Header("Timer Settings")]
    public Text timerText; // Link your UI Text component clock layout here
    public float totalTimeLeft = 60f; // Total game round length in seconds
    private bool gameEnded = false;

    void Start()
    {
        // Bind categorizing button action listeners
        if (everydayFoodButton != null) everydayFoodButton.onClick.AddListener(() => VerifyChoice(true));
        if (enjoyLessOftenButton != null) enjoyLessOftenButton.onClick.AddListener(() => VerifyChoice(false));

        // Initial image layout states
        if (idleGroup != null) idleGroup.SetActive(true);
        if (jumpGroup != null) jumpGroup.SetActive(false);

        if (idleGroup != null)
            idleCharacterAnchor = idleGroup.transform.Find("body");
        if (jumpGroup != null)
            jumpCharacterAnchor = jumpGroup.transform.Find("body");

        if (idleCharacterAnchor == null || jumpCharacterAnchor == null)
        {
            Debug.LogError("FoodHopGameManager requires a child named 'body' under both character groups.", this);
            enabled = false;
            return;
        }

        initialCharacterPosition = idleCharacterAnchor.position;

        // Hook up individual click listeners to the Food objects dynamically
        for (int i = 0; i < foodSequence.Length; i++)
        {
            if (foodSequence[i].foodObject != null)
            {
                foodSequence[i].originalFoodPosition = foodSequence[i].foodObject.transform.position;
                
                int indexClosure = i; // Cache step loop index
                Button foodClickButton = foodSequence[i].foodObject.GetComponent<Button>();
                if (foodClickButton != null)
                {
                    foodClickButton.onClick.AddListener(() => OnFoodTapped(indexClosure));
                }
            }
        }
    }

    void Update()
    {
        if (gameEnded) return;

        // Process active background countdown clock
        if (totalTimeLeft > 0)
        {
            totalTimeLeft -= Time.deltaTime;
            if (timerText != null)
                timerText.text = "Time: " + Mathf.CeilToInt(totalTimeLeft).ToString() + "s";
        }
        else
        {
            TriggerGameOver(false);
        }
    }

    void OnFoodTapped(int foodIndex)
    {
        // Enforce sequence progression layout order (Player must tap next logical target item)
        if (isMoving || gameEnded || foodIndex != currentIndex + 1) return;

        currentIndex = foodIndex;
        Vector3 landingSpot = foodSequence[currentIndex].foodObject.transform.position;
        landingSpot.y += 40f; // Offset character visually so they rest directly above the item platform

        StartCoroutine(JumpRoutine(landingSpot, true));
    }

    void VerifyChoice(bool playerSelectedHealthy)
    {
        if (isMoving || gameEnded || !standardSelectionAllowed || currentIndex < 0) return;

        FoodItem currentFood = foodSequence[currentIndex];

        if (playerSelectedHealthy == currentFood.isHealthy)
        {
            Debug.Log("Correct Categorization: " + currentFood.foodName);
            standardSelectionAllowed = false; // Lock selection until next platform reached

            if (currentIndex == foodSequence.Length - 1)
            {
                TriggerGameOver(true);
            }
        }
        else
        {
            Debug.Log("Wrong Choice! Applying penalty consequence...");
            StartCoroutine(HandleFailureConsequence());
        }
    }

    IEnumerator HandleFailureConsequence()
    {
        isMoving = true;
        standardSelectionAllowed = false;

        // 1. Calculate time penalty deduction
        totalTimeLeft = Mathf.Max(0f, totalTimeLeft - 5f);
        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(totalTimeLeft).ToString() + "s";

        // 2. Identify the retreat return point coordinates
        Vector3 rollbackCharacterPos = (currentIndex - 1 >= 0) 
            ? foodSequence[currentIndex - 1].foodObject.transform.position + new Vector3(0, 40f, 0)
            : initialCharacterPosition;

        // 3. Kick off paired rollback jumps (Character moving back, item shifting out)
        GameObject currentFoodObj = foodSequence[currentIndex].foodObject;
        Vector3 displacedFoodPos = currentFoodObj.transform.position - new Vector3(0, 100f, 0); // Displace item downwards slightly

        // Animate retreat sequence
        yield return StartCoroutine(JumpRoutine(rollbackCharacterPos, false));
        
        // Snap the shifted food back to its starting home base 
        if (currentFoodObj != null)
            currentFoodObj.transform.position = foodSequence[currentIndex].originalFoodPosition;

        // Step index index back down one slot
        currentIndex--; 
        isMoving = false;
    }

    IEnumerator JumpRoutine(Vector3 targetPos, bool clearToAnswerAfter)
    {
        isMoving = true;
        Vector3 startPos = idleCharacterAnchor.position;
        MoveGroupAnchorTo(jumpGroup.transform, jumpCharacterAnchor, startPos);
        idleGroup.SetActive(false);
        jumpGroup.SetActive(true);

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / jumpDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;

            MoveGroupAnchorTo(jumpGroup.transform, jumpCharacterAnchor, currentPos);

            yield return null;
        }

        MoveGroupAnchorTo(jumpGroup.transform, jumpCharacterAnchor, targetPos);
        MoveGroupAnchorTo(idleGroup.transform, idleCharacterAnchor, targetPos);

        jumpGroup.SetActive(false);
        idleGroup.SetActive(true);
        
        isMoving = false;
        if (clearToAnswerAfter) standardSelectionAllowed = true;
    }

    static void MoveGroupAnchorTo(Transform group, Transform anchor, Vector3 targetPosition)
    {
        group.position += targetPosition - anchor.position;
    }

    void TriggerGameOver(bool won)
    {
        gameEnded = true;
        if (won)
            Debug.Log("Winner! You categorized all items correctly before the clock ran out!");
        else
            Debug.Log("Game Over! Time ran out.");
    }
}
