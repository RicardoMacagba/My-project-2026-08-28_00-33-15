using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionManager : MonoBehaviour
{
    public GameplayTimerManager levelTimer; // Reference to your countdown clock object

    [Header("STANDING Menu Sprites")]
    public Sprite boyStandingSprite;
    public Sprite girlStandingSprite;

    [Header("ACTION Flying Sprites")]
    public Sprite boyFlyingSprite;
    public Sprite girlFlyingSprite;

    [Header("UI Layout Elements")]
    public Image characterDisplayImage;
    public GameObject selectionContainer;
    public Image introJumpingCharacter;
    public GameObject gameplayHUDLayer;

    [Header("Scene Transition Graphics")]
    public GameObject cargoHoldBackground;
    public GameObject airplaneCockpit;
    public GameObject scrollingForestMap;

    [Header("Animation Setup")]
    public Animator jumpAnimator;

    [Header("Gameplay Objects")]
    public GameObject actualGameplayHero;
    
    // 🚨 ADDED LOGIC: This gives the script access to your spawner!
    public ObstacleSpawner skySpawner; 

    private bool isBoySelected = true;

    public void ToggleCharacterSelection()
    {
        isBoySelected = !isBoySelected;
        if (characterDisplayImage != null)
            characterDisplayImage.sprite = isBoySelected ? boyStandingSprite : girlStandingSprite;
    }

    public void LetsFlyClicked()
    {
        if (selectionContainer != null)
            selectionContainer.SetActive(false);

        if (introJumpingCharacter != null)
        {
            introJumpingCharacter.sprite = isBoySelected ? boyFlyingSprite : girlFlyingSprite;
            introJumpingCharacter.gameObject.SetActive(true);
        }

        if (jumpAnimator != null)
        {
            jumpAnimator.SetTrigger("PlayLeap");
        }

        Invoke(nameof(ActivateActualGameplay), 1.0f);
    }

    public void ActivateActualGameplay()
    {
        if (introJumpingCharacter != null)
        {
            introJumpingCharacter.gameObject.SetActive(false);
        }

        if (cargoHoldBackground != null) cargoHoldBackground.SetActive(false);
        if (airplaneCockpit != null) airplaneCockpit.SetActive(false);

        if (scrollingForestMap != null)
            scrollingForestMap.SetActive(true);

        if (gameplayHUDLayer != null)
            gameplayHUDLayer.SetActive(true);

        if (actualGameplayHero != null)
        {
            actualGameplayHero.SetActive(true);

            SpriteRenderer playerRenderer = actualGameplayHero.GetComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                playerRenderer.sprite = isBoySelected ? boyFlyingSprite : girlFlyingSprite;
            }
        }

        // 🚨 ADDED LOGIC: Kick off the randomized item spawning routine now!
        if (skySpawner != null)
        {
            skySpawner.StartSpawningGameplay();
        }

        if (levelTimer != null)
        {
            levelTimer.StartLevelCountdown();
        }

        Debug.Log("SCENE SWAP COMPLETE! Forest map, flight HUD, and Sky Spawner are active.");
    }

    private void Start()
    {
        if (characterDisplayImage != null && boyStandingSprite != null)
            characterDisplayImage.sprite = boyStandingSprite;

        if (introJumpingCharacter != null)
            introJumpingCharacter.gameObject.SetActive(false);

        if (actualGameplayHero != null)
            actualGameplayHero.SetActive(false);

        if (cargoHoldBackground != null)
            BronzeStartupHold(true);

        if (scrollingForestMap != null)
            scrollingForestMap.SetActive(false);

        if (gameplayHUDLayer != null)
            gameplayHUDLayer.SetActive(false);
    }

    private void BronzeStartupHold(bool state)
    {
        if (cargoHoldBackground != null) cargoHoldBackground.SetActive(state);
    }

    public bool IsBoySelected()
    {
        return isBoySelected;
    }
}
