using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BaonGameManager : MonoBehaviour
{
    [Header("Goal Requirements")]
    public int requiredGlow = 2;
    public int requiredGo = 1;
    public int requiredGrow = 1;
    public int requiredWater = 1;

    [Header("Current Baon Status")]
    private int currentGlow = 0;
    private int currentGo = 0;
    private int currentGrow = 0;
    private int currentWater = 0;

    [Header("UI Elements")]
    public Button checkMyBaonButton;
    //public GameObject outroAnimationPanel;

    // Track items currently inside the lunchbox tray
    private List<FoodItem_PP> itemsInTray = new List<FoodItem_PP>();

    void Start()
    {
        // Hide the check button and outro panel at the start
        if (checkMyBaonButton != null) checkMyBaonButton.gameObject.SetActive(false);
        //if (outroAnimationPanel != null) outroAnimationPanel.SetActive(false);

        // Assign the button click event dynamically
        if (checkMyBaonButton != null)
        {
            checkMyBaonButton.onClick.AddListener(OnCheckBaonClicked);
        }
    }

    // Call this function whenever a food item is dropped into the lunchbox
    public void AddFoodToTray(FoodItem_PP food)
    {
        if (!itemsInTray.Contains(food))
        {
            itemsInTray.Add(food);
            UpdateCategoryCount(food.category, 1);
            CheckGoals();
        }
    }

    // Call this function if a player drags a food item OUT of the lunchbox
    public void RemoveFoodFromTray(FoodItem_PP food)
    {
        if (itemsInTray.Contains(food))
        {
            itemsInTray.Remove(food);
            UpdateCategoryCount(food.category, -1);
            CheckGoals();
        }
    }

    private void UpdateCategoryCount(FoodCategory category, int amount)
    {
        switch (category)
        {
            case FoodCategory.Go: currentGo += amount; break;
            case FoodCategory.Grow: currentGrow += amount; break;
            case FoodCategory.Glow: currentGlow += amount; break;
            case FoodCategory.Water: currentWater += amount; break;
        }
    }

    // Evaluates if the current items match or exceed the target goals
    private void CheckGoals()
    {
        bool goalsMet = (currentGlow >= requiredGlow) &&
                        (currentGo >= requiredGo) &&
                        (currentGrow >= requiredGrow) &&
                        (currentWater >= requiredWater);

        // Show button only if requirements are fulfilled
        if (checkMyBaonButton != null)
        {
            checkMyBaonButton.gameObject.SetActive(goalsMet);
        }
    }

    private void OnCheckBaonClicked()
    {
        // 1. Show the Outro UI Panel / Play Animation
        //if (outroAnimationPanel != null)
        //{
        //    outroAnimationPanel.SetActive(true);
            
        //    // If you have an Animator component on the panel, trigger it here:
        //    // outroAnimationPanel.GetComponent<Animator>().SetTrigger("PlayOutro");
        //}

        // 2. Transition to next mini-game after a delay or inside an animation event
        Invoke("LoadNextMiniGame", 3.0f); // 3-second delay example
    }

    private void LoadNextMiniGame()
    {
        // Replace with your actual scene loading logic
        // UnityEngine.SceneManagement.SceneManager.LoadScene("NextMiniGameSceneName");
        Debug.Log("Loading next mini-game...");
    }
}
