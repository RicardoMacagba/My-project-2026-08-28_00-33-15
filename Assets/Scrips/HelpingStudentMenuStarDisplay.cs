using UnityEngine;
using UnityEngine.UI;

public class HelpingStudentMenuStarDisplay : MonoBehaviour
{
    public Image star1Image;
    public Image star2Image;
    public Image star3Image;

    public Sprite emptyStarSprite;
    public Sprite glowingStarSprite;

    private void OnEnable()
    {
        RefreshMenuStars();
    }

    public void RefreshMenuStars()
    {
        if (star1Image == null ||
            star2Image == null ||
            star3Image == null)
        {
            Debug.LogWarning(
                "HelpingStudentMenuStarDisplay is missing star references."
            );
            return;
        }

        int savedStars =
            PlayerPrefs.GetInt(
                "HelpingStudent_HighScoreStars",
                0
            );

        savedStars = Mathf.Clamp(savedStars, 0, 3);

        // Start with all stars white
        star1Image.sprite = emptyStarSprite;
        star2Image.sprite = emptyStarSprite;
        star3Image.sprite = emptyStarSprite;

        // Turn earned stars yellow
        if (savedStars >= 1)
            star1Image.sprite = glowingStarSprite;

        if (savedStars >= 2)
            star2Image.sprite = glowingStarSprite;

        if (savedStars >= 3)
            star3Image.sprite = glowingStarSprite;
    }
}