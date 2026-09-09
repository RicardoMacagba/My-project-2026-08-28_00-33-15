using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RobotFoodGameManager : MonoBehaviour
{
    [Header("Questions")]
    public RobotFoodQuestion[] questions;

    [Header("Question UI")]
    public Image foodImage;
    public TMP_InputField answerInput;
    public TMP_Text feedbackText;
    public TMP_Text questionNumberText;

    [Header("Magic Food")]
    public MagicFoodMover magicFoodMover;

    [Header("Timing")]
    public float correctAnswerDelay = 0.5f;
    public float nextQuestionDelay = 0.5f;

    private int currentQuestion = 0;
    private int firstAttemptCorrect = 0;
    private bool firstAttemptUsed = false;

    // Prevents the player from submitting multiple times
    // while the magical food is moving.
    private bool isProcessingAnswer = false;

    void Start()
    {
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestion >= questions.Length)
        {
            FinishGame();
            return;
        }

        RobotFoodQuestion question =
            questions[currentQuestion];

        // Show the question food image
        if (foodImage != null)
        {
            foodImage.sprite = question.foodSprite;
        }

        // Prepare the MagicFood automatically
        if (magicFoodMover != null)
        {
            magicFoodMover.SetFoodSprite(
                question.foodSprite
            );

            magicFoodMover.assemblyStage =
                question.assemblyStage;

            magicFoodMover.target =
                question.target;

            magicFoodMover.targetOffset =
                question.targetOffset;

            magicFoodMover.ResetFood();
        }

        // Clear previous answer
        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.interactable = true;
            answerInput.ActivateInputField();
        }

        // Clear feedback
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        // Question number
        if (questionNumberText != null)
        {
            questionNumberText.text =
                "Question " +
                (currentQuestion + 1) +
                " / " +
                questions.Length;
        }

        firstAttemptUsed = false;
        isProcessingAnswer = false;

        Debug.Log(
            "🍎 Showing Question " +
            (currentQuestion + 1) +
            ": " +
            question.foodName
        );
    }

    public void SubmitAnswer()
    {
        // Prevent duplicate submissions
        if (isProcessingAnswer)
            return;

        if (currentQuestion >= questions.Length)
            return;

        // Safety check
        if (answerInput == null)
            return;

        string playerAnswer =
            answerInput.text
            .Trim()
            .ToLower();

        string correctAnswer =
            questions[currentQuestion]
            .correctAnswer
            .Trim()
            .ToLower();

        // Prevent empty answers
        if (string.IsNullOrEmpty(playerAnswer))
        {
            if (feedbackText != null)
            {
                feedbackText.text =
                    "Please type your answer!";
            }

            answerInput.ActivateInputField();
            return;
        }

        if (playerAnswer == correctAnswer)
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
    }

    void CorrectAnswer()
    {
        isProcessingAnswer = true;

        // Count first-attempt accuracy
        if (!firstAttemptUsed)
        {
            firstAttemptCorrect++;
            firstAttemptUsed = true;
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                "✨ Correct! Great job!";
        }

        // Disable typing while animation plays
        if (answerInput != null)
        {
            answerInput.interactable = false;
        }

        StartCoroutine(
            SendMagicFood()
        );
    }

    void WrongAnswer()
    {
        firstAttemptUsed = true;

        if (feedbackText != null)
        {
            feedbackText.text =
                "💛 Not quite! Try again.";
        }

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.ActivateInputField();
        }
    }

    IEnumerator SendMagicFood()
    {
        // Small moment for the player to see "Correct!"
        yield return new WaitForSeconds(
            correctAnswerDelay
        );

        if (magicFoodMover != null)
        {
            magicFoodMover.SendFoodToRobot();

            // Wait for:
            // 1. Food movement
            // 2. Sparkle effect
            yield return new WaitForSeconds(
                magicFoodMover.moveDuration +
                magicFoodMover.sparkleDuration
            );
        }

        yield return new WaitForSeconds(
            nextQuestionDelay
        );

        // Move to the next food question
        currentQuestion++;

        ShowQuestion();
    }

    void FinishGame()
    {
        Debug.Log(
            "🎉 ROBOT COMPLETE!"
        );

        Debug.Log(
            "⭐ First-attempt correct: " +
            firstAttemptCorrect
        );

        // We will add:
        // - Energy bar completion
        // - Robot dance
        // - Star calculation
        // - Save Game 3 stars
        // in the next step.
    }
}