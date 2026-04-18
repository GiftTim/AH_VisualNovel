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

    /// <summary>목적지 도착 시 발생하는 이벤트. Inspector에서 리스너 연결 가능.</summary>
    public UnityEvent OnArrived;

    /// <summary>현재 실행 중인 이동 코루틴 참조. 재이동 요청 시 중단에 사용.</summary>
    private Coroutine _moveCoroutine;

    /// <summary>
    /// 외부에서 이동을 시작할 때 호출.
    /// 이동 중일 경우 요청을 무시한다.
    /// </summary>
    /// <param name="path">이동 경로 (HQ → waypoints → 목적지 순서의 Transform 배열).</param>
    /// <param name="onArrived">도착 시 실행할 콜백 (선택적).</param>
    public void MoveAlong(Transform[] path, Action onArrived = null)
    {
        if (_moveCoroutine != null) return;
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

            // t가 1에 도달할 때까지 매 프레임 위치 보간
            while (t < 1f)
            {
                t += Time.deltaTime * _moveSpeed;
                transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
        }

        // 도착 콜백 실행 (코드에서 직접 전달한 Action)
        onArrived?.Invoke();
        // 도착 이벤트 실행 (Inspector에서 연결된 리스너)
        OnArrived?.Invoke();

        // 이동 완료 → 다음 MoveAlong() 요청을 수락할 수 있도록 초기화
        _moveCoroutine = null;
    }
}
