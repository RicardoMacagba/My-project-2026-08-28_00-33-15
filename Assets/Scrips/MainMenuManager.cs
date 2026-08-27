using UnityEngine;
using UnityEngine.SceneManagement; // Required to shift between scenes

public class MainMenuManager : MonoBehaviour
{
    // This is the function your Play Button will call
    public void OpenGameCategories()
    {
        // 1. Tell the tracker that our final destination is the categories page
        SceneTracker.NextSceneName = "GameCategories";

        // 2. Open the banana loading screen buffer first
        SceneManager.LoadScene("LoadingScene");
    }

    // This is an extra function you can attach to your Quit Button!
    public void QuitGame()
    {
        Debug.Log("Game closing...");
        Application.Quit(); // Shuts down the .apk app on a real mobile device
    }
}
