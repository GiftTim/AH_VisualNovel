using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BFS(너비 우선 탐색) 기반의 최단 경로 탐색 매니저.
/// nodesParent 하위의 Node들을 수집하고, 두 노드 사이의 최단 경로를 반환한다.
/// </summary>
public class PathFinderManager : MonoBehaviour
{
    [Header("그래프 설정")]
    public Transform nodesParent; // 모든 노드를 담고 있는 부모 오브젝트
    public Color lineColor = Color.cyan; // Gizmo에서 노드 연결선 색상
    public float lineWidth = 2f;         // (현재 미사용) 연결선 굵기

    // 씬에서 수집된 전체 노드 목록
    private List<Node> nodes = new List<Node>();

    [Header("디버그")]
    [SerializeField] private bool _debug = false;             // true 시 I키 입력으로 경로 테스트 활성화
    [SerializeField] private Node Origin;                     // 경로 탐색 시작 노드
    [SerializeField] private Node Destiny;                    // 경로 탐색 목적지 노드
    [SerializeField] private List<Node> _currentPath = new List<Node>(); // 마지막으로 탐색된 경로
    [SerializeField] private Transform _playerTransform;      // (예약) 플레이어 Transform 참조
    [SerializeField] private float _animationSpeed = 0.5f;   // (예약) 경로 이동 애니메이션 속도

    void Start()
    {
        // nodesParent 하위 자식 오브젝트에서 Node 컴포넌트를 모두 수집
        nodes.Clear();
        if (nodesParent == null) nodesParent = transform; // nodesParent 미지정 시 자기 자신을 사용

        foreach (Transform child in nodesParent)
        {
            Node node = child.GetComponent<Node>();
            if (node != null)
                nodes.Add(node);
        }
    }

    private void Update()
    {
        // I 키를 누르면 Origin → Destiny 경로를 탐색하고 결과를 Console에 출력
        if (Input.GetKeyUp(KeyCode.I))
        {
            var path = GetPath(Origin, Destiny);
            _currentPath = path;
            Debug.Log($"Path {_currentPath.Count}"); // 탐색된 노드 개수 출력
        }
    }

    // ============================================================
    // 두 노드 사이의 최단 경로를 BFS로 탐색
    // origem: 출발 노드 / destino: 목적지 노드
    // 반환값: 출발 → 목적지 순서의 Node 리스트 (경로 없으면 빈 리스트)
    // ============================================================
    public List<Node> GetPath(Node origem, Node destino)
    {
        // 출발 또는 목적지가 없으면 빈 리스트 반환
        if (origem == null || destino == null)
            return new List<Node>();

        // 경로 역추적을 위해 "어디서 왔는지" 기록하는 딕셔너리
        Dictionary<Node, Node> veioDe = new Dictionary<Node, Node>();
        Queue<Node> fila = new Queue<Node>();       // BFS 탐색 큐
        HashSet<Node> visitados = new HashSet<Node>(); // 이미 방문한 노드 집합

        fila.Enqueue(origem);
        visitados.Add(origem);

        bool encontrou = false; // 목적지 도달 여부

        while (fila.Count > 0)
        {
            Node atual = fila.Dequeue(); // 큐에서 현재 노드 꺼내기

            // 목적지에 도달하면 탐색 종료
            if (atual == destino)
            {
                encontrou = true;
                break;
            }

            // 현재 노드의 인접 노드를 큐에 추가
            foreach (var viz in atual.Neighbors)
            {
                if (viz == null || visitados.Contains(viz))
                    continue; // null이거나 이미 방문한 노드는 건너뜀

                visitados.Add(viz);
                veioDe[viz] = atual; // viz에 도달한 경로 기록
                fila.Enqueue(viz);
            }
        }

        // 목적지에 도달하지 못한 경우 빈 리스트 반환
        if (!encontrou)
            return new List<Node>();

        // 목적지에서 출발지까지 역추적하여 경로를 재구성
        List<Node> caminho = new List<Node>();
        Node n = destino;
        while (n != origem)
        {
            caminho.Insert(0, n); // 앞에 삽입해서 순서를 올바르게 유지
            n = veioDe[n];
        }
        caminho.Insert(0, origem); // 출발 노드를 맨 앞에 추가

        return caminho;
    }

    // ============================================================
    // 에디터 Gizmo 시각화
    // Cyan: 전체 노드 연결선 / Red: 현재 탐색된 경로
    // ============================================================
    private void OnDrawGizmos()
    {
        if (nodesParent == null || !_debug) return;

        // 모든 노드의 인접 연결선을 Cyan으로 표시
        Gizmos.color = lineColor;
        foreach (Transform child in nodesParent)
        {
            Node node = child.GetComponent<Node>();
            if (node == null) continue;

            foreach (var viz in node.Neighbors)
            {
                if (viz != null)
                    Gizmos.DrawLine(node.transform.position, viz.transform.position);
            }
        }

        // 현재 탐색된 경로를 Red로 표시
        Gizmos.color = Color.red;
        for (int i = 1; i < _currentPath.Count; i++)
        {
            Gizmos.DrawLine(_currentPath[i - 1].transform.position, _currentPath[i].transform.position);
        }
    }
}
