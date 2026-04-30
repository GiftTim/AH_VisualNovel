using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 미션 UI를 활성화하고
/// Background Image를 페이드 인시키는 매니저.
/// </summary>
public class MissionManager : MonoBehaviour
{
    [Header("배경 이미지")]
    [SerializeField] private GameObject _backgroundObject;
    [SerializeField] private Image _backgroundImage;

    [Header("퀘스트 UI")]
    [SerializeField] private GameObject _questUI;

    [Header("카운트다운")]
    [SerializeField] private SliderCountdown _sliderCountdown;

    [Header("Fade 설정")]
    [SerializeField] private float _fadeDuration = 0.5f;

    private Tween _fadeTween;

    /// <summary>
    /// 버튼 OnClick에 연결.
    /// Background와 QuestUI를 활성화하고 Fade In 실행.
    /// </summary>
    public void OpenMissionUI()
    {
        Debug.Log($"{gameObject.name} StartCountdown");

        _sliderCountdown?.StopCountdown();

        if (_backgroundObject != null)
            _backgroundObject.SetActive(true);

        if (_questUI != null)
            _questUI.SetActive(true);

        if (_backgroundImage == null)
            return;

        _fadeTween?.Kill();

        Color color = _backgroundImage.color;
        color.a = 0f;
        _backgroundImage.color = color;

        _fadeTween = _backgroundImage
            .DOFade(1f, _fadeDuration)
            .SetEase(Ease.Linear);
    }


    /// <summary>
    /// 
    /// Fade Out 후 Background와 QuestUI를 비활성화한다.
    /// </summary>
    public void CloseMissionUI()
    {
        if (_backgroundImage == null)
            return;

        _fadeTween?.Kill();

        _fadeTween = _backgroundImage
            .DOFade(0f, _fadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (_backgroundObject != null)
                    _backgroundObject.SetActive(false);

                if (_questUI != null)
                    _questUI.SetActive(false);
            });

            _sliderCountdown?.StartCountdown();
    }
}
