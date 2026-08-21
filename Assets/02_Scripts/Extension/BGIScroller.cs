using UnityEngine;
using UnityEngine.UI;

public class BGIScroller : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 50f;
    [SerializeField] private RectTransform _bg1;
    [SerializeField] private RectTransform _bg2;
    [SerializeField] private Image _image1;
    [SerializeField] private Image _image2;

    private float _imageWidth;

    private void Awake()
    {
        if (_bg1 != null) _imageWidth = _bg1.rect.width;
    }

    private void Start()
    {
        ResetPositions();
    }

    private void Update()
    {
        float moveDelta = _scrollSpeed * Time.deltaTime;
        _bg1.anchoredPosition -= new Vector2(moveDelta, 0f);
        _bg2.anchoredPosition -= new Vector2(moveDelta, 0f);

        if (_bg1.anchoredPosition.x >= _imageWidth)
            _bg1.anchoredPosition = new Vector2(_bg2.anchoredPosition.x + _imageWidth, _bg1.anchoredPosition.y);

        if (_bg2.anchoredPosition.x >= _imageWidth)
            _bg2.anchoredPosition = new Vector2(_bg1.anchoredPosition.x + _imageWidth, _bg2.anchoredPosition.y);
    }

    public void ResetPositions()
    {
        if (_bg1 != null && _bg2 != null)
        {
            _bg1.anchoredPosition = new Vector2(0f, _bg1.anchoredPosition.y);
            _bg2.anchoredPosition = new Vector2(-_imageWidth, _bg2.anchoredPosition.y);
        }
    }

    // StageManager가 이미지를 바꿀 때 호출할 함수
    public void SetBackgroundSprite(Sprite newSprite)
    {
        if (_image1 != null) _image1.sprite = newSprite;
        if (_image2 != null) _image2.sprite = newSprite;
    }
}
