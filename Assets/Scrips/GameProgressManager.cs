using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    // =========================================================
    // STAR KEYS
    // =========================================================

    public const string Game1StarsKey = "FlyingHero_HighScoreStars";
    public const string Game2StarsKey = "HelpingStudent_HighScoreStars";
    public const string Game3StarsKey = "Game3_HighScoreStars";
    public const string Game4StarsKey = "Game4_HighScoreStars";
    public const string Game5StarsKey = "Game5_HighScoreStars";


    // =========================================================
    // GET SAVED STARS
    // =========================================================

    public int GetGame1Stars()
    {
        return PlayerPrefs.GetInt(Game1StarsKey, 0);
    }

    public int GetGame2Stars()
    {
        return PlayerPrefs.GetInt(Game2StarsKey, 0);
    }

    public int GetGame3Stars()
    {
        return PlayerPrefs.GetInt(Game3StarsKey, 0);
    }

    public int GetGame4Stars()
    {
        return PlayerPrefs.GetInt(Game4StarsKey, 0);
    }

    public int GetGame5Stars()
    {
        return PlayerPrefs.GetInt(Game5StarsKey, 0);
    }


    // =========================================================
    // UNLOCK RULES
    // =========================================================

    // Game 1 is always unlocked.
    public bool IsGame1Unlocked()
    {
        return true;
    }

    // Game 2 requires 2+ stars in Game 1.
    public bool IsGame2Unlocked()
    {
        return GetGame1Stars() >= 2;
    }

    // Game 3 requires 2+ stars in Game 2.
    public bool IsGame3Unlocked()
    {
        return GetGame2Stars() >= 2;
    }

    // Game 4 requires 2+ stars in Game 3.
    public bool IsGame4Unlocked()
    {
        return GetGame3Stars() >= 2;
    }

    // Game 5 requires 2+ stars in Game 4.
    public bool IsGame5Unlocked()
    {
        return GetGame4Stars() >= 2;
    }


    // =========================================================
    // RESET ALL PROGRESS
    // =========================================================

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(Game1StarsKey);
        PlayerPrefs.DeleteKey(Game2StarsKey);
        PlayerPrefs.DeleteKey(Game3StarsKey);
        PlayerPrefs.DeleteKey(Game4StarsKey);
        PlayerPrefs.DeleteKey(Game5StarsKey);

        PlayerPrefs.Save();

        Debug.Log("🧹 ALL GAME PROGRESS RESET!");
    }
}