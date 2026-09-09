using UnityEngine;
using UnityEngine.UI;

public class GameLock : MonoBehaviour
{
    [Header("Which Game Is This?")]
    [Tooltip("Enter the game number: 2, 3, 4, or 5.")]
    public int gameNumber = 2;

    [Header("Lock / Unlock Visual")]
    public Image lockUnlockImage;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private GameProgressManager progressManager;


    private void OnEnable()
    {
        progressManager =
            FindFirstObjectByType<GameProgressManager>();

        RefreshLock();
    }


    public void RefreshLock()
    {
        if (progressManager == null)
        {
            progressManager =
                FindFirstObjectByType<GameProgressManager>();
        }

        if (progressManager == null)
        {
            Debug.LogWarning(
                "⚠️ GameLock: GameProgressManager was not found."
            );
            return;
        }

        bool unlocked = false;

        switch (gameNumber)
        {
            case 2:
                unlocked = progressManager.IsGame2Unlocked();
                break;

            case 3:
                unlocked = progressManager.IsGame3Unlocked();
                break;

            case 4:
                unlocked = progressManager.IsGame4Unlocked();
                break;

            case 5:
                unlocked = progressManager.IsGame5Unlocked();
                break;

            default:
                Debug.LogWarning(
                    "⚠️ GameLock: gameNumber must be 2, 3, 4, or 5."
                );
                return;
        }


        if (lockUnlockImage != null)
        {
            lockUnlockImage.sprite =
                unlocked
                ? unlockedSprite
                : lockedSprite;
        }

        Debug.Log(
            "🔐 Game "
            + gameNumber
            + " | Unlocked = "
            + unlocked
        );
    }


    public bool IsUnlocked()
    {
        if (progressManager == null)
        {
            progressManager =
                FindFirstObjectByType<GameProgressManager>();
        }

        if (progressManager == null)
        {
            return false;
        }

        switch (gameNumber)
        {
            case 2:
                return progressManager.IsGame2Unlocked();

            case 3:
                return progressManager.IsGame3Unlocked();

            case 4:
                return progressManager.IsGame4Unlocked();

            case 5:
                return progressManager.IsGame5Unlocked();

            default:
                return false;
        }
    }
}