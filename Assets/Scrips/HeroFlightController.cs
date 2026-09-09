using UnityEngine;

public class HeroFlightController : MonoBehaviour
{
    [Header("Flight & Steering Speeds")]
    public float verticalMoveSpeed = 5f;    
    public float horizontalMoveSpeed = 6f;  

    [Header("BOY CHARACTER ASSETS")]
    public Sprite boyCenterPose;              
    public Sprite boyLeftTiltPose;            
    public Sprite boyRightTiltPose;           

    [Header("BOY SIZE SCALING")]
    public float boyCenterScale = 1.0f;
    public float boyLeftScale = 1.0f;
    public float boyRightScale = 1.0f;

    [Header("GIRL CHARACTER ASSETS")]
    public Sprite girlCenterPose;              
    public Sprite girlLeftTiltPose;            
    public Sprite girlRightTiltPose;           

    [Header("GIRL SIZE SCALING")]
    public float girlCenterScale = 1.0f;
    public float girlLeftScale = 1.0f;
    public float girlRightScale = 1.0f;

    [Header("World Space Forcefield Walls")]
    [Tooltip("The left edge boundary wall. Increase this number to push the wall inward!")]
    public float leftWallLimit = -6.0f;
    [Tooltip("The right edge boundary wall. Decrease this number to keep them away from the right arrows!")]
    public float rightWallLimit = 3.5f;
    [Tooltip("The lowest point the hero can sink down.")]
    public float bottomWallLimit = -4.0f;
    [Tooltip("The highest point the hero can climb up.")]
    public float topWallLimit = 4.0f;

    private Rigidbody2D rb2d;
    private SpriteRenderer worldSpriteRenderer; 
    private HeroSelectionManager selectionManager;

    // Movement Tracking Flags
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isMovingUp = false;
    private bool isMovingDown = false;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        worldSpriteRenderer = GetComponent<SpriteRenderer>();
        selectionManager = Object.FindFirstObjectByType<HeroSelectionManager>();
    }

    void Update()
    {
        if (selectionManager == null || worldSpriteRenderer == null || rb2d == null) return;

        bool isBoy = selectionManager.IsBoySelected();
        float moveX = 0f;
        float currentScale = 1.0f;

        // 1. DIRECTIONAL MOVEMENT & SPRITE RESOLUTION
        if (isMovingLeft)
        {
            moveX = -1f;
            worldSpriteRenderer.sprite = isBoy ? boyLeftTiltPose : girlLeftTiltPose;
            currentScale = isBoy ? boyLeftScale : girlLeftScale;
        }
        else if (isMovingRight)
        {
            moveX = 1f;
            worldSpriteRenderer.sprite = isBoy ? boyRightTiltPose : girlRightTiltPose;
            currentScale = isBoy ? boyRightScale : girlRightScale;
        }
        else
        {
            worldSpriteRenderer.sprite = isBoy ? boyCenterPose : girlCenterPose;
            currentScale = isBoy ? boyCenterScale : girlCenterScale;
        }

        transform.localScale = new Vector3(currentScale, currentScale, 1f);

        // 2. VERTICAL HEIGHT CALCULATION
        float moveY = 0f;
        if (isMovingUp) moveY = 1f;
        else if (isMovingDown) moveY = -1f;

        // 3. ENFORCE PHYSICS VELOCITY MOTION
        rb2d.velocity = new Vector2(moveX * horizontalMoveSpeed, moveY * verticalMoveSpeed);

        // 4. 🚨 BULLETPROOF WORLD FORCEFIELD CLAMP:
        // Constrains your character's true coordinates in real-time so they can never cross onto your HUD buttons!
        float clampedX = Mathf.Clamp(transform.position.x, leftWallLimit, rightWallLimit);
        float clampedY = Mathf.Clamp(transform.position.y, bottomWallLimit, topWallLimit);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    // --- BUTTON EVENT TRIGGER BRIDGES ---
    public void SetMovingLeft(bool state) { isMovingLeft = state; }
    public void SetMovingRight(bool state) { isMovingRight = state; }
    public void SetMovingUp(bool state) { isMovingUp = state; }
    public void SetMovingDown(bool state) { isMovingDown = state; }
}
