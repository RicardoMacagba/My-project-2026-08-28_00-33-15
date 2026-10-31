using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlateCatcherManager : MonoBehaviour
{
    public static PlateCatcherManager Instance;

    [Header("Timer Settings")]
    public float timeRemaining = 60f; // 1 minute game time
    public Text timerText; // Drag your TIMER text component here

    [Header("Goal Targets")]
    public int targetGlow = 2;
    public int targetGo = 1;
    public int targetGrow = 1;
    public int targetWater = 1;

    [Header("Goal UI Text Elements")]
    public Text glowTextDisplay;
    public Text goTextDisplay;
    public Text growTextDisplay;
    public Text waterTextDisplay;

    private int glowCount, goCount, growCount, waterCount;
    private bool isGameOver = false;

    public bool IsGameOver => isGameOver;

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
        UpdateGoalUI();
        DisplayTime(timeRemaining);
    }

    void Update()
    {
        if (isGameOver) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            isGameOver = true;
            OnTimeExpired();
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void AddScore(FoodCategory category)
    {
        if (isGameOver) return;

        switch (category)
        {
            case FoodCategory.Glow: glowCount++; break;
            case FoodCategory.Go: goCount++; break;
            case FoodCategory.Grow: growCount++; break;
            case FoodCategory.Water: waterCount++; break;
        }

        UpdateGoalUI();
        EvaluateWinState();
    }

    public void PenaltyUnhealthyFood()
    {
        if (isGameOver) return;

        // Deduct 5 seconds from the clock for catching junk food!
        timeRemaining = Mathf.Max(0, timeRemaining - 5f);
        Debug.Log("💥 Caught junk food! -5 Seconds Penalty!");
    }

    void UpdateGoalUI()
    {
        if (glowTextDisplay) glowTextDisplay.text = $"{glowCount}/{targetGlow}";
        if (goTextDisplay) goTextDisplay.text = $"{goCount}/{targetGo}";
        if (growTextDisplay) growTextDisplay.text = $"{growCount}/{targetGrow}";
        if (waterTextDisplay) waterTextDisplay.text = $"{waterCount}/{targetWater}";
    }

    void EvaluateWinState()
    {
        if (glowCount >= targetGlow && goCount >= targetGo && 
            growCount >= targetGrow && waterCount >= targetWater)
        {
            isGameOver = true;
            Debug.Log("🎉 Victory! Goal fulfilled before timer ran out!");
            // Load next mini-game outro here
        }
    }

    void OnTimeExpired()
    {
        Debug.Log("❌ Out of time! Level Failed.");
        // Restart scene or show try again UI panel
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
