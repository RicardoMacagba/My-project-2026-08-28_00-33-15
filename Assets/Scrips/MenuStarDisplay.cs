using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuStarDisplay : MonoBehaviour
{
    [Header("Visual Star Image Components")]
    public Image star1Image;
    public Image star2Image;
    public Image star3Image;

    [Header("Star Sprites Assets Pool")]
    public Sprite emptyStarSprite;   // Drag your "Star 29 (1)" white asset here
    public Sprite glowingStarSprite; // Drag your "Star 29" yellow asset here

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshMenuStars();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshMenuStars();
    }

    public void RefreshMenuStars()
    {
        if (star1Image == null || star2Image == null || star3Image == null)
        {
            Debug.LogWarning("⭐ MenuStarDisplay missing one or more Image references. Assign star1Image, star2Image, and star3Image in the Inspector.");
            return;
        }

        // Reads your saved high-score star count from Unity's secure PlayerPrefs memory system register
        int savedStars = PlayerPrefs.GetInt("FlyingHero_HighScoreStars", 0);
        savedStars = Mathf.Clamp(savedStars, 0, 3);

        if (emptyStarSprite == glowingStarSprite)
        {
            Debug.LogWarning("⚠️ MenuStarDisplay: emptyStarSprite and glowingStarSprite are the same asset. Assign a different empty white star and glowing yellow star sprite.");
        }

        Debug.Log($"🧪 STAR READ: PlayerPrefs key FlyingHero_HighScoreStars = {savedStars}");

        // Default all stars back to the empty white outline look
        if (star1Image != null) star1Image.sprite = emptyStarSprite;
        if (star2Image != null) star2Image.sprite = emptyStarSprite;
        if (star3Image != null) star3Image.sprite = emptyStarSprite;

        // Turn on the golden star textures dynamically based on how well the child played the flying game!
        if (savedStars >= 1 && star1Image != null) star1Image.sprite = glowingStarSprite;
        if (savedStars >= 2 && star2Image != null) star2Image.sprite = glowingStarSprite;
        if (savedStars >= 3 && star3Image != null) star3Image.sprite = glowingStarSprite;

        int actualGlowingStars = 0;
        if (star1Image.sprite == glowingStarSprite) actualGlowingStars++;
        if (star2Image.sprite == glowingStarSprite) actualGlowingStars++;
        if (star3Image.sprite == glowingStarSprite) actualGlowingStars++;

        Debug.Log($"⭐ BLACKBOARD REFRESH: expectedSavedStars={savedStars}, actualGlowingStars={actualGlowingStars}. Visual sprites matched expected value: {actualGlowingStars == savedStars}");
    }

    // Testing helper: clears the star save so you can retest multiple runs quickly.
    public void ResetSavedStarsForTesting()
    {
        PlayerPrefs.DeleteKey("FlyingHero_HighScoreStars");
        PlayerPrefs.Save();
        RefreshMenuStars();
        Debug.Log("🧪 TEST RESET: FlyingHero_HighScoreStars was cleared from PlayerPrefs.");
    }

    // Testing helper: force the star display to read the latest saved value immediately.
    public void ForceRefreshForTesting()
    {
        RefreshMenuStars();
        Debug.Log("🧪 TEST REFRESH: Forced MenuStarDisplay to re-read PlayerPrefs.");
    }
}
