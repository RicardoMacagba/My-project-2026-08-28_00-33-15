using UnityEngine;

public class GoFoodMover : MonoBehaviour
{
    public Transform tornado;

    [Header("Movement")]
    public float speed = 100f;
    public float stopDistance = 180f;
    public float pushDistance = 100f;

    [Header("Floating")]
    public float floatAmount = 10f;
    public float floatSpeed = 2f;

    private bool beingPushed = false;

    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (tornado == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            tornado.position
        );

        // -------------------------
        // MOVE TOWARD TORNADO
        // -------------------------
        if (!beingPushed)
        {
            if (distance > stopDistance)
            {
                Vector3 direction =
                    (tornado.position - transform.position).normalized;

                transform.position +=
                    direction * speed * Time.deltaTime;
            }
            else
            {
                beingPushed = true;
            }
        }

        // -------------------------
        // GET PUSHED AWAY
        // -------------------------
        else
        {
            Vector3 awayDirection =
                (transform.position - tornado.position).normalized;

            transform.position +=
                awayDirection * speed * Time.deltaTime;

            if (distance > stopDistance + pushDistance)
            {
                beingPushed = false;
            }
        }

        // -------------------------
        // FLOATING MOTION
        // -------------------------
        float floatY =
            Mathf.Sin(Time.time * floatSpeed + randomOffset)
            * floatAmount;

        transform.position += Vector3.up * floatY * Time.deltaTime;
    }
}