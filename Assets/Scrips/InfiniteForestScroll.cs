using UnityEngine;

public class InfiniteForestScroll : MonoBehaviour
{
    public RectTransform forest1;
    public RectTransform forest2;

    public float scrollSpeed = 150f;
    public float recycleDistance = 3240f;

    void Update()
    {
        float move = scrollSpeed * Time.deltaTime;

        forest1.anchoredPosition += Vector2.down * move;
        forest2.anchoredPosition += Vector2.down * move;

        if (forest1.anchoredPosition.y <= -recycleDistance)
        {
            forest1.anchoredPosition += Vector2.up * recycleDistance * 2f;
        }

        if (forest2.anchoredPosition.y <= -recycleDistance)
        {
            forest2.anchoredPosition += Vector2.up * recycleDistance * 2f;
        }
    }
}