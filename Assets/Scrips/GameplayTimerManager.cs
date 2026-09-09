using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayTimerManager : MonoBehaviour
{
    [Header("Timer Configurations")]
    public float levelTimeLimit = 45f;      // Total seconds the child has to complete the level

    [Header("Debug / testing options")]
    public bool allowCurrentRunToOverrideSavedStars = true;

    private float timeRemaining;
    private bool isGameActive = false;
    private bool timerExpired = false;
    private bool levelFinished = false;
    private HeroCollisionTracker collisionTracker;

    void Start()
    {
        timeRemaining = levelTimeLimit;
        ResolveCollisionTracker();
    }

    private HeroCollisionTracker ResolveCollisionTracker()
    {
        if (collisionTracker == null)
        {
            collisionTracker = Object.FindFirstObjectByType<HeroCollisionTracker>();
            if (collisionTracker != null)
            {
                Debug.Log("✅ GameplayTimerManager resolved HeroCollisionTracker from the active scene.");
            }
        }

        return collisionTracker;
    }

    // Fired automatically by your HeroSelectionManager when the cinematic intro animation finishes!
    public void StartLevelCountdown()
    {
        ResolveCollisionTracker();
        isGameActive = true;
        Debug.Log($"⏱️ TIMER STARTED: You have {levelTimeLimit} seconds to complete your balanced diet!");
    }

    void Update()
    {
        if (!isGameActive || levelFinished) return;

        // 1. TICK DOWN THE CLOCK OVER TIME
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (Mathf.FloorToInt(timeRemaining) != Mathf.FloorToInt(timeRemaining + Time.deltaTime))
            {
                Debug.Log($"⏱️ TIMER: {timeRemaining:F1}s remaining");
            }
        }
        else
        {
            timeRemaining = 0f;

            if (!timerExpired)
            {
                timerExpired = true;
                Debug.Log("===== TIMER HIT ZERO =====");
                Debug.Log("⏱️ TIMER: 0.0s remaining - TIME EXPIRED. Saving current star value now, then continue until full collection or full fuel is reached.");

                HeroCollisionTracker tracker = ResolveCollisionTracker();
                if (tracker != null)
                {
                    Debug.Log("===== CALLING SaveStarsAtTimeout() =====");
                    tracker.SaveStarsAtTimeout();
                }
                else
                {
                    Debug.LogWarning("===== SaveStarsAtTimeout() could not run because collisionTracker is still null at timeout. Check that HeroCollisionTracker is active in the scene. =====");
                }
            }
        }
    }

    // Can be called instantly by your HeroCollisionTracker the moment they hit the win condition.
    public void ForceVictoryEvaluation()
    {
        if (levelFinished)
        {
            return;
        }

        if (!isGameActive)
        {
            Debug.Log("⚠️ ForceVictoryEvaluation called while timer was not active; still processing win flow.");
        }

        levelFinished = true;
        isGameActive = false;
        //EvaluateLevelPerformance(timerExpired);
    }

    public bool IsTimerExpired()
    {
        return timerExpired;
    }

    public int CalculateStarRatingFromFoodCount(int totalFoodEaten)
    {
        if (totalFoodEaten >= 5)
        {
            return 3;
        }
        if (totalFoodEaten >= 4)
        {
            return 2;
        }
        if (totalFoodEaten >= 2)
        {
            return 1;
        }

        return 0;
    }

    // private void EvaluateLevelPerformance(bool timedOut)
    // {
    //     HeroCollisionTracker tracker = ResolveCollisionTracker();
    //     if (tracker == null)
    //     {
    //         Debug.LogWarning("===== EvaluateLevelPerformance() aborted because HeroCollisionTracker could not be resolved. =====");
    //         return;
    //     }

    //     int totalFoodEaten = tracker.GetTotalValidFoodCount();
    //     int starRating = CalculateStarRatingFromFoodCount(totalFoodEaten);

    //     if (totalFoodEaten >= 5)
    //     {
    //         Debug.Log("⭐ 3 STARS! Full food collection reached.");
    //     }
    //     else if (totalFoodEaten >= 4)
    //     {
    //         Debug.Log("⭐ 2 STARS! 4 foods collected.");
    //     }
    //     else if (totalFoodEaten >= 2)
    //     {
    //         Debug.Log("⭐ 1 STAR! 2 foods collected.");
    //     }
    //     else
    //     {
    //         Debug.LogError("❌ 0 STARS! Not enough food collected.");
    //     }

    //     if (timedOut)
    //     {
    //         Debug.Log($"⏱️ TIMER EXPIRED, BUT THE FINAL STAR RESULT IS BASED ONLY ON FOOD COUNT: {totalFoodEaten} foods => {starRating} stars.");
    //     }

    //     SaveStarsForCurrentRun(totalFoodEaten);

    //     // Freeze the movement of foods, asteroids, and backgrounds cleanly
    //     DeactivateGameplayLoop();
    // }

    private void DeactivateGameplayLoop()
    {
        // Stop the sky spawner from cloning further items
        ObstacleSpawner spawner = Object.FindFirstObjectByType<ObstacleSpawner>();
        if (spawner != null) spawner.enabled = false;
        
        // Stop all background scrolling panels from sliding
        BackgroundScroller[] scrollers = Object.FindObjectsByType<BackgroundScroller>(FindObjectsSortMode.None);
        foreach (BackgroundScroller scroller in scrollers) scroller.enabled = false;
    }
}
