
using UnityEngine;

public class EndlessTiles : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera mainCamera;

    [Header("Ground Chunks")]
    public Transform groundA;
    public Transform groundB;

    [Header("Chunk Width")]
    [Tooltip("Width of each ground chunk in world units.")]
    public float chunkWidth = 20f;

    private bool ready;

    private void Start()
    {
        if (player == null ||
            mainCamera == null ||
            groundA == null ||
            groundB == null)
        {
            Debug.LogError(
                "EndlessTiles: Assign Player, Main Camera, " +
                "Ground A, and Ground B.", this);
            enabled = false;
            return;
        }

        if (groundA == groundB)
        {
            Debug.LogError(
                "EndlessTiles: Ground A and Ground B must be different objects.",
                this);
            enabled = false;
            return;
        }

        if (chunkWidth <= 0f)
        {
            Debug.LogError(
                "EndlessTiles: Chunk Width must be greater than zero.",
                this);
            enabled = false;
            return;
        }

        // Put ground B immediately after ground A.
        groundB.position = new Vector3(
            groundA.position.x + chunkWidth,
            groundA.position.y,
            groundA.position.z);

        ready = true;
    }

    private void LateUpdate()
    {
        if (!ready)
            return;

        RecycleChunk(groundA, groundB);
        RecycleChunk(groundB, groundA);
    }

    private void RecycleChunk(Transform chunk, Transform other)
    {
        float cameraHalfWidth =
            mainCamera.orthographicSize * mainCamera.aspect;

        float cameraLeft =
            mainCamera.transform.position.x - cameraHalfWidth;

        float cameraRight =
            mainCamera.transform.position.x + cameraHalfWidth;

        float chunkLeft = chunk.position.x - chunkWidth / 2f;
        float chunkRight = chunk.position.x + chunkWidth / 2f;

        // If this chunk has gone past the left side,
        // place it after the other chunk.
        if (chunkRight < cameraLeft)
        {
            chunk.position = new Vector3(
                other.position.x + chunkWidth,
                chunk.position.y,
                chunk.position.z);
        }
        // If moving left, recycle it to the other side.
        else if (chunkLeft > cameraRight)
        {
            chunk.position = new Vector3(
                other.position.x - chunkWidth,
                chunk.position.y,
                chunk.position.z);
        }
    }
}
