using UnityEngine;

/// <summary>
/// HQ와 각 건물을 잇는 경로 데이터.
/// </summary>
[System.Serializable]
public class RoadPath
{
    /// <summary>목적지 건물 Spot.</summary>
    public Spot buildingSpot;
    /// <summary>경로를 시각적으로 표시하는 LineRenderer.</summary>
    public LineRenderer lineRenderer;
    /// <summary>HQ와 목적지 사이의 중간 경유 지점 목록.</summary>
    public Transform[] waypoints;
}

/// <summary>
/// HQ에서 각 건물로 이어지는 LineRenderer 경로를 관리하는 컴포넌트.
/// [ExecuteAlways]로 에디터 모드에서도 Update가 실행되어 선이 실시간으로 반영된다.
/// </summary>
[ExecuteAlways]
public class LineManager : MonoBehaviour
{
    /// <summary>본부 건물. 모든 경로의 시작점.</summary>
    [SerializeField] private Spot _hqSpot;
    /// <summary>HQ에서 각 건물로 이어지는 경로 목록.</summary>
    [SerializeField] private RoadPath[] _roads;
    /// <summary>LineRenderer 표시용 z값. 2D에서 선의 렌더링 순서를 조정.</summary>

    [Header("Editor Preview")]
    /// <summary>에디터(Play 전)에서 경로 선을 표시할지 여부. Inspector에서 토글로 켜고 끌 수 있다.</summary>
    [SerializeField] private bool _showPathInEditor = true;

    private void OnEnable()
    {
        if (Application.isPlaying) return;
        RefreshEditorLines();
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        // Play 시작 시 모든 경로 선을 비활성화
        foreach (RoadPath road in _roads)
        {
            if (road.lineRenderer == null) continue;
            road.lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            RefreshEditorLines();
        }
        else
        {
            ApplyLines();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Inspector에서 값이 바뀌면 즉시 반영
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            RefreshEditorLines();
        };
    }
#endif

    /// <summary>
    /// 에디터 모드에서 _showPathInEditor 토글에 따라 선을 갱신.
    /// </summary>
    private void RefreshEditorLines()
    {
        SetLinesEnabled(_showPathInEditor);
        if (_showPathInEditor)
            ApplyLines();
    }

    /// <summary>
    /// 모든 경로의 LineRenderer 활성화 상태를 일괄 설정.
    /// </summary>
    private void SetLinesEnabled(bool enabled)
    {
        if (_roads == null) return;
        foreach (RoadPath road in _roads)
        {
            if (road.lineRenderer == null) continue;
            road.lineRenderer.enabled = enabled;
        }
    }

    /// <summary>
    /// 매 프레임 각 RoadPath의 LineRenderer 위치를 최신 Transform 위치로 갱신.
    /// </summary>
    private void ApplyLines()
    {
        if (_hqSpot == null || _roads == null) return;

        // HQ 위치 (z값은 LineRenderer 표시용으로 고정)
        Vector3 hqPos = _hqSpot.transform.position;

        foreach (RoadPath road in _roads)
        {
            if (road.lineRenderer == null || road.buildingSpot == null) continue;
            if (road.waypoints == null) continue;

            // 총 점 개수: HQ(1) + 중간 waypoints + 목적지(1)
            int count = 2 + road.waypoints.Length;
            road.lineRenderer.useWorldSpace = true;
            road.lineRenderer.positionCount = count;

            // 시작점: HQ
            road.lineRenderer.SetPosition(0, hqPos);

            // 중간 경유 지점
            for (int i = 0; i < road.waypoints.Length; i++)
            {
                if (road.waypoints[i] == null) continue;
                Vector3 wp = road.waypoints[i].position;
                road.lineRenderer.SetPosition(i + 1, wp);
            }

            // 끝점: 목적지 건물
            Vector3 buildingPos = road.buildingSpot.transform.position;
            road.lineRenderer.SetPosition(count - 1, buildingPos);
        }
    }

    /// <summary>
    /// 특정 건물 Spot까지의 이동 경로를 Transform 배열로 반환.
    /// 반환 순서: [HQ, waypoint0, waypoint1, ..., targetSpot]
    /// </summary>
    /// <param name="targetSpot">경로를 찾을 목적지 건물.</param>
    /// <returns>이동 경로 Transform 배열. targetSpot이 등록되지 않은 경우 null.</returns>
    public Transform[] GetWaypoints(Spot targetSpot)
    {
        foreach (RoadPath road in _roads)
        {
            if (road.buildingSpot == targetSpot)
            {
                // 배열 크기: HQ(1) + 중간 waypoints + 목적지(1)
                int count = 1 + road.waypoints.Length + 1;
                Transform[] path = new Transform[count];

                // 시작점: HQ
                path[0] = _hqSpot.transform;

                // 중간 경유 지점
                for (int i = 0; i < road.waypoints.Length; i++)
                    path[i + 1] = road.waypoints[i];

                // 끝점: 목적지 건물
                path[count - 1] = road.buildingSpot.transform;
                return path;
            }
        }

        // 해당 Spot의 경로가 등록되지 않은 경우
        return null;
    }

    /// <summary>
    /// 특정 건물 Spot에서 HQ로 귀환하는 역방향 경로를 Transform 배열로 반환.
    /// 반환 순서: [targetSpot, waypoint(n-1), ..., waypoint0, HQ]
    /// </summary>
    /// <param name="fromSpot">귀환을 시작할 건물.</param>
    /// <returns>역방향 이동 경로 Transform 배열. fromSpot이 등록되지 않은 경우 null.</returns>
    public Transform[] GetReturnWaypoints(Spot fromSpot)
    {
        Transform[] path = GetWaypoints(fromSpot);
        if (path == null) return null;

        System.Array.Reverse(path);
        return path;
    }
}
