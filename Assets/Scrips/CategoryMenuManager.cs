using UnityEngine;
using UnityEngine.SceneManagement;

public class CategoryMenuManager : MonoBehaviour
{
    [Header("Game Progress")]
    public GameProgressManager progressManager;


    // =========================================================
    // GAME 1
    // =========================================================

    public void Button_FlyHero()
    {
        // Game 1 is always unlocked.

        Debug.Log("🚀 Button_FlyHero clicked: starting Game 1.");

        SceneTracker.NextSceneName = "MiniGame_SuperGoHero";
        SceneManager.LoadScene("LoadingScene");
    }


    // =========================================================
    // GO FOODS SUB-CATEGORY
    // =========================================================

    public void OpenGoFoodsSubCategory()
    {
        Debug.Log("🚀 Opening GoFoodsSubCategory.");

        SceneTracker.NextSceneName = "GoFoodsSubCategory";
        SceneManager.LoadScene("LoadingScene");
    }


    // =========================================================
    // GAME 2
    // =========================================================

    public void LoadStudentFuelGame()
    {
        if (progressManager == null)
        {
            Debug.LogWarning(
                "⚠️ CategoryMenuManager: GameProgressManager is not assigned."
            );
            return;
        }

        // Game 2 requires 2+ stars in Game 1.
        if (!progressManager.IsGame2Unlocked())
        {
            Debug.Log(
                "🔒 Game 2 is locked. You need 2 or more stars in Game 1."
            );
            return;
        }

        Debug.Log("🚀 GAME 2 UNLOCKED: Loading MiniGame_StudentFuel...");

        SceneTracker.NextSceneName = "MiniGame_StudentFuel";
        SceneManager.LoadScene("LoadingScene");
    }


    // =========================================================
    // GAME 3 For Dancing Robot
    // =========================================================

    // public void LoadStudentFuelGame()
    // {
    //     if (progressManager == null)
    //     {
    //         Debug.LogWarning(
    //             "⚠️ CategoryMenuManager: GameProgressManager is not assigned."
    //         );
    //         return;
    //     }

    //     // Game 3 requires 2+ stars in Game 2.
    //     if (!progressManager.IsGame3Unlocked())
    //     {
    //         Debug.Log(
    //             "🔒 Game 3 is locked. You need 2 or more stars in Game 2."
    //         );
    //         return;
    //     }

    //     Debug.Log("🚀 GAME 3 UNLOCKED: Loading MiniGame_StudentFuel...");

    //     SceneTracker.NextSceneName = "MiniGame_StudentFuel";
    //     SceneManager.LoadScene("LoadingScene");
    // }


    // =========================================================
    // BACK BUTTONS
    // =========================================================

    public void GoBackToMainMenu()
    {
        SceneTracker.NextSceneName = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }


    public void GoBackToCategoryMenu()
    {
        SceneTracker.NextSceneName = "GameCategories";
        SceneManager.LoadScene("LoadingScene");
    }
}