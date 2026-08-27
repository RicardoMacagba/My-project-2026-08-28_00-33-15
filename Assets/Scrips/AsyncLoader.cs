using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minimumDelay = 2.5f;

    private void Start()
    {
        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        yield return new WaitForSeconds(minimumDelay);

        // Reads the exact scene name stored by whatever button was clicked
        string targetScene = SceneTracker.NextSceneName;

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
