using UnityEngine;

/// <summary>
/// 캐릭터 파견/귀환을 총괄하는 컴포넌트.
/// 파견: 캐릭터를 HQ에서 목표 건물로 이동 → 도착 시 비활성화.
/// 귀환: 캐릭터를 목표 건물에서 HQ로 이동 → 도착 시 비활성화.
/// </summary>
public class MissionDispatcher : MonoBehaviour
{
    /// <summary>경로 데이터를 제공하는 컴포넌트.</summary>
    [SerializeField] LineManager _connector;
    /// <summary>캐릭터 시작 위치 기준점 (HQ Spot의 Transform).</summary>
    [SerializeField] Transform _hqTransform;
    /// <summary>Awake에서 초기 위치를 설정할 캐릭터.</summary>
    [SerializeField] CharacterAgent _character;

    void Awake()
    {
        // 캐릭터 시작 위치를 HQ로 설정 후 비활성화
        _character.transform.position = _hqTransform.position;
        _character.gameObject.SetActive(false);
    }

    /// <summary>
    /// 파견: 캐릭터를 HQ에서 목표 건물로 이동 → 도착 시 비활성화.
    /// </summary>
    /// <param name="character">파견할 캐릭터.</param>
    /// <param name="targetSpot">목표 건물.</param>
    public void Dispatch(CharacterAgent character, Spot targetSpot)
    {
        targetSpot.SetState(BuildingState.Selected);
        character.gameObject.SetActive(true);
        Transform[] path = _connector.GetWaypoints(targetSpot);
        if (path != null)
            character.MoveAlong(path, () => character.gameObject.SetActive(false));
    }

    /// <summary>
    /// 귀환: 캐릭터를 목표 건물에서 HQ로 이동 → 도착 시 비활성화.
    /// </summary>
    /// <param name="character">귀환할 캐릭터.</param>
    /// <param name="fromSpot">귀환을 시작할 건물.</param>
    public void Return(CharacterAgent character, Spot fromSpot)
    {
        fromSpot.SetState(BuildingState.Completed);
        character.gameObject.SetActive(true);
        Transform[] path = _connector.GetReturnWaypoints(fromSpot);
        if (path != null)
            character.MoveAlong(path, () => character.gameObject.SetActive(false));
    }
}
