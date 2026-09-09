using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody2D))]
public class HeroCollisionTracker : MonoBehaviour
{
    [Header("Fuel Bar UI Mapping")]
    public Slider fuelSliderBar;       
    public float startingFuel = 30f;   // Starts a bit lower so rewards feel impactful
    public float maxFuel = 100f;       

    [Header("Mechanics Values")]
    public float foodRewardAmount = 10f;    // Points added per valid food item eaten
    public float asteroidPenaltyAmount = 15f; // Points deducted when hitting an asteroid (-3 on the bar)

    [Header("Balanced Diet Tracking Tracker (Max 1 Each)")]
    // This tracks the exact number of times each unique food file is collected
    private int breadCount = 0;
    private int cornCount = 0;
    private int spaghetyCount = 0;
    private int riceCount = 0;
    private int potatoCount = 0;

    private const int maxPerFoodType = 1;
    private const int totalFoodTypes = 5;
    private const int fullFuelCollectionGoal = maxPerFoodType * totalFoodTypes;

    private float currentFuel;
    private bool starsLockedAfterTimeout = false;

    void Start()
    {
        currentFuel = startingFuel;

        if (fuelSliderBar != null)
        {
            fuelSliderBar.maxValue = maxFuel;
            fuelSliderBar.value = currentFuel;
        }

        Debug.Log("🥦 NUTRITION CORE READY: Collect 1 of each food type to complete the goal!");
    }

    // 🍏 TRIGGER CHECK: Handles passing through your Healthy Go Foods!
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectFood(other.gameObject);
    }

    // 🪨 COLLISION CHECK: Handles crashing into solid Asteroid Hazards!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == null)
        {
            return;
        }

        if (IsAsteroid(collision.gameObject))
        {
            // Deducts the penalty value (-3 units from your system balance design)
            ModifyFuelValue(-asteroidPenaltyAmount);

            Debug.LogWarning($"🪨 OUCH! Hit asteroid: {collision.gameObject.name}. Penalizing fuel meter bar.");

            // Break apart the obstacle rock so the road path clears up
            Destroy(collision.gameObject);
            return;
        }

        TryCollectFood(collision.gameObject);
    }

    private void TryCollectFood(GameObject foodObject)
    {
        if (foodObject == null || !IsFood(foodObject))
        {
            return;
        }

        Debug.Log($"✅ Food consumed by Gameplay_Player_Hero: {foodObject.name}");

        // Convert the name to completely lowercase so capitalization typos can never break our code!
        string foodName = foodObject.name.ToLower();
        bool scoreAdded = false;

        // 2. CHECK INDIVIDUAL TYPE KEYWORDS (Bulletproof Lowercase Matching)
        if (foodName.Contains("bread"))
        {
            if (breadCount < 1) { breadCount++; scoreAdded = true; Debug.Log($"� Bread Count: {breadCount}/1"); }
            else { Debug.Log("❌ Bread limit reached! Eat other items for a balanced diet!"); }
        }
        else if (foodName.Contains("corn"))
        {
            if (cornCount < 1) { cornCount++; scoreAdded = true; Debug.Log($"🌽 Corn Count: {cornCount}/1"); }
            else { Debug.Log("❌ Corn limit reached! Find something else!"); }
        }
        else if (foodName.Contains("spaghety"))
        {
            if (spaghetyCount < 1) { spaghetyCount++; scoreAdded = true; Debug.Log($"� Spaghety Count: {spaghetyCount}/1"); }
            else { Debug.Log("❌ Spaghety limit reached!"); }
        }
        else if (foodName.Contains("rice"))
        {
            if (riceCount < 1) { riceCount++; scoreAdded = true; Debug.Log($"🍚 Rice Count: {riceCount}/1"); }
            else { Debug.Log("❌ Rice limit reached!"); }
        }
        else if (foodName.Contains("potato"))
        {
            if (potatoCount < 1) { potatoCount++; scoreAdded = true; Debug.Log($"🥔 Potato Count: {potatoCount}/1"); }
            else { Debug.Log("❌ Potato limit reached!"); }
        }

        // 3. IF THE COUNT WAS UNDER 3, AWARD THE FUEL METERS POINTS!
        if (scoreAdded)
        {
            UpdateFuelFromCollectionProgress();
        }

        // Always destroy the item asset so it clears out of the sky channel runway path
        Destroy(foodObject);
    }

    private bool IsFood(GameObject foodObject)
    {
        if (foodObject == null)
        {
            return false;
        }

        string objectName = foodObject.name ?? string.Empty;
        string normalizedName = objectName.Replace(" ", string.Empty);

        return foodObject.CompareTag("Food")
            || objectName.IndexOf("Prefab_Bread", StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("Prefab_Corn", StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("Prefab_Spaghety", StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("Prefab_Rice", StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("Prefab_Potato", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("bread", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("corn", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("spaghety", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("rice", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("potato", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool IsAsteroid(GameObject hazardObject)
    {
        if (hazardObject == null)
        {
            return false;
        }

        string objectName = hazardObject.name ?? string.Empty;
        string normalizedName = objectName.Replace(" ", string.Empty);

        return hazardObject.CompareTag("Asteroid")
            || objectName.IndexOf("Prefab_Rock", StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("Prefab_Icecream", StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("Prefab_Fries", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("icecream", StringComparison.OrdinalIgnoreCase) >= 0
            || normalizedName.IndexOf("fries", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void UpdateFuelFromCollectionProgress()
    {
        int totalCollected = breadCount + cornCount + spaghetyCount + riceCount + potatoCount;
        float progress = totalCollected / (float)fullFuelCollectionGoal;
        currentFuel = Mathf.Lerp(startingFuel, maxFuel, progress);

        if (fuelSliderBar != null)
        {
            fuelSliderBar.value = currentFuel;
        }

        Debug.Log($"⚡ Fuel progress: {totalCollected}/{fullFuelCollectionGoal} ({progress * 100f:0}%) | currentFuel={currentFuel}");

        if (totalCollected >= fullFuelCollectionGoal)
        {
            currentFuel = maxFuel;
            if (fuelSliderBar != null)
            {
                fuelSliderBar.value = currentFuel;
            }
            Debug.Log("✅ FULL FUEL! All 5 food types are complete at 1 each.");
            TriggerWinIfGoalReached();
        }
    }

    public void SaveStarsAtTimeout()
    {
        int totalCollected = GetTotalValidFoodCount();
        Debug.Log($"===== SaveStarsAtTimeout() called. totalCollected={totalCollected} =====");
        
        int starRating = GetStarCountFromCollectedFoods(totalCollected);
        int previousSaved = PlayerPrefs.GetInt("FlyingHero_HighScoreStars", 0);
        
        PlayerPrefs.SetInt("FlyingHero_HighScoreStars", starRating);
        PlayerPrefs.Save();
        
        starsLockedAfterTimeout = true;
        Debug.Log($"⏱️ TIME-UP STAR SAVE: timer expired with {totalCollected} foods => {starRating} stars. Previous value {previousSaved} was replaced. Star value is now locked for the remainder of this run.");
    }

    private int GetStarCountFromCollectedFoods(int totalCollected)
    {
        if (totalCollected >= 5)
        {
            return 3;
        }
        if (totalCollected >= 4)
        {
            return 2;
        }
        if (totalCollected >= 2)
        {
            return 1;
        }

        return 0;
    }

    private void ModifyFuelValue(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);

        if (fuelSliderBar != null)
        {
            fuelSliderBar.value = currentFuel;
        }

        if (currentFuel <= 0f)
        {
            Debug.LogError("💀 GAME OVER! The hero ran out of energy!");
        }

        if (fuelSliderBar != null && fuelSliderBar.value >= fuelSliderBar.maxValue)
        {
            TriggerWinIfGoalReached();
        }
    }

    private void TriggerWinIfGoalReached()
    {
        bool totalGoalReached = GetTotalValidFoodCount() >= fullFuelCollectionGoal;
        bool fuelBarFull = fuelSliderBar != null && fuelSliderBar.value >= fuelSliderBar.maxValue;

        if (totalGoalReached || fuelBarFull)
        {
            Debug.Log($"✅ WIN CONDITION MET: totalCollected={GetTotalValidFoodCount()}/{fullFuelCollectionGoal}, fuel={currentFuel}/{maxFuel}");
            Debug.Log($"⏱️ Stars locked after timeout: {starsLockedAfterTimeout}. (Stars only saved when timer expires via SaveStarsAtTimeout())");

            GameplayTimerManager timerManager = UnityEngine.Object.FindFirstObjectByType<GameplayTimerManager>();
            if (timerManager != null)
            {
                timerManager.ForceVictoryEvaluation();
            }

            SceneTracker.NextSceneName = "GoFoodsSubCategory";
            SceneManager.LoadScene("LoadingScene");
        }
    }

    // 🚨 GAMEWAY HOOK FOR THE TIMER:
    // This adds up your unique food counters so the clock knows your total score!
    public int GetTotalValidFoodCount()
    {
        return breadCount + cornCount + spaghetyCount + riceCount + potatoCount;
    }
}
