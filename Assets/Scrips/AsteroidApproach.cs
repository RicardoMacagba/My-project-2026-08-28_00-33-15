using UnityEngine;

public class AsteroidApproach : MonoBehaviour
{
    public RectTransform hero;
    public UnityEngine.UI.Image heroImage;
public GameObject lowEnergyGlow;

public HeroFall heroFall;

    public float approachSpeed = 300f;
    public float impactSpeed = 900f;

    public float stopDistance = 1500f;
    public float impactDistance = 80f;

    public Vector2 afterBumpPosition = new Vector2(61f, 628f);
public float moveAwaySpeed = 600f;   // px per second

    public float pauseTime = 0.5f;

    public GameObject bumpEffect;

    public GameObject forestBG; // optional - drag ForestBG here
    public GameObject forestBG2;

    private RectTransform asteroid;
    private float pauseTimer = 0f;

    public GameObject bar;
    public GameObject bar1;
    public GameObject bar2;
    public GameObject bar3;
    public GameObject bar4;

    public GameObject dizzyEffect;

    private enum State
    {
        Approaching,
        Pausing,
        Impacting,
        Finished
    }

    private State state = State.Approaching;

    void Start()
    {
        asteroid = GetComponent<RectTransform>();
    }

    void Update()
    {
        Vector2 direction = hero.anchoredPosition - asteroid.anchoredPosition;
        float distance = direction.magnitude;

        switch (state)
        {
            case State.Approaching:

                if (distance > stopDistance)
                {
                    asteroid.anchoredPosition +=
                        direction.normalized * approachSpeed * Time.deltaTime;
                }
                else
                {
                    pauseTimer = pauseTime;
                    state = State.Pausing;
                }

                break;


            case State.Pausing:

                pauseTimer -= Time.deltaTime;

                if (pauseTimer <= 0f)
                {
                    state = State.Impacting;
                }

                break;


            case State.Impacting:

                if (distance > impactDistance)
                {
                    asteroid.anchoredPosition +=
                        direction.normalized * impactSpeed * Time.deltaTime;
                }
                else
                {
                if (bumpEffect != null)
                {
                    bumpEffect.SetActive(true);
                }

                MakeHeroWeak();
                heroFall.BeginFall();

                // Drop energy to the final red bar
                if (bar1 != null) bar1.SetActive(false);
                if (bar2 != null) bar2.SetActive(false);
                if (bar3 != null) bar3.SetActive(false);
                if (bar4 != null) bar4.SetActive(false);

                state = State.Finished;

                }

                break;  

                case State.Finished:
                asteroid.anchoredPosition = Vector2.MoveTowards(
                    asteroid.anchoredPosition,
                    afterBumpPosition,
                    moveAwaySpeed * Time.deltaTime);

                //heroFall.BeginFall();

                break;
        }
    }

    void MakeHeroWeak()
    {
        if (heroImage != null)
        {
            heroImage.color = new Color(0.55f, 0.55f, 0.55f, 1f);
        }

        if (lowEnergyGlow != null)
        {
            lowEnergyGlow.SetActive(true);
            if (dizzyEffect != null) dizzyEffect.SetActive(true);
        }
        if (heroFall != null)
        {
            heroFall.lowEnergyGlow = lowEnergyGlow;
            heroFall.dizzyEffect = dizzyEffect;
            if (bumpEffect != null) bumpEffect.SetActive(false);
            // forestBG.SetActive(false);
            // forestBG2.SetActive(false);
            
            heroFall.BeginFall();
        }
            }
}