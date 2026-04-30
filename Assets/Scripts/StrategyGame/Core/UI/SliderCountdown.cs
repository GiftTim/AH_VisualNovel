using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slider 값을 시간에 따라 감소시키는 카운트다운 컴포넌트.
/// </summary>
public class SliderCountdown : MonoBehaviour
{
    [Header("연결 대상")]
    [SerializeField] private Slider _slider;

    [Header("카운트다운 설정")]
    [SerializeField] private float _duration = 10f;

    /// <summary>
    /// 카운트다운이 끝났을 때 호출되는 이벤트.
    /// </summary>
    public event Action OnCountdownCompleted;

    /// <summary>
    /// 현재 실행 중인 카운트다운 코루틴.
    /// </summary>
    private Coroutine _countdownCoroutine;

    /// <summary>
    /// 현재까지 경과한 시간.
    /// </summary>
    private float _elapsed;

    /// <summary>
    /// 카운트다운을 시작한다.
    /// 이미 실행 중이면 무시한다.
    /// </summary>
    public void StartCountdown()
    {
        if (_countdownCoroutine != null) return;

        _countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// 현재 진행 중인 카운트다운을 중지한다.
    /// 진행 상태는 유지된다.
    /// </summary>
    public void StopCountdown()
    {
        if (_countdownCoroutine == null) return;

        StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = null;
    }

    /// <summary>
    /// 카운트다운을 초기 상태로 되돌린다.
    /// Slider 값은 1로 복구된다.
    /// </summary>
    public void ResetCountdown()
    {
        StopCountdown();

        _elapsed = 0f;

        if (_slider != null)
            _slider.value = 1f;
    }

    /// <summary>
    /// 카운트다운을 실제로 진행하는 코루틴.
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        if (_slider == null)
        {
            _countdownCoroutine = null;
            yield break;
        }

        while (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;

            _slider.value = 1f - Mathf.Clamp01(_elapsed / _duration);

            yield return null;
        }

        _slider.value = 0f;

        _elapsed = 0f;
        _countdownCoroutine = null;

        OnCountdownCompleted?.Invoke();
    }
}