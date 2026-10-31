
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float jumpForce = 8f;

    [Header("Animation Objects")]
    public GameObject run;
    public GameObject jump;
    public GameObject bump;

    [Header("NPC")]
    public GameObject npc;
    public GameObject textBackground;
    public GameObject caption1;
    public GameObject caption2;
    public GameObject caption3;

    [Header("Energy")]
    public GameObject energyBar;
    public float energyDrainDuration = 10f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.12f;
    public LayerMask groundLayer;

    [Header("Cactus Bump")]
    public float bumpDuration = 0.4f;
    public float bumpKnockbackSpeed = 6f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private readonly HashSet<Collider2D> obstacleContacts =
        new HashSet<Collider2D>();

    private Animator runAnimator;
    private Animator jumpAnimator;
    private Animator bumpAnimator;
    private TypewriterText captionTypewriter;
    private TypewriterText caption2Typewriter;
    private TypewriterText caption3Typewriter;

    private bool isGrounded;
    private bool isBumped;
    private bool jumpStarted;
    private bool wasGrounded;
    private bool bumpTriggeredForContact;
    private bool bumpPresentationShown;
    private float bumpTimer;
    private float moveInput;

    private Vector3 runOriginalScale;
    private Vector3 jumpOriginalScale;
    private Vector3 bumpOriginalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (run != null)
            runAnimator = run.GetComponent<Animator>();

        if (jump != null)
            jumpAnimator = jump.GetComponent<Animator>();

        if (bump != null)
            bumpAnimator = bump.GetComponent<Animator>();

        if (caption1 != null)
            captionTypewriter = caption1.GetComponent<TypewriterText>();

        if (caption2 != null)
            caption2Typewriter = caption2.GetComponent<TypewriterText>();

        if (caption3 != null)
            caption3Typewriter = caption3.GetComponent<TypewriterText>();

        if (run != null)
            runOriginalScale = run.transform.localScale;

        if (jump != null)
            jumpOriginalScale = jump.transform.localScale;

        if (bump != null)
            bumpOriginalScale = bump.transform.localScale;
    }

    void Start()
    {
        if (rb == null || playerCollider == null)
        {
            Debug.LogError(
                "PlayerController requires a Rigidbody2D and a Collider2D.",
                this
            );
            enabled = false;
            return;
        }

        if (run == null || jump == null || bump == null)
        {
            Debug.LogError(
                "Assign Run, Jump, and Bump in the Inspector.",
                this
            );
            enabled = false;
            return;
        }

        if (npc == null || textBackground == null || caption1 == null)
            Debug.LogError(
                "Assign the NPC, Text Background, and Caption 1 in the Inspector.",
                this
            );

        if (energyBar == null)
        {
            Debug.LogError("Assign the Energy Bar in the Inspector.", this);
        }
        else if (energyBar.transform.childCount == 0)
        {
            Debug.LogError(
                "The Energy Bar needs child GameObjects for its bars.",
                energyBar
            );
        }

        SetBumpPresentationActive(false);

        ShowAnimation(run, runAnimator, "Base Layer.RunAgain");
    }

    void Update()
    {
        CheckGround();

        if (isBumped)
        {
            bumpTimer -= Time.deltaTime;

            if (bumpTimer <= 0f)
            {
                isBumped = false;
                jumpStarted = false;

                ShowAnimation(run, runAnimator, "Base Layer.RunAgain");
            }

            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
        }

        // Flip the artwork without mirroring the Rigidbody2D or collider.
        if (moveInput > 0)
        {
            SetFacingDirection(1f);
        }
        else if (moveInput < 0)
        {
            SetFacingDirection(-1f);
        }

        // Jump with the Up Arrow.
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) &&
            isGrounded)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );

            jumpStarted = true;
            ShowAnimation(jump, jumpAnimator, "Base Layer.Jump");
        }

        // Return to running after landing.
        if (isGrounded && !wasGrounded && jumpStarted)
        {
            jumpStarted = false;
            ShowAnimation(run, runAnimator, "Base Layer.RunAgain");
        }

        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isBumped)
            return;

        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );
    }

    void CheckGround()
    {
        if (rb == null || playerCollider == null)
        {
            isGrounded = false;
            return;
        }

        var filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false;

        int hitCount = rb.Cast(
            Vector2.down,
            filter,
            groundHits,
            groundCheckDistance
        );

        isGrounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (groundHits[i].normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (FindObstacleRoot(collision.collider) != null)
        {
            RegisterObstacleContact(collision.collider);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        UnregisterObstacleContact(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (FindObstacleRoot(other) != null)
        {
            RegisterObstacleContact(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        UnregisterObstacleContact(other);
    }

    void RegisterObstacleContact(Collider2D obstacle)
    {
        if (!obstacleContacts.Add(obstacle))
            return;

        if (!isBumped && !bumpTriggeredForContact)
        {
            bumpTriggeredForContact = true;
            TriggerBump(obstacle);
        }
    }

    void UnregisterObstacleContact(Collider2D obstacle)
    {
        obstacleContacts.Remove(obstacle);
        if (obstacleContacts.Count == 0)
            bumpTriggeredForContact = false;
    }

    Transform FindObstacleRoot(Collider2D collider)
    {
        if (!(collider is BoxCollider2D))
            return null;

        Transform current = collider.transform;
        while (current != null)
        {
            if (current.CompareTag("Obstacle"))
                return current;

            current = current.parent;
        }

        return null;
    }

    void SetFacingDirection(float direction)
    {
        SetVisualScale(run, runOriginalScale, direction);
        SetVisualScale(jump, jumpOriginalScale, direction);
        SetVisualScale(bump, bumpOriginalScale, direction);
    }

    static void SetVisualScale(GameObject visual, Vector3 originalScale, float direction)
    {
        if (visual == null)
            return;

        visual.transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * direction,
            originalScale.y,
            originalScale.z
        );
    }

    void TriggerBump(Collider2D obstacle)
    {
        if (isBumped)
            return;

        float horizontalOffset =
            playerCollider.bounds.center.x - obstacle.bounds.center.x;
        float pushDirection = Mathf.Sign(horizontalOffset);
        if (pushDirection == 0f)
        {
            pushDirection = moveInput != 0f
                ? -Mathf.Sign(moveInput)
                : 1f;
        }

        isBumped = true;
        bumpTimer = bumpDuration;
        moveInput = 0f;
        jumpStarted = false;
        rb.velocity = new Vector2(
            pushDirection * Mathf.Abs(bumpKnockbackSpeed),
            rb.velocity.y
        );

        if (!bumpPresentationShown)
        {
            bumpPresentationShown = true;
            SetBumpPresentationActive(true);
            StartCoroutine(DrainEnergy());
            StartCoroutine(PlayCaptionSequence());
        }

        ShowAnimation(bump, bumpAnimator, "Base Layer.BumpAgain");
    }

    void SetBumpPresentationActive(bool active)
    {
        if (npc != null)
            npc.SetActive(active);

        if (textBackground != null)
            textBackground.SetActive(active);

        if (energyBar != null)
            energyBar.SetActive(active);

        if (!active)
        {
            SetCaptionActive(caption1, false);
            SetCaptionActive(caption2, false);
            SetCaptionActive(caption3, false);
        }
    }

    IEnumerator DrainEnergy()
    {
        if (energyBar == null)
            yield break;

        int barCount = energyBar.transform.childCount;
        if (barCount == 0)
            yield break;

        for (int i = 0; i < barCount; i++)
            energyBar.transform.GetChild(i).gameObject.SetActive(true);

        float interval = Mathf.Max(0f, energyDrainDuration) / barCount;
        for (int i = barCount - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(interval);
            energyBar.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    IEnumerator PlayCaptionSequence()
    {
        GameObject[] captions = { caption1, caption2, caption3 };
        TypewriterText[] typewriters =
        {
            captionTypewriter,
            caption2Typewriter,
            caption3Typewriter
        };

        for (int i = 0; i < captions.Length; i++)
        {
            if (captions[i] == null)
            {
                Debug.LogError(
                    $"Assign Caption {i + 1} in the Inspector.",
                    this
                );
                yield break;
            }

            if (typewriters[i] == null)
            {
                Debug.LogError(
                    $"Caption {i + 1} needs a TypewriterText component.",
                    captions[i]
                );
                yield break;
            }

            SetCaptionActive(captions[i], true);
            typewriters[i].StartTyping();

            while (typewriters[i].IsTyping)
                yield return null;

            if (i + 1 < captions.Length)
            {
                if (captions[i + 1] == null)
                {
                    Debug.LogError(
                        $"Assign Caption {i + 2} in the Inspector.",
                        this
                    );
                    yield break;
                }

                if (typewriters[i + 1] == null)
                {
                    Debug.LogError(
                        $"Caption {i + 2} needs a TypewriterText component.",
                        captions[i + 1]
                    );
                    yield break;
                }

                SetCaptionActive(captions[i], false);
            }
        }
    }

    static void SetCaptionActive(GameObject caption, bool active)
    {
        if (caption != null)
            caption.SetActive(active);
    }

    void ShowAnimation(GameObject target, Animator animator, string stateName)
    {
        if (target == null)
            target = run;

        if (target == null)
            return;

        if (run != null)
            run.SetActive(target == run);

        if (jump != null)
            jump.SetActive(target == jump);

        if (bump != null)
            bump.SetActive(target == bump);

        if (animator != null)
        {
            animator.Rebind();
            if (target == run)
            {
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    if (parameter.name == "isGrounded" &&
                        parameter.type == AnimatorControllerParameterType.Bool)
                    {
                        animator.SetBool(parameter.name, true);
                    }
                }
            }

            animator.Play(stateName, 0, 0f);
            animator.Update(0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
            return;

        Gizmos.color = Color.yellow;
        Bounds bounds = collider.bounds;
        Vector3 feetCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        Gizmos.DrawLine(feetCenter, feetCenter + Vector3.down * groundCheckDistance);
    }
}
