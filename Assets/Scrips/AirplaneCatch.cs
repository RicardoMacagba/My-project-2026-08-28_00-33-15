using System.Collections;
using UnityEngine;

public class AirplaneCatch : MonoBehaviour
{
    [Header("Outgoing forest scene")]
    public RectTransform forestScene;
    public CanvasGroup forestSceneCanvas;
    public GameObject forestBG;
    public GameObject forestBG2;
    public GameObject asteroid;
    public float forestBackoutDuration = 0.35f;
    public float forestBackoutScale = 1.08f;

    [Header("Blackout handoff")]
    public CanvasGroup blackoutCanvas;
    public float blackoutFadeDuration = 0.08f;
    public float blackoutHoldDuration = 0.12f;
    public float blackoutRevealDuration = 0.2f;

    [Header("Inside airplane handoff")]
    public RectTransform insideAirplaneBG;
    public Vector2 insideAirplaneStartPosition = new Vector2(-40f, -27f);
    public Vector2 insideAirplaneTargetPosition = new Vector2(-2014f, -27f);
    public float insideAirplaneSlideDuration = 1f;
    public GameObject heroFlying;
    public GameObject lowEnergyHero;
    public Vector2 lowEnergyHeroStartPosition = new Vector2(-27f, -127f);
    public Vector2 lowEnergyHeroTargetPosition = new Vector2(-1988f, -124f);

    [Header("Background slide")]
    public RectTransform airplaneBG;
    public Vector2 bgTargetPosition = new Vector2(279.0001f, -1.6367f);
    public float bgSlideDuration = 1f;

    [Header("Hero's second fall (top-left into the plane)")]
    public RectTransform hero;
    public Vector2 heroStartPosition = new Vector2(-1057f, 470f); // top-left
    public Vector2 heroCatchPosition = new Vector2(43f, -77f);   // center of airplane bg
    public float heroFallDuration = 1.2f;
    public float heroStartScale = 0.4f;  // small = far away
    public float heroEndScale = 1f;      // full size when caught

    Vector3 heroBaseScale;
    Vector2 forestSceneStartPosition;
    Vector3 forestSceneStartScale;
    bool started;

    void Awake()
    {
        heroBaseScale = hero.localScale; // remember his true full size

        if (forestScene != null)
        {
            forestSceneStartPosition = forestScene.anchoredPosition;
            forestSceneStartScale = forestScene.localScale;
        }

        if (blackoutCanvas != null)
        {
            blackoutCanvas.alpha = 0f;
            blackoutCanvas.blocksRaycasts = false;
        }
    }

    public void BeginRescue()
    {
        if (started) return;   // safety: only run once
        started = true;
        StartCoroutine(RescueRoutine());
    }

    IEnumerator RescueRoutine()
    {
        // Phase 1: briefly hide the impact handoff behind a black screen.
        Coroutine forestBackout = StartCoroutine(BackOutForestScene());
        if (blackoutCanvas != null)
        {
            blackoutCanvas.blocksRaycasts = true;
            yield return FadeCanvasGroup(blackoutCanvas, 0f, 1f, blackoutFadeDuration);
            DisableForestScene();
            yield return new WaitForSeconds(blackoutHoldDuration);
        }

        // Phase 2: move the airplane into place while the screen is black.
        yield return MoveOver(airplaneBG, bgTargetPosition, bgSlideDuration);
        DisableForestScene();
        yield return forestBackout;

        if (blackoutCanvas != null)
        {
            yield return FadeCanvasGroup(
                blackoutCanvas,
                1f,
                0f,
                blackoutRevealDuration);
            blackoutCanvas.blocksRaycasts = false;
        }

        // Phase 3: hero reappears top-left, small
        hero.gameObject.SetActive(true);
        hero.localRotation = Quaternion.identity;
        hero.anchoredPosition = heroStartPosition;
        hero.localScale = heroBaseScale * heroStartScale;

        // Phase 4: falls diagonally to the catch point, growing as he approaches
        Vector2 startPos = heroStartPosition;
        Vector3 startScale = hero.localScale;
        Vector3 endScale = heroBaseScale * heroEndScale;

        float t = 0;
        while (t < heroFallDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / heroFallDuration));
            hero.anchoredPosition = Vector2.Lerp(startPos, heroCatchPosition, p);
            hero.localScale = Vector3.Lerp(startScale, endScale, p);
            yield return null;
        }

        hero.anchoredPosition = heroCatchPosition;
        hero.localScale = endScale;

        // Phase 5: cut to the inside of the airplane after the catch.
        yield return InsideAirplaneRoutine();
    }

    IEnumerator InsideAirplaneRoutine()
    {
        if (blackoutCanvas != null)
        {
            blackoutCanvas.blocksRaycasts = true;
            airplaneBG.gameObject.SetActive(false);
            yield return FadeCanvasGroup(blackoutCanvas, 0f, 1f, blackoutFadeDuration);
            yield return new WaitForSeconds(blackoutHoldDuration);
        }

        if (heroFlying != null)
        {
            heroFlying.SetActive(false);
        }
        else if (hero != null)
        {
            hero.gameObject.SetActive(false);
            
        }

        if (lowEnergyHero != null)
        {
            lowEnergyHero.SetActive(true);
        }

        if (insideAirplaneBG != null)
        {
            insideAirplaneBG.anchoredPosition = insideAirplaneStartPosition;
        }

        RectTransform lowEnergyHeroRect = null;
        if (lowEnergyHero != null)
        {
            lowEnergyHeroRect = lowEnergyHero.GetComponent<RectTransform>();
            if (lowEnergyHeroRect != null)
            {
                lowEnergyHeroRect.anchoredPosition = lowEnergyHeroStartPosition;
            }
        }

        yield return MoveInsideAirplane(insideAirplaneBG, lowEnergyHeroRect);

        if (blackoutCanvas != null)
        {
            yield return FadeCanvasGroup(
                blackoutCanvas,
                1f,
                0f,
                blackoutRevealDuration);
            blackoutCanvas.blocksRaycasts = false;
            airplaneBG.gameObject.SetActive(false);
            
        }
    }

    IEnumerator BackOutForestScene()
    {
        if (forestScene == null)
        {
            DisableForestScene();
            yield break;
        }

        float duration = Mathf.Max(0.01f, forestBackoutDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, progress);

            forestScene.localScale = Vector3.Lerp(
                forestSceneStartScale,
                forestSceneStartScale * forestBackoutScale,
                eased);
            forestScene.anchoredPosition = Vector2.Lerp(
                forestSceneStartPosition,
                forestSceneStartPosition + Vector2.down * 80f,
                eased);

            if (forestSceneCanvas != null)
            {
                forestSceneCanvas.alpha = 1f - eased;
            }

            yield return null;
        }

    }

    void DisableForestScene()
    {
        if (forestBG != null) forestBG.SetActive(false);
        if (forestBG2 != null) forestBG2.SetActive(false);
        if (asteroid != null) asteroid.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    IEnumerator MoveInsideAirplane(RectTransform background, RectTransform lowEnergyHeroRect)
    {
        if (background == null && lowEnergyHeroRect == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, insideAirplaneSlideDuration);
        Vector2 backgroundStart = background != null
            ? background.anchoredPosition
            : Vector2.zero;
        Vector2 heroStart = lowEnergyHeroRect != null
            ? lowEnergyHeroRect.anchoredPosition
            : Vector2.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration));

            if (background != null)
            {
                background.anchoredPosition = Vector2.Lerp(
                    backgroundStart,
                    insideAirplaneTargetPosition,
                    progress);
            }

            if (lowEnergyHeroRect != null)
            {
                lowEnergyHeroRect.anchoredPosition = Vector2.Lerp(
                    heroStart,
                    lowEnergyHeroTargetPosition,
                    progress);
            }

            yield return null;
        }

        if (background != null)
        {
            background.anchoredPosition = insideAirplaneTargetPosition;
        }

        if (lowEnergyHeroRect != null)
        {
            lowEnergyHeroRect.anchoredPosition = lowEnergyHeroTargetPosition;
        }
    }

    IEnumerator MoveOver(RectTransform rt, Vector2 target, float duration)
    {
        Vector2 start = rt.anchoredPosition;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            rt.anchoredPosition = Vector2.Lerp(start, target, p);
            yield return null;
        }
        rt.anchoredPosition = target;
    }
}