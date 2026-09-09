using System.Collections;
using UnityEngine;

public class RobotAssemblyController : MonoBehaviour
{
    [Header("Robot Parts")]
    public RectTransform head;
    public RectTransform body;
    public RectTransform leftArm;
    public RectTransform rightArm;
    public RectTransform leftFoot;
    public RectTransform rightFoot;

    [Header("Robot Face")]
    public GameObject happyFace;

    [Header("Starting Offsets")]
    public Vector2 headStartOffset = new Vector2(0f, 250f);
    public Vector2 bodyStartOffset = new Vector2(0f, -250f);
    public Vector2 leftArmStartOffset = new Vector2(-250f, 0f);
    public Vector2 rightArmStartOffset = new Vector2(250f, 0f);
    public Vector2 leftFootStartOffset = new Vector2(-180f, -150f);
    public Vector2 rightFootStartOffset = new Vector2(180f, -150f);

    [Header("Animation")]
    public float assemblyDuration = 0.8f;

    private Vector2 headTarget;
    private Vector2 bodyTarget;
    private Vector2 leftArmTarget;
    private Vector2 rightArmTarget;
    private Vector2 leftFootTarget;
    private Vector2 rightFootTarget;


    void Awake()
    {
        // Remember the final assembled positions.
        if (head != null)
            headTarget = head.anchoredPosition;

        if (body != null)
            bodyTarget = body.anchoredPosition;

        if (leftArm != null)
            leftArmTarget = leftArm.anchoredPosition;

        if (rightArm != null)
            rightArmTarget = rightArm.anchoredPosition;

        if (leftFoot != null)
            leftFootTarget = leftFoot.anchoredPosition;

        if (rightFoot != null)
            rightFootTarget = rightFoot.anchoredPosition;


        // Hide the face at the beginning.
        if (happyFace != null)
            happyFace.SetActive(false);


        // Separate the robot pieces.
        if (head != null)
            head.anchoredPosition = headTarget + headStartOffset;

        if (body != null)
            body.anchoredPosition = bodyTarget + bodyStartOffset;

        if (leftArm != null)
            leftArm.anchoredPosition = leftArmTarget + leftArmStartOffset;

        if (rightArm != null)
            rightArm.anchoredPosition = rightArmTarget + rightArmStartOffset;

        if (leftFoot != null)
            leftFoot.anchoredPosition = leftFootTarget + leftFootStartOffset;

        if (rightFoot != null)
            rightFoot.anchoredPosition = rightFootTarget + rightFootStartOffset;
    }


    // =========================================================
    // QUESTION 1
    // =========================================================

    public void AssembleHead()
    {
        if (head != null)
        {
            StartCoroutine(
                MovePart(head, headTarget)
            );
        }

        Debug.Log("🤖 Question 1: HEAD ASSEMBLING!");
    }


    // =========================================================
    // QUESTION 2
    // =========================================================

    public void AssembleBody()
    {
        if (body != null)
        {
            StartCoroutine(
                MovePart(body, bodyTarget)
            );
        }

        Debug.Log("🤖 Question 2: BODY ASSEMBLING!");
    }


    // =========================================================
    // QUESTION 3
    // =========================================================

    public void AssembleArms()
    {
        if (leftArm != null)
        {
            StartCoroutine(
                MovePart(leftArm, leftArmTarget)
            );
        }

        if (rightArm != null)
        {
            StartCoroutine(
                MovePart(rightArm, rightArmTarget)
            );
        }

        Debug.Log("🤖 Question 3: ARMS ASSEMBLING!");
    }


    // =========================================================
    // QUESTION 4
    // =========================================================

    public void AssembleFeet()
    {
        if (leftFoot != null)
        {
            StartCoroutine(
                MovePart(leftFoot, leftFootTarget)
            );
        }

        if (rightFoot != null)
        {
            StartCoroutine(
                MovePart(rightFoot, rightFootTarget)
            );
        }

        Debug.Log("🤖 Question 4: FEET ASSEMBLING!");
    }


    // =========================================================
    // QUESTION 5
    // =========================================================

    public void AssembleFace()
    {
        if (happyFace != null)
        {
            happyFace.SetActive(true);
        }

        Debug.Log("🤖 Question 5: FACE COMPLETE!");
    }


    // =========================================================
    // MOVEMENT
    // =========================================================

    IEnumerator MovePart(
    RectTransform part,
    Vector2 target
    )
    {
        Vector2 startPosition =
            part.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < assemblyDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / assemblyDuration
                );

            // Smooth movement toward the robot.
            progress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            part.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    target,
                    progress
                );

            yield return null;
        }

        // Make absolutely sure we reach the exact target.
        part.anchoredPosition = target;

        // Little bounce when the part connects.
        yield return StartCoroutine(
            BouncePart(part, target)
        );
    }

    IEnumerator BouncePart(
    RectTransform part,
    Vector2 target
    )
    {
        float bounceDistance = 12f;
        float bounceDuration = 0.18f;

        // Move slightly past the target.
        Vector2 bouncePosition =
            target + Vector2.down * bounceDistance;

        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / bounceDuration
                );

            part.anchoredPosition =
                Vector2.Lerp(
                    target,
                    bouncePosition,
                    progress
                );

            yield return null;
        }

        elapsed = 0f;

        // Spring back to the correct position.
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / bounceDuration
                );

            part.anchoredPosition =
                Vector2.Lerp(
                    bouncePosition,
                    target,
                    progress
                );

            yield return null;
        }

        part.anchoredPosition = target;
    }
}