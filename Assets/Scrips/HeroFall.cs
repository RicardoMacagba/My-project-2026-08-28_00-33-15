using UnityEngine;

public class HeroFall : MonoBehaviour
{
    [Header("Falling")]
    public float impactPauseDuration = 0.7f;
    public float gravity = 3000f;       // px/s² - higher = falls faster
    public float fallDistance = 1200f;  // how far down he drops
    public float endScale = 0.3f;       // shrinks to 30% = "falling away" look
    public Vector2 drift = new Vector2(-300f, 0f); // slides toward bottom-LEFT

    [Header("Losing control")]
    public float wobbleAmount = 120f;
    public float wobbleSpeed = 2.5f;
    public float tumbleAngle = 25f;
    public float tumbleSpeed = 3f;

    [Header("What happens next")]
    public AirplaneCatch rescue;        // drag AirplaneCatchHero here
    public GameObject lowEnergyGlow;    // optional - drag LowEnergyGlow here
    public GameObject dizzyEffect;      // optional - drag DizzyEffect here

    // public GameObject forestBG; // optional - drag ForestBG here
    // public GameObject forestBG2;

    RectTransform hero;
    Vector3 baseScale;
    float verticalSpeed;
    float startX, startY, stopY, time;
    float impactPauseTimer;
    Vector3 lowEnergyGlowOffset;
    Vector3 dizzyEffectOffset;
    bool falling;

    public void BeginFall()
    {
        if (falling) return;
        hero = GetComponent<RectTransform>();
        baseScale = hero.localScale;
        startX = hero.anchoredPosition.x;
        startY = hero.anchoredPosition.y;
        stopY = startY - fallDistance;
        impactPauseTimer = Mathf.Max(0f, impactPauseDuration);
        if (lowEnergyGlow != null)
        {
            lowEnergyGlowOffset = lowEnergyGlow.transform.position - hero.position;
        }
        if (dizzyEffect != null)
        {
            dizzyEffectOffset = dizzyEffect.transform.position - hero.position;
        }
        falling = true;
        // forestBG.SetActive(false);
        // forestBG2.SetActive(false);


    }

    void Update()
    {
        if (!falling) return;

        UpdateEffectPositions();

        if (impactPauseTimer > 0f)
        {
            impactPauseTimer -= Time.deltaTime;
            return;
        }

        time += Time.deltaTime;

        verticalSpeed += gravity * Time.deltaTime;

        Vector2 pos = hero.anchoredPosition;
        pos.y -= verticalSpeed * Time.deltaTime;

        float progress = Mathf.Clamp01((startY - pos.y) / fallDistance);

        // bottom-left drift + sway (sway fades as he gets smaller)
        pos.x = startX + drift.x * progress
              + Mathf.Sin(time * wobbleSpeed) * wobbleAmount * (1f - progress);

        // shrink as he falls away
        hero.localScale = Vector3.Lerp(baseScale, baseScale * endScale, progress);
        hero.localRotation = Quaternion.Euler(0f, 0f,
            Mathf.Sin(time * tumbleSpeed) * tumbleAngle * (1f - progress));

        if (pos.y <= stopY)
        {
            // Reached the bottom-left: he vanishes into the distance...
            falling = false;
            hero.localRotation = Quaternion.identity;
            hero.gameObject.SetActive(false);
            if (lowEnergyGlow != null) lowEnergyGlow.SetActive(false);
            if (dizzyEffect != null) dizzyEffect.SetActive(false);

            // ...and the airplane scene takes over!
            if (rescue != null) rescue.BeginRescue();
            return;
        }

        hero.anchoredPosition = pos;
        UpdateEffectPositions();
    }

    void UpdateEffectPositions()
    {
        if (hero == null) return;

        if (lowEnergyGlow != null && lowEnergyGlow.activeSelf)
        {
            lowEnergyGlow.transform.position = hero.position + lowEnergyGlowOffset;
        }

        if (dizzyEffect != null && dizzyEffect.activeSelf)
        {
            dizzyEffect.transform.position = hero.position + dizzyEffectOffset;
        }
    }
}