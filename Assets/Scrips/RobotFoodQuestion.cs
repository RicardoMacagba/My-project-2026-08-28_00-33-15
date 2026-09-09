using UnityEngine;

[System.Serializable]
public class RobotFoodQuestion
{
    [Header("Food Information")]
    public string foodName;
    public Sprite foodSprite;
    public string correctAnswer;

    [Header("Robot Assembly")]
    [Range(1, 5)]
    public int assemblyStage;

    [Header("Food Destination")]
    public RectTransform target;

    public Vector2 targetOffset;
}