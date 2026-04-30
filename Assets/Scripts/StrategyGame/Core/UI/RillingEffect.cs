using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 주사위 굴리기 연출.
/// Animator.speed 감속 + 흔들림 + 던지기/착지 Scale Punch 를 조합한다.
/// Dice 오브젝트에 부착 후 Roll() 을 호출하면 된다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RillingEffect : MonoBehaviour
{
    [Header("감속 설정")]
    [SerializeField] private float _startSpeed       = 3f;      // 초기 애니메이션 배속
    [SerializeField] private float _rollDuration     = 1.5f;    // 감속 구간 길이(초)
    [SerializeField] private Ease  _decelerationEase = Ease.InQuad;

    [Header("흔들림")]
    [SerializeField] private float _shakeStrength    = 10f;
    [SerializeField] private int   _shakeVibrato     = 20;
    [SerializeField] private float _shakeRandomness  = 90f;

    [Header("던지기 반동 (Scale Punch)")]
    [SerializeField] private Vector3 _throwPunch     = new Vector3(0.25f, 0.25f, 0f);
    [SerializeField] private float   _throwDuration  = 0.18f;

    [Header("착지 반동 (Scale Punch)")]
    [SerializeField] private Vector3 _landPunch      = new Vector3(0.2f, -0.2f, 0f);
    [SerializeField] private float   _landDuration   = 0.22f;

    // ── 내부 ──────────────────────────────────────────────
    private Animator      _animator;
    private RectTransform _rect;
    private Sequence      _seq;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rect     = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        _seq?.Kill();
    }

    // ── Public API ────────────────────────────────────────

    /// <summary>
    /// 주사위 굴리기 연출을 시작한다.
    /// onComplete : 연출이 완전히 끝난 뒤 호출되는 콜백.
    /// </summary>
    public void Roll(Action onComplete = null)
    {
        _seq?.Kill(complete: false);
        _animator.speed = _startSpeed;

        _seq = DOTween.Sequence();

        // ① 던지는 순간 - Scale Punch
        _seq.Append(
            transform.DOPunchScale(_throwPunch, _throwDuration, vibrato: 5, elasticity: 0.5f)
        );

        // ② 감속 (Animator.speed: startSpeed → 0)
        _seq.Append(
            DOVirtual.Float(_startSpeed, 0f, _rollDuration,
                            v => _animator.speed = v)
                     .SetEase(_decelerationEase)
        );

        // ③ 흔들림 - 감속과 동시에, fadeOut으로 자연스럽게 수렴
        if (_rect != null)
        {
            _seq.Join(
                _rect.DOShakeAnchorPos(
                    _rollDuration,
                    _shakeStrength,
                    _shakeVibrato,
                    _shakeRandomness,
                    snapping: false,
                    fadeOut:  true)
            );
        }

        // ④ 착지 반동
        _seq.Append(
            transform.DOPunchScale(_landPunch, _landDuration, vibrato: 4, elasticity: 0.5f)
        );

        _seq.OnComplete(() => onComplete?.Invoke());
        _seq.Play();
    }

    /// <summary>진행 중인 연출을 즉시 중단하고 Animator 속도를 정상화한다.</summary>
    public void Stop()
    {
        _seq?.Kill(complete: false);
        _animator.speed = 1f;
    }
}
