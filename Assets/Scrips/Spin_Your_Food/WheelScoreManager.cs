using UnityEngine;
using UnityEngine.UI;

public class WheelScoreManager : MonoBehaviour
{
    public Text scoreText; // Drag 'timer_text' or a new score text asset component field here
    private int currentScore = 0;

    void Start()
    {
        UpdateScoreDisplay();
    }

    public void AddScore(int pointsToAdd)
    {
        currentScore += pointsToAdd;
        if (currentScore < 0) currentScore = 0; // Prevent score from tracking into negative numbers
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }
}
