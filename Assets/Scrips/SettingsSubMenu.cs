using UnityEngine;

public class SettingsSubMenu : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;

    // [Header("Exit Button")]
    // public GameObject exitButton;

    [Header("Progress Manager")]
    public GameProgressManager progressManager;

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ResetAllProgress()
    {
        if (progressManager == null)
        {
            Debug.LogWarning("⚠️ SettingsMenu: GameProgressManager is not assigned.");
            return;
        }

        // Reset saved progress
        progressManager.ResetAllProgress();

        // Refresh Game 1 stars
        MenuStarDisplay[] game1Stars =
            FindObjectsByType<MenuStarDisplay>(FindObjectsSortMode.None);

        foreach (MenuStarDisplay display in game1Stars)
        {
            display.RefreshMenuStars();
        }

        // Refresh Game 2 stars
        HelpingStudentMenuStarDisplay[] game2Stars =
            FindObjectsByType<HelpingStudentMenuStarDisplay>(FindObjectsSortMode.None);

        foreach (HelpingStudentMenuStarDisplay display in game2Stars)
        {
            display.RefreshMenuStars();
        }

        // Refresh Game 3 lock
        GameLock[] game3Locks =
            FindObjectsByType<GameLock>(FindObjectsSortMode.None);

        foreach (GameLock game3Lock in game3Locks)
        {
            game3Lock.RefreshLock();
        }

        Debug.Log("🧹 SETTINGS: All game progress has been reset.");

        // Close settings after reset
        CloseSettings();
    }
}