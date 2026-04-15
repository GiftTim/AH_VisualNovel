using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 경로 탐색 그래프의 노드 하나를 나타내는 컴포넌트.
/// Neighbors 리스트로 인접 노드를 연결하며, _isBidirectional이 true이면
/// Awake 시점에 자동으로 양방향 연결을 설정한다.
/// </summary>
public class Node : MonoBehaviour
{
    public Transform Transform;              // 이 노드의 Transform (Gizmo 선 그리기에 사용)
    public List<Node> Neighbors = new List<Node>(); // 이 노드에 인접한 노드 목록
    public bool _isBidirectional = false;    // true이면 Awake 시 인접 노드에도 자동으로 역방향 연결 추가

    [Header("디버그")]
    [SerializeField] private bool _debug = false;          // true이면 에디터에서 원형 표시 활성화
    [SerializeField] private TextMeshProUGUI _txtNumber;   // 노드 번호를 표시하는 TMP 텍스트 (디버그용)
    [SerializeField] private GameObject _circle;           // 노드 위치를 표시하는 원형 오브젝트 (디버그용)

    private void Awake()
    {
        // _isBidirectional이 true이면 각 인접 노드에 자신을 역방향으로 추가
        if (_isBidirectional)
        {
            foreach (var node in Neighbors)
            {
                if (!node.Neighbors.Contains(this)) node.Neighbors.Add(this);
            }
        }
    }

    private void OnValidate()
    {
        // _debug가 false이면 원형 오브젝트를 숨기고 종료
        if (!_debug)
        {
            if (_circle != null) _circle.gameObject.SetActive(false); // null 체크 후 비활성화
            return;
        }

        // _debug가 true이면 원형 오브젝트를 표시
        _circle.gameObject.SetActive(true);

        try
        {
            // 게임 오브젝트 이름에서 괄호 안의 숫자를 파싱해 텍스트에 표시
            // 예: "Node(3)" → _txtNumber.text = "3"
            int startIndex = name.IndexOf('(') + 1;
            int endIndex = name.IndexOf(')');

            if (startIndex < 0 || endIndex >= name.Length)
            {
                _txtNumber.text = "0"; // 파싱 실패 시 기본값
                return;
            }

            string id = name.Substring(startIndex, endIndex - startIndex);
            _txtNumber.text = id;
        }
        catch (Exception error)
        {
            // 예외 발생 시 무시 (에디터 환경에서 컴포넌트 초기화 전 호출될 수 있음)
            Debug.Log("Node OnValidate parsing error: " + error.Message);
        }
    }

    /// <summary>
    /// Transform을 지정해 노드를 초기화하는 생성자.
    /// </summary>
    public Node(Transform t)
    {
        Transform = t;
    }

    private void OnDrawGizmos()
    {
        // 에디터에서 각 인접 노드까지 빨간 선으로 연결 표시
        Gizmos.color = Color.red;
        foreach (var node in Neighbors)
        {
            Gizmos.DrawLine(Transform.position, node.transform.position);
        }
    }
}
