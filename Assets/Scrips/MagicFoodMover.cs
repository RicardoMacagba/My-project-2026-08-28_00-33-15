using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MagicFoodMover : MonoBehaviour
{
    [Header("Food")]
    public RectTransform food;
    public Image foodImage;

    [Header("Starting Position")]
    public Vector2 foodStartPosition;

    [Header("Target")]
    public RectTransform target;

    [Header("Target Offset")]
    public Vector2 targetOffset = new Vector2(0f, 0f);

    [Header("Robot")]
    public RobotAssemblyController robotAssembly;

    [Header("Movement")]
    public float moveDuration = 1.2f;

    [Header("Magic Effect")]
    public GameObject sparkleEffect;
    public float sparkleDuration = 0.5f;

    [Header("Food Pulse")]
    public float glowSpeed = 4f;
    public float glowAmount = 0.15f;

    [Header("Stage")]
    public int assemblyStage = 1;

    private Vector3 originalScale;

    

    void Awake()
    {
        if (food != null)
        {
            originalScale = food.localScale;

            foodStartPosition =
                food.anchoredPosition;
        }

    }

    void Start()
    {
        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(false);
        }
    }

    void Update()
    {
        if (food == null)
            return;

        float pulse =
            1f +
            Mathf.Sin(Time.time * glowSpeed)
            * glowAmount;

        food.localScale =
            originalScale * pulse;
    }

    public void SetFoodSprite(Sprite newSprite)
    {
        if (foodImage != null)
        {
            foodImage.sprite = newSprite;
        }
    }

    public void ResetFood()
    {
        if (food == null)
            return;

        StopAllCoroutines();

        food.gameObject.SetActive(true);

        food.anchoredPosition =
            foodStartPosition;

        food.localScale =
            originalScale;

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(false);
        }
    }

    public void SendFoodToRobot()
    {
        if (food == null || target == null)
        {
            Debug.LogWarning(
                "MagicFoodMover is missing Food or Target."
            );

            return;
        }

        StartCoroutine(
            MoveFoodToRobot()
        );
    }

    IEnumerator MoveFoodToRobot()
    {
        Vector2 startPosition =
            food.anchoredPosition;

        Vector2 targetPosition =
            target.anchoredPosition
            + targetOffset;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / moveDuration
                );

            progress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            food.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

            yield return null;
        }

        food.anchoredPosition =
            targetPosition;

        Debug.Log(
            "✨ Magical Go Food reached the robot!"
        );

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(true);
        }

        yield return new WaitForSeconds(
            sparkleDuration
        );

        if (robotAssembly != null)
        {
            switch (assemblyStage)
            {
                case 1:
                    robotAssembly.AssembleHead();
                    break;

                case 2:
                    robotAssembly.AssembleBody();
                    break;

                case 3:
                    robotAssembly.AssembleArms();
                    break;

                case 4:
                    robotAssembly.AssembleFeet();
                    break;

                case 5:
                    robotAssembly.AssembleFace();
                    break;
            }
        }

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(false);
        }

        food.gameObject.SetActive(false);

        Debug.Log(
            "🤖 Robot assembly stage " +
            assemblyStage +
            " complete!"
        );
    }
}