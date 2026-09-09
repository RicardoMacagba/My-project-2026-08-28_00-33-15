using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrativeController : MonoBehaviour
{
    [Header("Scene Sections")]

    public GameObject background;
    public GameObject background2;
    public GameObject intro;
    public GameObject flyingGoFoods;
    public GameObject narrativesIntro;
    public GameObject spotlight;

    [Header("Dialogue")]
    public TMP_Text dialogueText;
    public Button nextButton;
    public CanvasGroup narrativeGroup;
    public CanvasGroup dialogueGroup;

    [Header("Ricardo Mouth")]
    public GameObject mouthOpen;
    public GameObject mouthClosed;

    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public GameObject practiceQuestion;
    public TMP_Text tutorialDialogueText;
    public Button tutorialNextButton;

    [Header("Tutorial Ricardo Mouth")]
    public GameObject tutorialMouthOpen;
    public GameObject tutorialMouthClosed;

    [Header("Tutorial Answer Buttons")]
    public Button tutorialGoButton;
    public Button tutorialGrowButton;
    public Button tutorialGlowButton;
    public GameObject tutorialShadowCircleGlowing;

    [Header("Actual Mini-Game")]
    public GameObject gamePanel;

    [Header("Text Animation")]
    public float letterSpeed = 0.04f;
    public float mouthSpeed = 0.12f;

    [Header("Timing")]
    public float introDuration = 5f;

    private int dialogueIndex = 0;

    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentTypingText = "";

    private bool tutorialPracticeStarted = false;
    private bool tutorialCorrect = false;

    private string[] dialogues =
    {
        "Hi! My name is Ricardo.",

        "Oh no! Our classmate needs our help. " +
        "His energy level is getting low. He needs Go Foods!",

        "Can you help him?",

        "Choose the right Go Foods to give him energy!"
    };

    void Start()
    {
        // Start with the intro visible
        intro.SetActive(true);
        flyingGoFoods.SetActive(true);

        // Hide Ricardo
        narrativesIntro.SetActive(false);
        SetCanvasGroupAlpha(narrativeGroup, 0f);
        SetCanvasGroupAlpha(dialogueGroup, 0f);
        SetTextAlpha(0f);

        // Hide spotlight
        spotlight.SetActive(false);

        // Prepare mouth
        mouthOpen.SetActive(false);
        mouthClosed.SetActive(true);

        // Tutorial setup
        tutorialPanel.SetActive(false);
        practiceQuestion.SetActive(false);
        tutorialMouthOpen.SetActive(false);
        tutorialMouthClosed.SetActive(true);
        gamePanel.SetActive(false);

        // Connect buttons
        nextButton.onClick.AddListener(NextDialogue);
        tutorialNextButton.onClick.AddListener(TutorialNext);
        tutorialGoButton.onClick.AddListener(() => CheckTutorialAnswer("GO"));
        tutorialGrowButton.onClick.AddListener(() => CheckTutorialAnswer("GROW"));
        tutorialGlowButton.onClick.AddListener(() => CheckTutorialAnswer("GLOW"));

        // Start automatic transition
        Invoke(nameof(StartDialogue), introDuration);
    }

    void StartDialogue()
    {
        // Show the dialogue section smoothly
        intro.SetActive(true);
        flyingGoFoods.SetActive(true);

        // Show the spotlight
        spotlight.SetActive(true);

        // Show Ricardo
        narrativesIntro.SetActive(true);

        dialogueIndex = 0;
        StartCoroutine(FadeInDialogue());
    }

    IEnumerator FadeInDialogue()
    {
        SetCanvasGroupAlpha(narrativeGroup, 0f);
        SetCanvasGroupAlpha(dialogueGroup, 0f);
        SetTextAlpha(0f);

        float elapsed = 0f;
        float fadeDuration = 0.6f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            SetCanvasGroupAlpha(narrativeGroup, alpha);
            SetCanvasGroupAlpha(dialogueGroup, alpha);
            SetTextAlpha(alpha);

            yield return null;
        }

        SetCanvasGroupAlpha(narrativeGroup, 1f);
        SetCanvasGroupAlpha(dialogueGroup, 1f);
        SetTextAlpha(1f);
        ShowDialogue();
    }

    void SetCanvasGroupAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
        {
            group.alpha = alpha;
        }
    }

    void SetTextAlpha(float alpha)
    {
        if (dialogueText != null)
        {
            Color textColor = dialogueText.color;
            textColor.a = alpha;
            dialogueText.color = textColor;
        }
    }

    void ShowDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(
            TypeDialogue(dialogues[dialogueIndex], dialogueText, mouthOpen, mouthClosed)
        );
    }

    IEnumerator TypeDialogue(
        string sentence,
        TMP_Text text,
        GameObject openMouth,
        GameObject closedMouth
    )
    {
        isTyping = true;
        currentTypingText = sentence;

        text.text = "";

        openMouth.SetActive(false);
        closedMouth.SetActive(true);

        float mouthTimer = 0f;

        foreach (char letter in sentence)
        {
            text.text += letter;

            mouthTimer += letterSpeed;

            if (mouthTimer >= mouthSpeed)
            {
                mouthTimer = 0f;

                openMouth.SetActive(!openMouth.activeSelf);
                closedMouth.SetActive(!closedMouth.activeSelf);
            }

            yield return new WaitForSeconds(letterSpeed);
        }

        isTyping = false;

        openMouth.SetActive(false);
        closedMouth.SetActive(true);
    }

    void NextDialogue()
    {
        // If Ricardo is still talking,
        // finish the sentence immediately.
        if (isTyping)
        {
            FinishCurrentDialogue();
            return;
        }

        // Move to next dialogue
        dialogueIndex++;

        if (dialogueIndex < dialogues.Length)
        {
            ShowDialogue();
        }
        else
        {
            StartTutorial();
        }
    }

    void StartTutorial()
    {
        narrativesIntro.SetActive(false);
        spotlight.SetActive(false);

        tutorialPanel.SetActive(true);
        practiceQuestion.SetActive(false);

        tutorialPracticeStarted = false;
        tutorialCorrect = false;

        tutorialGoButton.interactable = true;
        tutorialGrowButton.interactable = true;
        tutorialGlowButton.interactable = true;
        tutorialShadowCircleGlowing.SetActive(true);

        SetTutorialButtonText("Next");

        StartTutorialDialogue(
            "Here's how to play! Read the question, then tap GO, GROW, or GLOW to choose your answer."
        );
    }

    void TutorialNext()
    {
        if (isTyping)
        {
            FinishCurrentDialogue();
            return;
        }

        if (!tutorialPracticeStarted)
        {
            tutorialPracticeStarted = true;
            practiceQuestion.SetActive(true);
            tutorialNextButton.gameObject.SetActive(false);
            StartTutorialDialogue("Let's try one together!");
            SetTutorialButtonText("Next");
            return;
        }

        if (tutorialCorrect)
        {
            StartMiniGame();
            return;
        }
    }

    void StartTutorialDialogue(string sentence)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(
            TypeDialogue(sentence, tutorialDialogueText, tutorialMouthOpen, tutorialMouthClosed)
        );
    }

    void CheckTutorialAnswer(string playerAnswer)
    {
        if (tutorialCorrect)
            return;

        if (isTyping)
        {
            FinishCurrentDialogue();
        }

        string correctAnswer = "GO";

        if (playerAnswer == correctAnswer)
        {
            TutorialCorrectAnswer();
        }
        else
        {
            TutorialWrongAnswer(playerAnswer);
        }
    }

    void TutorialCorrectAnswer()
    {
        tutorialCorrect = true;

        tutorialGoButton.interactable = false;
        tutorialGrowButton.interactable = false;
        tutorialGlowButton.interactable = false;
        tutorialNextButton.gameObject.SetActive(true);

        StartTutorialDialogue(
            "Correct! Bread is a Go Food! Go Foods give us energy to learn, play, and move."
        );

        SetTutorialButtonText("START GAME");
    }

    void TutorialWrongAnswer(string playerAnswer)
    {
        string explanation = "";

        if (playerAnswer == "GROW")
        {
            explanation =
                "You chose Grow Food. Grow Foods help build and repair our muscles. But Bread is a Go Food. Try again!";
        }
        else if (playerAnswer == "GLOW")
        {
            explanation =
                "You chose Glow Food. Glow Foods help protect our body. But Bread is a Go Food. Try again!";
        }

        tutorialNextButton.gameObject.SetActive(false);
        StartTutorialDialogue(explanation);
    }

    void FinishCurrentDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (tutorialPanel.activeSelf && tutorialDialogueText != null)
        {
            tutorialDialogueText.text = currentTypingText;
        }
        else if (dialogueText != null)
        {
            dialogueText.text = currentTypingText;
        }

        isTyping = false;

        mouthOpen.SetActive(false);
        mouthClosed.SetActive(true);

        tutorialMouthOpen.SetActive(false);
        tutorialMouthClosed.SetActive(true);
    }

void StartMiniGame()
{
    Debug.Log("Mini-game starting!");

    tutorialPanel.SetActive(false);
    spotlight.SetActive(false);

    gamePanel.SetActive(true);

    MiniGameController miniGame =
        gamePanel.GetComponent<MiniGameController>();

    if (miniGame != null)
    {
        //miniGame.StartGame();
    }
}

    void SetTutorialButtonText(string newText)
    {
        TMP_Text buttonText = tutorialNextButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = newText;
        }
    }
}

