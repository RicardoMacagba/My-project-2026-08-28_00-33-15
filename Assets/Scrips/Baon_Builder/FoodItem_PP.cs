using UnityEngine;

public enum FoodCategory { Go, Grow, Glow, Water }

public class FoodItem_PP : MonoBehaviour
{
    [Header("Food Configuration")]
    public FoodCategory category;
}
