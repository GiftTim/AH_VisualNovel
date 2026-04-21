using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼에 부착.
/// StartCountdown()을 Button OnClick에 연결하면
/// Slider value가 1에서 0으로 duration 초에 걸쳐 감소한다.
/// </summary>
public class SliderCountdown : MonoBehaviour
{
    [Header("연결 대상")]
    [SerializeField] private Slider _slider;

    [Header("타이머 설정")]
    [SerializeField] private float _duration = 10f;   // Inspector에서 직접 입력

    public event Action OnCountdownCompleted;

    private Coroutine _countdownCoroutine;

    /// <summary>
    /// 카운트다운을 시작한다.
    /// 이미 진행 중인 카운트다운이 있으면 무시 (초기화하지 않음).
    /// </summary>
    public void StartCountdown()
    {
        if (_countdownCoroutine != null) return;

        _countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        if (_slider == null)
        {
            _countdownCoroutine = null;
            OnCountdownCompleted?.Invoke();
            yield break;
        }

        _slider.value = 1f;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            _slider.value = 1f - Mathf.Clamp01(elapsed / _duration);
            yield return null;
        }

        _slider.value = 0f;
        _countdownCoroutine = null;
        OnCountdownCompleted?.Invoke();
    }
}
