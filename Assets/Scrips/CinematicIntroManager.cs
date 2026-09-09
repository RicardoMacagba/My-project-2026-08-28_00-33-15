using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class IntroSlideData
{
    public Sprite backgroundSprite; // The background artwork panel
    [TextArea(3, 5)]
    public string npcDialogueText;  // What the NPC says during this specific slide
}

public class CinematicIntroManager : MonoBehaviour
{
    [Header("UI Element Bindings")]
    public Image slideImageDisplay;
    public Text dialogueTextDisplay; // Use TMPro.TextMeshProUGUI if using TextMeshPro
    public GameObject introPanel;

    [Header("Intro Slideshow Data")]
    [Tooltip("Add exactly 4 elements here to match your 4 comic panels.")]
    public List<IntroSlideData> introSlides;

    private int currentIndex = 0;

    void Start()
    {
        // 1. Ensure the intro panel is visible at game start
        introPanel.SetActive(true);
        currentIndex = 0;

        // 2. Load up the very first slide and text line
        UpdateCurrentSlide();
    }

    // Connect this function to your "Next" button click event in Unity
    public void AdvanceIntro()
    {
        currentIndex++;

        // If we still have slides left, show the next one
        if (currentIndex < introSlides.Count)
        {
            UpdateCurrentSlide();
        }
        // If we finished the last slide, turn off the intro and jump to the menu/game
        else
        {
            FinishIntroPanel();
        }
    }

    private void UpdateCurrentSlide()
    {
        if (introSlides != null && introSlides.Count > currentIndex)
        {
            // Update both background image and NPC text at the exact same moment
            slideImageDisplay.sprite = introSlides[currentIndex].backgroundSprite;
            dialogueTextDisplay.text = introSlides[currentIndex].npcDialogueText;
        }
    }

    private void FinishIntroPanel()
    {
        Debug.Log("Intro finished! Turning off intro panel.");
        introPanel.SetActive(false);

        // TODO: This is where you activate your classroom menu panel 
        // and tell them: "Tap the tired student to start the game!"
    }
}
