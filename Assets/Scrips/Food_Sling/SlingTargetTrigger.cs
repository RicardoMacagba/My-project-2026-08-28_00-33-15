using UnityEngine;

public class SlingTargetTrigger : MonoBehaviour
{
    private SlingGameManager gameManager;

    public void Initialize(SlingGameManager manager)
    {
        gameManager = manager;

        Collider2D targetCollider = GetComponent<Collider2D>();
        if (targetCollider == null)
        {
            RectTransform targetRect = GetComponent<RectTransform>();
            if (targetRect == null)
            {
                Debug.LogError("Sling target needs a Collider2D or RectTransform.", this);
                enabled = false;
                return;
            }

            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = targetRect.rect.size;
            targetCollider = boxCollider;
        }

        targetCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        UISlingshotItem slingItem = other.GetComponentInParent<UISlingshotItem>();
        if (slingItem != null)
            gameManager.TryConvertTarget(gameObject, slingItem);
    }
}
