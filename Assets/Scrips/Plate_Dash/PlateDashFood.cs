using UnityEngine;
using System.Collections;

public class PlateDashFood : MonoBehaviour
{
    [Header("Food Identity Parameters")]
    public bool isUnhealthy = false;
    public FoodCategory category; 
    
    [Header("Conveyor Physics")]
    public float rollSpeed = 200f;
    [HideInInspector] public bool spawnOnRightBelt = false; // Controlled by the Spawner script

    private RectTransform rectTransform;
    private bool isProcessed = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Register itself into the manager's active tracking loop queue
        if (PlateDashManager.Instance != null)
        {
            PlateDashManager.Instance.activeFoods.Add(this);
        }
    }

    void Update()
    {
        if (isProcessed || (PlateDashManager.Instance != null && PlateDashManager.Instance.IsGameOver)) return;

        // Right belt items travel Left (-1), Left belt items travel Right (+1)
        float directionModifier = spawnOnRightBelt ? -1f : 1f;
        rectTransform.anchoredPosition += new Vector2(directionModifier * rollSpeed * Time.deltaTime, 0f);

        // Natural Pass/Miss Boundary (If it slides past the center line unsorted)
        if (!spawnOnRightBelt && rectTransform.anchoredPosition.x > 80f) SafeMissExit();
        if (spawnOnRightBelt && rectTransform.anchoredPosition.x < -80f) SafeMissExit();
    }

    public void AnimateFlyToPlate()
    {
        isProcessed = true;
        PlateDashManager.Instance.activeFoods.Remove(this);
        StartCoroutine(FlyToCenterAnimation());
    }

    IEnumerator FlyToCenterAnimation()
    {
        Vector2 targetCenterPos = Vector2.zero; // Moves down directly onto center plate
        Vector2 startingPos = rectTransform.anchoredPosition;
        float duration = 0.25f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startingPos, targetCenterPos, elapsedTime / duration);
            yield return null;
        }

        // Add any script calls here to turn on layers of the 'center_plate_to_fill' asset
        Destroy(gameObject);
    }

    public void TriggerSplatFailure()
    {
        RemoveFromQueueAndDestroy();
    }

    void SafeMissExit()
    {
        if (isUnhealthy)
        {
            Debug.Log("🗑️ Unhealthy item safely discarded off the belt lines into trash.");
        }
        else
        {
            Debug.Log("💨 Healthy food item slipped past without being sorted!");
        }
        RemoveFromQueueAndDestroy();
    }

    void RemoveFromQueueAndDestroy()
    {
        if (PlateDashManager.Instance != null)
        {
            PlateDashManager.Instance.activeFoods.Remove(this);
        }
        Destroy(gameObject);
    }
}
