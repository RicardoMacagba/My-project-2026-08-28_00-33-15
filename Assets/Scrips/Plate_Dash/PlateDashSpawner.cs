using UnityEngine;

public class PlateDashSpawner : MonoBehaviour
{
    [Header("Conveyor Spawning Points (Canvas Coordinate Positions)")]
    public RectTransform leftSpawnAnchor;  // Position a UI anchor far left offscreen (e.g., X = -1000)
    public RectTransform rightSpawnAnchor; // Position a UI anchor far right offscreen (e.g., X = 1000)
    public Transform activeFoodParent;     // Drag your Canvas parent transform here

    [Header("Item Pools")]
    public GameObject[] healthyFoodPrefabs;
    public GameObject[] unhealthyFoodPrefabs;

    [Header("Timing Calibration")]
    public float spawnInterval = 1.8f;
    [Range(0f, 100f)] public float junkFoodChance = 40f;

    private float timer;

    void Update()
    {
        if (PlateDashManager.Instance != null && PlateDashManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnConveyorPlate();
            timer = 0f;
        }
    }

    void SpawnConveyorPlate()
    {
        if (leftSpawnAnchor == null || rightSpawnAnchor == null || activeFoodParent == null)
        {
            Debug.LogError("PlateDashSpawner requires left/right spawn anchors and an active food parent.", this);
            return;
        }

        // Randomly choose a belt and a position within that belt's spawn-anchor height.
        bool useRightBelt = Random.value > 0.5f;
        RectTransform chosenAnchor = useRightBelt ? rightSpawnAnchor : leftSpawnAnchor;
        float spawnX = useRightBelt ? chosenAnchor.rect.xMax : chosenAnchor.rect.xMin;
        float randomY = Random.Range(chosenAnchor.rect.yMin, chosenAnchor.rect.yMax);
        Vector3 spawnWorldPosition = chosenAnchor.TransformPoint(new Vector3(spawnX, randomY, 0f));
        Vector3 spawnLocalPosition = activeFoodParent.InverseTransformPoint(spawnWorldPosition);
        Vector2 chosenCoordinates = new Vector2(spawnLocalPosition.x, spawnLocalPosition.y);

        // 2. Determine if item is healthy or unhealthy trap
        bool spawnUnhealthy = Random.Range(0f, 100f) < junkFoodChance;
        GameObject[] prefabPool = spawnUnhealthy ? unhealthyFoodPrefabs : healthyFoodPrefabs;
        if (prefabPool == null || prefabPool.Length == 0)
        {
            Debug.LogError($"PlateDashSpawner cannot spawn {(spawnUnhealthy ? "unhealthy" : "healthy")} food because its prefab pool is empty.", this);
            return;
        }

        GameObject selectedTemplate = prefabPool[Random.Range(0, prefabPool.Length)];
        if (selectedTemplate == null)
        {
            Debug.LogError($"PlateDashSpawner has a missing prefab in its {(spawnUnhealthy ? "unhealthy" : "healthy")} food pool.", this);
            return;
        }

        // 3. Create the UI element clone directly inside your UI Canvas structure
        GameObject clone = Instantiate(selectedTemplate, activeFoodParent, false);
        RectTransform cloneRect = clone.GetComponent<RectTransform>();
        if (cloneRect == null)
        {
            Debug.LogError($"PlateDashSpawner prefab '{selectedTemplate.name}' needs a RectTransform.", selectedTemplate);
            Destroy(clone);
            return;
        }

        cloneRect.anchoredPosition = chosenCoordinates;
        CenterVisualChildren(cloneRect);

        // 4. Configure vector flight direction parameters
        PlateDashFood dashItemScript = clone.GetComponent<PlateDashFood>();
        if (dashItemScript == null)
        {
            Debug.LogError($"PlateDashSpawner prefab '{selectedTemplate.name}' needs a PlateDashFood component.", selectedTemplate);
            Destroy(clone);
            return;
        }

        dashItemScript.spawnOnRightBelt = useRightBelt;
    }

    static void CenterVisualChildren(RectTransform root)
    {
        if (root.childCount == 0) return;

        Vector2 center = Vector2.zero;
        for (int i = 0; i < root.childCount; i++)
        {
            center += ((RectTransform)root.GetChild(i)).anchoredPosition;
        }

        center /= root.childCount;
        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = (RectTransform)root.GetChild(i);
            child.anchoredPosition -= center;
        }
    }
}
