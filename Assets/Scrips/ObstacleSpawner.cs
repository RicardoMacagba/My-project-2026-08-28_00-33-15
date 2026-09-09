using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Template Arrays")]
    public GameObject[] goFoodPrefabs;    
    public GameObject[] junkFoodPrefabs;  

    [Header("Spawning Timing Controls")]
    public float spawnInterval = 1.5f; 
    
    [Header("World Space Lane Bounds")]
    [Tooltip("The high point in world coordinates above the screen where items spawn")]
    public float spawnPositionY = 6f;   
    
    [Tooltip("Left limit of your world lanes")]
    public float leftLimitX = -5f;
    
    [Tooltip("Right limit of your world lanes")]
    public float rightLimitX = 4f;

    private float timer = 0f;
    private bool canSpawn = false;

    void Update()
    {
        if (!canSpawn) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRandomAsset();
            timer = 0f; 
        }
    }

    public void StartSpawningGameplay()
    {
        canSpawn = true;
    }

    void SpawnRandomAsset()
    {
        // Determine the visible screen bounds so the spawn point is anchored to the top edge,
        // not to the spawner object's center position in the scene.
        float cameraHalfHeight = Camera.main != null ? Camera.main.orthographicSize : 5f;
        float cameraHalfWidth = Camera.main != null ? cameraHalfHeight * Camera.main.aspect : 6f;

        float randomX = Random.Range(-cameraHalfWidth + 1f, cameraHalfWidth - 1f);
        float spawnY = cameraHalfHeight + 1.5f;

        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        // 2. Decide 50/50 whether to drop a Healthy item or an Obstacle item
        GameObject chosenPrefab = null;
        if (Random.value > 0.5f)
        {
            if (goFoodPrefabs.Length > 0)
                chosenPrefab = goFoodPrefabs[Random.Range(0, goFoodPrefabs.Length)];
        }
        else
        {
            if (junkFoodPrefabs.Length > 0)
                chosenPrefab = junkFoodPrefabs[Random.Range(0, junkFoodPrefabs.Length)];
        }

        if (chosenPrefab == null) return;

        // 3. Spawn at the top of the camera view, not at the spawner's center.
        Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);
    }
}
