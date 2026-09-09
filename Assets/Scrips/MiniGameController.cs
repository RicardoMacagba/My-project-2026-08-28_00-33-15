using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MiniGameController : MonoBehaviour
{
    [System.Serializable]
    public class Question
    
    {
        public string foodName;
        public GameObject foodObject;
        public string correctAnswer;
    }

    [Header("Question")]
    public TMP_Text questionText;

    [Header("Dialogue Animation")]
    public float letterSpeed = 0.04f;
    public float mouthSpeed = 0.12f;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    [Header("Answer Buttons")]
    public Button goButton;
    public Button growButton;
    public Button glowButton;

    [Header("Energy")]
    public Slider energySlider;

    [Header("Feedback")]
    public TMP_Text ricardoText;
    public TMP_Text explanationText;
    public Button nextButton;

    [Header("Circle Board LED Lights")]
    public GameObject circleGoLight;
    public GameObject circleGrowLight;
    public GameObject circleGlowLight;

    public float startingEnergy = 20f;
    public float correctAmount = 16f;
    public float wrongAmount = 10f;

    [Header("Food Questions")]
    public Question[] questions;

    [Header("Ricardo Mouth")]
    public GameObject mouthOpen;
    public GameObject mouthClosed;

    [Header("Energy Bar Visual")]
    public GameObject[] energyBeans;
    //public Slider energySlider;


    [Header("Star Scoring")]
    public int firstAttemptCorrect = 0;

    private bool currentQuestionAnswered = false;

    private int currentQuestion = 0;

    private bool waitingForNext = false;
    private bool currentAnswerCorrect = false;

    void Start()
    {
        goButton.onClick.AddListener(() => Answer("GO"));
        growButton.onClick.AddListener(() => Answer("GROW"));
        glowButton.onClick.AddListener(() => Answer("GLOW"));

        nextButton.onClick.AddListener(NextStep);

        energySlider.minValue = 0f;
        energySlider.maxValue = 100f;
        energySlider.value = startingEnergy;

        nextButton.gameObject.SetActive(false);

        if (explanationText != null)
        {
            explanationText.text = "";
        }

        if (ricardoText != null)
        {
            ricardoText.text = "";
        }

        ShowQuestion();
    }

    void ShowQuestion()
    {
        waitingForNext = false;
        currentAnswerCorrect = false;
        currentQuestionAnswered = false;

        // Hide all foods
        for (int i = 0; i < questions.Length; i++)
        {
            if (questions[i].foodObject != null)
            {
                questions[i].foodObject.SetActive(false);
            }
        }

        // Show current food
        if (questions[currentQuestion].foodObject != null)
        {
            questions[currentQuestion].foodObject.SetActive(true);
        }

        // Show question
        if (questionText != null)
        {
            questionText.text =
                "What type of food is " +
                questions[currentQuestion].foodName +
                "?";
        }

        // Enable answer buttons
        goButton.interactable = true;
        growButton.interactable = true;
        glowButton.interactable = true;

        // Clear previous feedback
        if (ricardoText != null)
        {
            ricardoText.text = "";
        }

        if (explanationText != null)
        {
            explanationText.text = "";
        }

        nextButton.gameObject.SetActive(false);

        Debug.Log(
            "Current food: " +
            questions[currentQuestion].foodName
        );
    }

    void Answer(string playerAnswer)
    {
        if (waitingForNext)
            return;

        waitingForNext = true;

        LightUpAnswer(playerAnswer);

        // Lock all answer buttons
        goButton.interactable = false;
        growButton.interactable = false;
        glowButton.interactable = false;

        string correctAnswer =
            questions[currentQuestion].correctAnswer;

        string food =
            questions[currentQuestion].foodName;

        bool firstAttempt =
            !currentQuestionAnswered;

        currentQuestionAnswered = true;

        if (playerAnswer == correctAnswer)
        {
            currentAnswerCorrect = true;

            if (firstAttempt)
            {
                firstAttemptCorrect++;
            }

            energySlider.value += correctAmount;

            if (ricardoText != null)
            {
                StartRicardoDialogue("Nice!");
            }

            if (explanationText != null)
            {
                explanationText.text =
                    "Go Foods give us energy to learn, play, and move. " +
                    food +
                    " is a Go Food!";
            }

            SetNextButtonText("NEXT FOOD");

            Debug.Log("CORRECT! Energy: " + energySlider.value);
        }
        else
        {
            currentAnswerCorrect = false;

            energySlider.value -= wrongAmount;

            if (ricardoText != null)
            {
                StartRicardoDialogue("Oh No!");
            }

            if (explanationText != null)
            {
                if (playerAnswer == "GROW")
                {
                    explanationText.text =
                        "Grow Foods help build and repair our muscles. " +
                        "But " +
                        food +
                        " is a Go Food.";
                }
                else if (playerAnswer == "GLOW")
                {
                    explanationText.text =
                        "Glow Foods help protect our body. " +
                        "But " +
                        food +
                        " is a Go Food.";
                }
            }

            SetNextButtonText("TRY AGAIN");

            Debug.Log("WRONG! Energy: " + energySlider.value);
        }

        nextButton.gameObject.SetActive(true);
    }

    void NextStep()
    {
        nextButton.gameObject.SetActive(false);

        if (!currentAnswerCorrect)
        {
            // Let the player try the same food again
            waitingForNext = false;

            goButton.interactable = true;
            growButton.interactable = true;
            glowButton.interactable = true;

            if (ricardoText != null)
            {
                StartRicardoDialogue("Try again!");
            }

            if (explanationText != null)
            {
                explanationText.text = "";
            }

            return;
        }

        // Correct answer
        currentQuestion++;

        if (currentQuestion >= questions.Length)
        {
            int stars = CalculateStars();

            SaveGame2Progress(stars);

            Debug.Log(
                "GAME COMPLETE! First-attempt correct: " +
                firstAttemptCorrect +
                "/5 | Stars: " +
                stars
            );

            SceneTracker.NextSceneName = "GoFoodsSubCategory";
            SceneManager.LoadScene("LoadingScene");

            return;
}

        ShowQuestion();
    }

    void SetNextButtonText(string newText)
    {
        TMP_Text buttonText =
            nextButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = newText;
        }
    }

    void LightUpAnswer(string answer)
    {
        // Turn all LEDs off first
        circleGoLight.SetActive(false);
        circleGrowLight.SetActive(false);
        circleGlowLight.SetActive(false);

        // Turn on the selected LED
        if (answer == "GO")
        {
            circleGoLight.SetActive(true);
        }
        else if (answer == "GROW")
        {
            circleGrowLight.SetActive(true);
        }
        else if (answer == "GLOW")
        {
            circleGlowLight.SetActive(true);
        }
    }

    void StartRicardoDialogue(string sentence)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeDialogue(sentence));
    }

    IEnumerator TypeDialogue(string sentence)
    {
        isTyping = true;

        ricardoText.text = "";

        mouthOpen.SetActive(false);
        mouthClosed.SetActive(true);

        float mouthTimer = 0f;

        foreach (char letter in sentence)
        {
            ricardoText.text += letter;

            mouthTimer += letterSpeed;

            if (mouthTimer >= mouthSpeed)
            {
                mouthTimer = 0f;

                mouthOpen.SetActive(!mouthOpen.activeSelf);
                mouthClosed.SetActive(!mouthClosed.activeSelf);
            }

            yield return new WaitForSeconds(letterSpeed);
        }

        mouthOpen.SetActive(false);
        mouthClosed.SetActive(true);

        isTyping = false;
    }

    int CalculateStars()
    {
        if (firstAttemptCorrect == 5)
            return 3;

        if (firstAttemptCorrect == 4)
            return 2;

        if (firstAttemptCorrect == 3)
            return 1;

        return 0;
    }

    void UpdateEnergyBeans()
    {
        if (energySlider == null || energyBeans == null)
            return;

        int totalBeans = energyBeans.Length;

        float energyPercent =
            energySlider.value / energySlider.maxValue;

        int beansToShow =
            Mathf.RoundToInt(energyPercent * totalBeans);

        for (int i = 0; i < totalBeans; i++)
        {
            if (energyBeans[i] != null)
            {
                energyBeans[i].SetActive(i < beansToShow);
            }
        }
    }

    void Update()
    {
        UpdateEnergyBeans();
    }

    void SaveGame2Progress(int stars)
    {
        int previousStars =
            PlayerPrefs.GetInt(
                "HelpingStudent_HighScoreStars",
                0
            );
        // Only save if the new score is better than the previous score
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(
                "HelpingStudent_HighScoreStars",
                stars
            );
        }

        PlayerPrefs.Save();

        Debug.Log(
            "⭐ Game 2 best score saved: "
            + PlayerPrefs.GetInt(
                "HelpingStudent_HighScoreStars",
                0
            )
        );
    }
}