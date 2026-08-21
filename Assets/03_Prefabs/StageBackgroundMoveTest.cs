using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StageBackgroundScroller : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private RectTransform scrollingContent;
    [SerializeField] private RectTransform viewport;

    private Vector2 startPosition;

    private void Awake()
    {
        if (scrollingContent == null)
        {
            scrollingContent = transform.Find("ScrollingBG") as RectTransform;

            if (scrollingContent == null)
            {
                scrollingContent = GetComponent<RectTransform>();
            }
        }

        if (viewport == null)
        {
            viewport = scrollingContent.parent as RectTransform;
        }

        if (scrollingContent == null || viewport == null)
        {
            Debug.LogError(
                "StageBackgroundScroller: ScrollingBG 또는 Viewport를 찾을 수 없습니다.",
                this
            );
            enabled = false;
            return;
        }

        Canvas.ForceUpdateCanvases();
        startPosition = scrollingContent.anchoredPosition;
    }

    private void Update()
    {
        if (viewport == null)
        {
            return;
        }

        float maxMoveDistance = Mathf.Max(
            0f,
            scrollingContent.rect.width - viewport.rect.width
        );

        if (maxMoveDistance <= 0f)
        {
            scrollingContent.anchoredPosition = startPosition;
            return;
        }

        float offsetX = Mathf.PingPong(
            Time.unscaledTime * speed,
            maxMoveDistance
        );

        Vector2 position = startPosition;
        position.x -= offsetX;

        scrollingContent.anchoredPosition = position;
    }
}
