using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 마우스를 올리면 크기를 키우고 대상 Image의 색을 바꾼다.
/// 마우스가 떠나면 원래 상태로 되돌린다.
/// 여러 버튼에 재사용 가능.
/// </summary>
public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover 대상")]
    [SerializeField] private Image _targetImage;

    [Header("Scale")]
    [SerializeField] private float _hoverScale    = 1.2f;
    [SerializeField] private float _scaleDuration = 0.2f;

    [Header("Color")]
    [SerializeField] private Color _normalColor   = Color.white;
    [SerializeField] private Color _hoverColor    = new Color(0x31 / 255f, 0xDC / 255f, 0xCB / 255f); // #31DCCB
    [SerializeField] private float _colorDuration = 0.2f;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;

        if (_targetImage != null)
            _targetImage.color = _normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(_originalScale * _hoverScale, _scaleDuration).SetEase(Ease.OutBack);

        if (_targetImage != null)
            _targetImage.DOColor(_hoverColor, _colorDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_originalScale, _scaleDuration).SetEase(Ease.OutBack);

        if (_targetImage != null)
            _targetImage.DOColor(_normalColor, _colorDuration);
    }
}
