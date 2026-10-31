using UnityEngine;
using UnityEngine.UI;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefabs Arrays")]
    public GameObject[] healthyFoodPrefabs;   // Assign your matching foods here
    public GameObject[] unhealthyFoodPrefabs; // Assign junk food objects here

    [Header("Spawn Control Settings")]
    public float spawnInterval = 1.2f;    // Fast drop rates for enjoyment/rush
    [Range(0f, 100f)] 
    public float unhealthySpawnChance = 65f; // More unhealthy items spawn than healthy ones

    [Header("Spawn Position Range")]
    public float minX = -6.5f;
    public float maxX = 6.5f;

    private float timer;
    private RectTransform canvasRect;
    private RectTransform spawnerRect;

    void Awake()
    {
        canvasRect = transform.parent as RectTransform;
        spawnerRect = transform as RectTransform;

        if (canvasRect == null || spawnerRect == null)
        {
            Debug.LogError("FoodSpawner must be a RectTransform under a UI Canvas.", this);
            enabled = false;
        }
    }

    void Start()
    {
        DisableTemplateMovement(healthyFoodPrefabs);
        DisableTemplateMovement(unhealthyFoodPrefabs);
    }

    void Update()
    {
        if (PlateCatcherManager.Instance != null && PlateCatcherManager.Instance.IsGameOver)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRandomFood();
            timer = 0f;
        }
    }

    void SpawnRandomFood()
    {
        bool spawnUnhealthy = Random.Range(0f, 100f) < unhealthySpawnChance;
        GameObject[] prefabs = spawnUnhealthy ? unhealthyFoodPrefabs : healthyFoodPrefabs;

        if (prefabs == null || prefabs.Length == 0)
            prefabs = spawnUnhealthy ? healthyFoodPrefabs : unhealthyFoodPrefabs;

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("FoodSpawner needs at least one food prefab assigned.", this);
            enabled = false;
            return;
        }

        GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];
        if (selectedPrefab == null)
        {
            Debug.LogError("FoodSpawner selected an empty prefab slot.", this);
            return;
        }

        GameObject food = Instantiate(selectedPrefab, canvasRect, false);
        RectTransform foodRect = food.GetComponent<RectTransform>();
        if (foodRect == null)
        {
            Debug.LogError("Spawned food prefab must have a RectTransform.", selectedPrefab);
            Destroy(food);
            return;
        }

        FallingFoodItem foodItem = food.GetComponent<FallingFoodItem>();
        if (foodItem == null)
        {
            Debug.LogError("Spawned food prefab needs a FallingFoodItem component.", selectedPrefab);
            Destroy(food);
            return;
        }

        Image foodImage = food.GetComponent<Image>();
        if (foodImage != null) foodImage.raycastTarget = false;

        foodRect.anchoredPosition = new Vector2(
            Random.Range(minX, maxX),
            spawnerRect.anchoredPosition.y);
        foodRect.SetAsLastSibling();
        foodItem.enabled = true;
    }

    static void DisableTemplateMovement(GameObject[] prefabs)
    {
        if (prefabs == null) return;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null) continue;

            FallingFoodItem foodItem = prefab.GetComponent<FallingFoodItem>();
            if (foodItem != null) foodItem.enabled = false;
        }
    }
}
