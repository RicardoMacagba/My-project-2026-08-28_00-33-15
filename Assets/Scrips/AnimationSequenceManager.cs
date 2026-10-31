using System.Collections;
using UnityEngine;

public class AnimationSequenceManager : MonoBehaviour
{
    [Header("Characters - drag from Hierarchy")]
    public RectTransform kiddoRun;        // Kiddo_Move
    public RectTransform kiddoLowEnergy;  // NoEnergy
    // public RectTransform textSuperCharged;
    // public RectTransform frameSuperCharged;
    public RectTransform kiddoAmazed;     // Kiddo_Amazed
    public RectTransform hero;      // GoHero
    public RectTransform textHereComesGoHero;      // Text here comes GoHero
    public RectTransform kiddoSuperHero;  // kiddo_GoHero

    [Tooltip("Optional - leave empty if you don't have it yet")]
    public RectTransform goPowerEffect;

    [Header("Go Foods")]
public RectTransform bread;
public RectTransform rice;
public RectTransform corn;
public RectTransform potato;
public RectTransform pancake;

[Header("Power Glow")]
public RectTransform glowEffect;

[Header("Go Foods Magic")]
public float magicDelay = 0.3f;
public float foodFlyDuration = 0.8f;
public float foodDelay = 0.15f;
public float glowDuration = 2f;

[Header("Food Entry Effects")]
public RectTransform effect1;
public RectTransform effect2;
public RectTransform effect3;

[Header("Hero flight")]
public RectTransform heroStandPoint;

public Vector2 heroStartPosition = new Vector2(-900f, 450f);

public float heroFlyDuration = 1.5f;

public Vector3 heroStartScale = new Vector3(0.5f, 0.5f, 1f);

    [Header("Timing (seconds)")]
    public float runDuration = 4f;
    public bool loop = false;

    [Header("Close-up zoom")]
    public Animator sceneContainerAnimator;

    void Awake()
    {
        // Clean start: only the runner visible
        kiddoRun.gameObject.SetActive(true);
        kiddoLowEnergy.gameObject.SetActive(false);
        kiddoAmazed.gameObject.SetActive(false);
        hero.gameObject.SetActive(false);
        textHereComesGoHero.gameObject.SetActive(false);
        // textSuperCharged.gameObject.SetActive(false);
        // frameSuperCharged.gameObject.SetActive(false);
        kiddoSuperHero.gameObject.SetActive(false);

        if (goPowerEffect != null)
            goPowerEffect.gameObject.SetActive(false);

            bread.gameObject.SetActive(false);

            rice.gameObject.SetActive(false);
            corn.gameObject.SetActive(false);
            potato.gameObject.SetActive(false);
            pancake.gameObject.SetActive(false);

            glowEffect.gameObject.SetActive(false);

            effect1.gameObject.SetActive(false);
            effect2.gameObject.SetActive(false);
            effect3.gameObject.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(PlaySequence());
        StartCoroutine(FreezeSceneContainer());
    }

    IEnumerator PlaySequence()
    {
        // 1. Let the running animation play
        yield return new WaitForSeconds(runDuration);

        // 2. Stop/hide the running Kiddo
        kiddoRun.gameObject.SetActive(false);

        // 3. Show the Low Energy Kiddo
        kiddoLowEnergy.gameObject.SetActive(true);
        

        // 4. Hero flies in
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FlyHero());
    }

IEnumerator FlyHero()
{
    // Show Hero
    hero.gameObject.SetActive(true);
    textHereComesGoHero.gameObject.SetActive(true);

    // Remember Hero's normal scale
    Vector3 normalScale = hero.localScale;

    // -------------------------
    // START: top-left
    // -------------------------
    Vector2 startPosition = new Vector2(-900f, 450f);

    // -------------------------
    // TOP-RIGHT position
    // Change this number if needed
    // -------------------------
    Vector2 skyRightPosition = new Vector2(800f, 450f);

    // -------------------------
    // FINAL: Low Energy Kiddo
    // -------------------------
    //Vector2 targetPosition = kiddoLowEnergy.anchoredPosition;
    Vector2 targetPosition = new Vector2(272f, -113f);

    // Start very small
    Vector3 tinyScale = new Vector3(-0.1f, 0.1f, 1f);

    hero.anchoredPosition = startPosition;
    hero.localScale = tinyScale;


    // ==========================================
    // PART 1: Fly across the sky
    // ==========================================

    float time = 0f;

    while (time < heroFlyDuration)
    {
        time += Time.deltaTime;

        float t = Mathf.Clamp01(time / heroFlyDuration);

        // Move LEFT → RIGHT
        hero.anchoredPosition = Vector2.Lerp(
            startPosition,
            skyRightPosition,
            t
        );

        // Stay tiny
        hero.localScale = tinyScale;

        yield return null;
    }


    // Make sure Hero is exactly at top-right
    hero.anchoredPosition = skyRightPosition;
    hero.localScale = tinyScale;


    // ==========================================
    // PART 2: Fly DOWN to Low Energy Kiddo
    // ==========================================

    float downDuration = 1.2f;
    time = 0f;

    while (time < downDuration)
    {
        time += Time.deltaTime;

        float t = Mathf.Clamp01(time / downDuration);

        // Move from top-right → Kiddo
        hero.anchoredPosition = Vector2.Lerp(
            skyRightPosition,
            targetPosition,
            t
        );

        // Grow from tiny → normal
        hero.localScale = Vector3.Lerp(
            tinyScale,
            normalScale,
            t
        );

        yield return null;
    }


    // Final position and normal size
    hero.anchoredPosition = targetPosition;
    hero.localScale = normalScale;

    // Change Low Energy Kiddo → Amazed Kiddo
kiddoLowEnergy.gameObject.SetActive(false);
kiddoAmazed.gameObject.SetActive(true);
//frameSuperCharged.gameObject.SetActive(true);
textHereComesGoHero.gameObject.SetActive(false);

StartCoroutine(PlayGoFoodsMagic());
}

IEnumerator PlayGoFoodsMagic()
{
    yield return new WaitForSeconds(magicDelay);

    // Turn on the glow
    glowEffect.gameObject.SetActive(true);
    //textSuperCharged.gameObject.SetActive(true);

    // Start glow small
    Vector3 glowNormalScale = glowEffect.localScale;
    glowEffect.localScale = Vector3.zero;

    // Start expanding the glow
    StartCoroutine(ExpandGlow(glowNormalScale, glowDuration));

    // Send the five Go Foods into the Kiddo
    yield return StartCoroutine(FlyFood(bread));
    StartCoroutine(PlayFoodEntryEffect(effect1));
    yield return new WaitForSeconds(foodDelay);

    yield return StartCoroutine(FlyFood(rice));
    StartCoroutine(PlayFoodEntryEffect(effect2));
    yield return new WaitForSeconds(foodDelay);

    yield return StartCoroutine(FlyFood(corn));
    StartCoroutine(PlayFoodEntryEffect(effect3));
    yield return new WaitForSeconds(foodDelay);

    yield return StartCoroutine(FlyFood(potato));
    StartCoroutine(PlayFoodEntryEffect(effect1));
    yield return new WaitForSeconds(foodDelay);

    yield return StartCoroutine(FlyFood(pancake));
    StartCoroutine(PlayFoodEntryEffect(effect2));

    // Wait a moment after the last food enters
    yield return new WaitForSeconds(0.5f);

    // Turn off the glow
    glowEffect.gameObject.SetActive(false);

    // Hide Amazed Kiddo
    kiddoAmazed.gameObject.SetActive(false);

    // Show Super Hero Kiddo
    kiddoSuperHero.gameObject.SetActive(true);
}

IEnumerator FlyFood(RectTransform food)
{
    if (food == null)
        yield break;

    // Remember where the food was originally placed
    Vector2 startPosition = food.anchoredPosition;

    // Target = Amazed Kiddo
    Vector2 targetPosition = kiddoAmazed.anchoredPosition;

    // Show the food
    food.gameObject.SetActive(true);

    // Start tiny
    food.localScale = Vector3.zero;

    float time = 0f;

    while (time < foodFlyDuration)
    {
        time += Time.deltaTime;

        float t = Mathf.Clamp01(time / foodFlyDuration);

        // Move toward Amazed Kiddo
        food.anchoredPosition = Vector2.Lerp(
            startPosition,
            targetPosition,
            t
        );

        // Grow 0 → 1
        food.localScale = Vector3.Lerp(
            Vector3.zero,
            Vector3.one,
            t
        );

        yield return null;
    }

    // Make sure it finishes exactly at Kiddo
    food.anchoredPosition = targetPosition;
    food.localScale = Vector3.one;

    // Food has entered Kiddo — hide it
    food.gameObject.SetActive(false);
}

IEnumerator ExpandGlow(Vector3 normalScale, float duration)
{
    float time = 0f;

    while (time < duration)
    {
        time += Time.deltaTime;

        float t = Mathf.Clamp01(time / duration);

        glowEffect.localScale = Vector3.Lerp(
            Vector3.zero,
            normalScale,
            t
        );

        yield return null;
    }

    glowEffect.localScale = normalScale;
}

IEnumerator PlayFoodEntryEffect(RectTransform effect)
{
    effect.gameObject.SetActive(true);

    Vector3 normalScale = effect.localScale;

    // Start tiny
    effect.localScale = Vector3.zero;

    float time = 0f;
    float duration = 0.25f;

    while (time < duration)
    {
        time += Time.deltaTime;

        float t = Mathf.Clamp01(time / duration);

        effect.localScale = Vector3.Lerp(
            Vector3.zero,
            normalScale,
            t
        );

        yield return null;
    }

    effect.localScale = normalScale;

    // Stay visible briefly
    yield return new WaitForSeconds(0.25f);

    effect.gameObject.SetActive(false);
}

IEnumerator FreezeSceneContainer()
{
    // Wait for the zoom animation to finish
    yield return new WaitForSeconds(25f);

    // Freeze SceneContainer exactly where the animation left it
    sceneContainerAnimator.enabled = false;
}
}