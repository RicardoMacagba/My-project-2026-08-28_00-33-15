using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Crucial for TextMeshPro components

[System.Serializable]
public class CutsceneSlideData
{
    public Sprite backgroundSprite; // The background artwork panel
    [TextArea(3, 5)]
    public string npcDialogueText;  // What the NPC says
    public float displayDuration = 4.0f; // How many seconds this specific slide stays on screen
}

public class AutomatedIntroManager : MonoBehaviour
{
    [Header("UI Element Bindings")]
    public GameObject introPanel;
    public Image slideImageDisplay;
    public TextMeshProUGUI dialogueTextDisplay; // Updated to support your Text (TMP) object

    [Header("Classroom Main Menu Setup")]
    public GameObject classroomMenuPanel;
    public Button tiredStudentButton;
    public GameObject tapPromptText; // Text that says "Tap the tired student to start!"

    [Header("Cutscene Data")]
    [Tooltip("Add your 4 panels here in order.")]
    public List<CutsceneSlideData> cutsceneSlides;

    void Start()
    {
        // Ensure panels start in the correct state
        introPanel.SetActive(true);
        
        // Safety checks to prevent crash if menu objects aren't linked yet
        if (classroomMenuPanel != null) classroomMenuPanel.SetActive(false);
        if (tapPromptText != null) tapPromptText.SetActive(false);
        if (tiredStudentButton != null) tiredStudentButton.interactable = false;

        // Start the automated cutscene sequence
        if (cutsceneSlides != null && cutsceneSlides.Count > 0)
        {
            StartCoroutine(PlayCutsceneRoutine());
        }
        else
        {
            Debug.LogError("Please assign slides to the Cutscene Slides list in the Inspector!");
            EndCutscene();
        }
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        // Loop through each slide one by one automatically
        for (int i = 0; i < cutsceneSlides.Count; i++)
        {
            // Update the background sprite and dialogue text instantly
            if (cutsceneSlides[i].backgroundSprite != null)
            {
                slideImageDisplay.sprite = cutsceneSlides[i].backgroundSprite;
            }
            
            dialogueTextDisplay.text = cutsceneSlides[i].npcDialogueText;

            // Pause the script execution and wait for the specified duration
            yield return new WaitForSeconds(cutsceneSlides[i].displayDuration);
        }

        // Once the loop finishes all 4 slides, transition to the game setup
        EndCutscene();
    }

    private void EndCutscene()
    {
        Debug.Log("Intro finished! Turning off intro panel.");
        // Hide the intro sequence completely
        introPanel.SetActive(false);

        // Turn on the interactive classroom environment
        if (classroomMenuPanel != null) classroomMenuPanel.SetActive(true);
        if (tapPromptText != null) tapPromptText.SetActive(true);
        if (tiredStudentButton != null) tiredStudentButton.interactable = true;
    }
}
