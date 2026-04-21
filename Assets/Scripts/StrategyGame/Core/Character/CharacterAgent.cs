using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 캐릭터 오브젝트에 붙는 이동 컴포넌트.
/// BuildingLineConnector.GetWaypoints()로 받은 경로를 따라 순서대로 이동한다.
/// </summary>
public class CharacterAgent : MonoBehaviour
{
    /// <summary>이동 속도. 값이 클수록 빠름 (Lerp t 증가 속도에 직접 영향).</summary>
    [SerializeField] float _moveSpeed = 2f;

    /// <summary>캐릭터가 지나간 자리에 트레일 효과를 그리는 컴포넌트.</summary>
    [SerializeField] TrailRenderer _trail;

    /// <summary>도착 시 가장 먼저 숨길 마스크 오브젝트.</summary>
    [SerializeField] private GameObject _mask;

    /// <summary>목적지 도착 시 발생하는 이벤트. Inspector에서 리스너 연결 가능.</summary>
    public UnityEvent OnArrived;

    /// <summary>현재 실행 중인 이동 코루틴 참조. 재이동 요청 시 중단에 사용.</summary>
    private Coroutine _moveCoroutine;

    private void Start()
    {
        if (_trail != null) _trail.emitting = false;
    }

    /// <summary>
    /// 외부에서 이동을 시작할 때 호출.
    /// 이동 중일 경우 기존 코루틴을 중단하고 처음부터 재이동한다.
    /// </summary>
    /// <param name="path">이동 경로 (HQ → waypoints → 목적지 순서의 Transform 배열).</param>
    /// <param name="onArrived">도착 시 실행할 콜백 (선택적).</param>
    public void MoveAlong(Transform[] path, Action onArrived = null)
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

        if (_trail != null)
        {
            _trail.Clear();
            _trail.emitting = true;
        }

        _moveCoroutine = StartCoroutine(MoveCoroutine(path, onArrived));
    }

    /// <summary>
    /// path 배열을 순서대로 구간 이동하는 코루틴.
    /// 각 구간을 Vector3.Lerp로 보간하며 이동한다.
    /// </summary>
    /// <param name="path">이동 경로 Transform 배열.</param>
    /// <param name="onArrived">도착 시 실행할 콜백.</param>
    IEnumerator MoveCoroutine(Transform[] path, Action onArrived)
    {
        // 경로가 없거나 점이 1개 이하면 이동 불가
        if (path == null || path.Length < 2) yield break;

        // path[i] → path[i+1] 구간을 순서대로 이동
        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 from = path[i].position;
            Vector3 to   = path[i + 1].position;
            float t = 0f;

            // t가 1에 도달할 때까지 매 프레임 위치 보간(보간(Interpolation)은 알려진 데이터 지점들 사이의 공간에 새로운 값을 추정하여 채워 넣는 기술)
            while (t < 1f)
            {
                t += Time.deltaTime * _moveSpeed;
                transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
        }

        if (_trail != null) _trail.emitting = false;

        // 1. 마스크 먼저 비활성화
        if (_mask != null) _mask.SetActive(false);

        // 2. 트레일이 완전히 사라질 때까지 대기
        float trailFadeTime = (_trail != null) ? _trail.time : 0f;
        if (trailFadeTime > 0f)
            yield return new WaitForSeconds(trailFadeTime);

        // 3. Mari 활성화
        gameObject.SetActive(true);

        // 도착 콜백 실행 (코드에서 직접 전달한 Action)
        onArrived?.Invoke();
        // 도착 이벤트 실행 (Inspector에서 연결된 리스너)
        OnArrived?.Invoke();
    }
}
