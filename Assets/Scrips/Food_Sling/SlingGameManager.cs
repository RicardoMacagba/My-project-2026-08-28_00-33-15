using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlingGameManager : MonoBehaviour
{
    [System.Serializable]
    public class SlingFood
    {
        public string name;
        public GameObject foodUIObject;
    }

    [Header("Healthy Food Ammo")]
    public SlingFood[] foodItems;
    public GameObject[] unhealthyTargets;
    public float timeLimitSeconds = 60f;
    public Text timerText;
    private int currentFoodIndex;
    private int targetCount;
    private float remainingTime;
    private bool isGameOver;
    private readonly HashSet<GameObject> convertedTargets = new HashSet<GameObject>();

    public bool IsGameOver => isGameOver;

    [Header("Sling Scene Anchors")]
    public RectTransform slingCenterAnchor;
    public Transform leftProngTip;
    public Transform rightProngTip;
    public UILineRenderer rubberBandLine;

    void Start()
    {
        if (foodItems == null || foodItems.Length == 0)
        {
            Debug.LogError("SlingGameManager has no healthy food ammo configured.", this);
            return;
        }

        if (unhealthyTargets == null || unhealthyTargets.Length == 0)
        {
            Debug.LogError("SlingGameManager has no unhealthy targets configured.", this);
            return;
        }

        foreach (SlingFood item in foodItems)
        {
            if (item == null || item.foodUIObject == null)
            {
                Debug.LogError("SlingGameManager contains a healthy food with no object assigned.", this);
                continue;
            }

            item.foodUIObject.SetActive(true);

            UISlingshotItem slingScript = item.foodUIObject.GetComponent<UISlingshotItem>();
            if (slingScript != null)
                slingScript.enabled = false;
        }

        foreach (GameObject target in unhealthyTargets)
        {
            if (target == null)
            {
                Debug.LogError("SlingGameManager contains an unhealthy target with no object assigned.", this);
                continue;
            }

            SlingTargetTrigger targetTrigger = target.GetComponent<SlingTargetTrigger>();
            if (targetTrigger == null)
                targetTrigger = target.AddComponent<SlingTargetTrigger>();

            targetTrigger.Initialize(this);
            targetCount++;
        }

        if (targetCount == 0)
        {
            Debug.LogError("SlingGameManager has no valid unhealthy targets.", this);
            return;
        }

        if (timeLimitSeconds < 60f)
        {
            Debug.LogError("The sling game timer must be at least 60 seconds.", this);
            return;
        }

        remainingTime = timeLimitSeconds;
        CreateTimerDisplayIfNeeded();
        UpdateTimerDisplay();
        SpawnNextFood();
    }

    void Update()
    {
        if (isGameOver)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isGameOver = true;
            if (timerText != null)
                timerText.text = "Time's up!";
            Debug.Log("Time's up! The level is over.");
            return;
        }

        UpdateTimerDisplay();
    }

    void CreateTimerDisplayIfNeeded()
    {
        if (timerText != null)
            return;

        Canvas canvas = slingCenterAnchor != null ? slingCenterAnchor.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            Debug.LogError("Cannot create the timer display because the sling is not under a Canvas.", this);
            return;
        }

        GameObject timerObject = new GameObject("Sling Timer", typeof(RectTransform), typeof(Text));
        timerObject.transform.SetParent(canvas.transform, false);

        RectTransform timerRect = timerObject.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(1f, 1f);
        timerRect.anchorMax = new Vector2(1f, 1f);
        timerRect.pivot = new Vector2(1f, 1f);
        timerRect.anchoredPosition = new Vector2(-32f, -24f);
        timerRect.sizeDelta = new Vector2(260f, 70f);

        timerText = timerObject.GetComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 48;
        timerText.fontStyle = FontStyle.Bold;
        timerText.alignment = TextAnchor.MiddleRight;
        timerText.color = Color.white;
        timerText.raycastTarget = false;
        if (timerText.font == null)
            Debug.LogError("Could not load the built-in font for the sling timer.", timerObject);
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    void SpawnNextFood()
    {
        if (currentFoodIndex >= foodItems.Length)
        {
            if (convertedTargets.Count < targetCount)
                Debug.LogWarning("No healthy food ammo remains before all unhealthy targets were converted.", this);
            return;
        }

        SlingFood item = foodItems[currentFoodIndex];
        if (item == null || item.foodUIObject == null)
        {
            Debug.LogError($"Healthy food at index {currentFoodIndex} has no object assigned.", this);
            return;
        }

        if (slingCenterAnchor == null)
        {
            Debug.LogError("Sling center anchor is not assigned.", this);
            return;
        }

        GameObject currentFood = item.foodUIObject;
        currentFood.SetActive(true);

        UISlingshotItem slingScript = currentFood.GetComponent<UISlingshotItem>();
        if (slingScript == null)
            slingScript = currentFood.AddComponent<UISlingshotItem>();

        slingScript.enabled = true;
        slingScript.Initialize(this);
        slingScript.SetupInitialPosition(slingCenterAnchor.position, rubberBandLine, leftProngTip, rightProngTip);
    }

    public void TryConvertTarget(GameObject target, UISlingshotItem projectile)
    {
        if (isGameOver || target == null || projectile == null || !projectile.IsLaunched)
            return;

        if (currentFoodIndex >= foodItems.Length ||
            foodItems[currentFoodIndex] == null ||
            foodItems[currentFoodIndex].foodUIObject != projectile.gameObject)
            return;

        if (convertedTargets.Contains(target))
            return;

        bool isConfiguredTarget = false;
        foreach (GameObject configuredTarget in unhealthyTargets)
        {
            if (configuredTarget == target)
            {
                isConfiguredTarget = true;
                break;
            }
        }

        if (!isConfiguredTarget)
            return;

        Image targetImage = target.GetComponent<Image>();
        Image projectileImage = projectile.GetComponent<Image>();
        if (targetImage == null || projectileImage == null || projectileImage.sprite == null)
        {
            Debug.LogError($"Cannot convert target '{target.name}': target and projectile need UI Images with sprites.", target);
            return;
        }

        targetImage.sprite = projectileImage.sprite;
        convertedTargets.Add(target);
        projectile.gameObject.SetActive(false);
        currentFoodIndex++;

        Debug.Log($"Converted {target.name} with {foodItems[currentFoodIndex - 1].name}.");

        if (convertedTargets.Count >= targetCount)
        {
            isGameOver = true;
            if (timerText != null)
                timerText.text = "Complete!";
            Debug.Log("Level Complete! All unhealthy foods have become healthy.");
            return;
        }

        SpawnNextFood();
    }

    public void ResetMissedFood(UISlingshotItem projectile)
    {
        if (isGameOver ||
            projectile == null ||
            currentFoodIndex >= foodItems.Length ||
            foodItems[currentFoodIndex] == null ||
            foodItems[currentFoodIndex].foodUIObject != projectile.gameObject ||
            slingCenterAnchor == null)
            return;

        projectile.SetupInitialPosition(slingCenterAnchor.position, rubberBandLine, leftProngTip, rightProngTip);
    }
}
