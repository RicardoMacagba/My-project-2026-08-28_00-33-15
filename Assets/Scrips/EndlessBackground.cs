
using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    [Header("Player and Camera")]
    public Transform player;
    public Camera mainCamera;

    [Header("Background Tiles")]
    public Transform backgroundA;
    public Transform backgroundB;

    private float backgroundWidth;
    private float previousPlayerX;
    private bool isReady;

    void Start()
    {
        // Check that all required objects are assigned.
        if (player == null ||
            mainCamera == null ||
            backgroundA == null ||
            backgroundB == null)
        {
            Debug.LogError(
                "EndlessBackground: Assign Player, Main Camera, " +
                "Background A, and Background B in the Inspector."
            );
            enabled = false;
            return;
        }

        SpriteRenderer spriteA =
            backgroundA.GetComponent<SpriteRenderer>();

        SpriteRenderer spriteB =
            backgroundB.GetComponent<SpriteRenderer>();

        if (spriteA == null || spriteB == null)
        {
            Debug.LogError(
                "Both background tiles need a SpriteRenderer."
            );
            enabled = false;
            return;
        }

        // Both tiles must have the same world-space width.
        backgroundWidth = spriteA.bounds.size.x;

        if (backgroundWidth <= 0f ||
            Mathf.Abs(backgroundWidth - spriteB.bounds.size.x) > 0.01f)
        {
            Debug.LogError(
                "Background A and B must have the same width."
            );
            enabled = false;
            return;
        }

        // Put tile B directly beside tile A.
        backgroundB.position = new Vector3(
            backgroundA.position.x + backgroundWidth,
            backgroundA.position.y,
            backgroundA.position.z
        );

        previousPlayerX = player.position.x;
        isReady = true;
    }

    void LateUpdate()
    {
        if (!isReady)
            return;

        // Measure how far the player moved this frame.
        float playerMovement =
            player.position.x - previousPlayerX;

        // Move both background tiles in the opposite direction.
        Vector3 movement =
            Vector3.left * playerMovement;

        backgroundA.position += movement;
        backgroundB.position += movement;

        previousPlayerX = player.position.x;

        // Recycle whichever tile has left the camera view.
        RepositionTile(backgroundA, backgroundB);
        RepositionTile(backgroundB, backgroundA);
    }

    void RepositionTile(Transform tile, Transform other)
    {
        float halfWidth = backgroundWidth / 2f;

        float cameraHalfWidth =
            mainCamera.orthographicSize * mainCamera.aspect;

        float cameraLeft =
            mainCamera.transform.position.x - cameraHalfWidth;

        float cameraRight =
            mainCamera.transform.position.x + cameraHalfWidth;

        float tileLeft = tile.position.x - halfWidth;
        float tileRight = tile.position.x + halfWidth;

        // Tile has moved completely off the left side.
        if (tileRight < cameraLeft)
        {
            tile.position = new Vector3(
                other.position.x + backgroundWidth,
                tile.position.y,
                tile.position.z
            );
        }

        // Tile has moved completely off the right side.
        else if (tileLeft > cameraRight)
        {
            tile.position = new Vector3(
                other.position.x - backgroundWidth,
                tile.position.y,
                tile.position.z
            );
        }
    }
}
