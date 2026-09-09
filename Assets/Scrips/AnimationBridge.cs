using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    // Drag your main manager object into this slot in the Inspector
    public HeroSelectionManager selectionManager;

    // This is the function Unity's animation event will talk to!
    public void TriggerGameplayHandoff()
    {
        if (selectionManager != null)
        {
            selectionManager.ActivateActualGameplay();
        }
    }
}
