using UnityEngine;

namespace TESTING
{
    /// <summary>
    /// 건물 상태 변경 및 캐릭터 이동을 테스트하는 컴포넌트.
    /// Mission 시스템 도입 전 임시 테스트용. 추후 MissionManager로 이전 예정.
    /// </summary>
    public class TestingSpotState : MonoBehaviour
    {
        /// <summary>상태를 변경할 목표 건물.</summary>
        [SerializeField] Spot _targetSpot;
        /// <summary>HQ → 목표 건물 경로를 제공하는 컴포넌트.</summary>
        [SerializeField] BuildingLineConnector _connector;
        /// <summary>경로를 따라 이동할 캐릭터.</summary>
        [SerializeField] CharacterAgent _character;

        void Update()
        {
            // A 키: 목표 건물을 Selected 상태로 변경 + 캐릭터 이동 시작
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (_targetSpot != null)
                {
                    // 건물 상태를 Selected(미션 진행 중)으로 변경
                    _targetSpot.SetState(BuildingState.Selected);

                    // HQ → 목표 건물까지의 경로 취득
                    Transform[] path = _connector.GetWaypoints(_targetSpot);

                    // 경로가 존재하면 캐릭터 이동 시작
                    if (path != null)
                        _character.MoveAlong(path);
                }
            }

            // B 키: 목표 건물을 Completed 상태로 변경 + 캐릭터 귀환 이동 시작
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (_targetSpot != null)
                {
                    _targetSpot.SetState(BuildingState.Completed);

                    Transform[] path = _connector.GetReturnWaypoints(_targetSpot);
                    if (path != null)
                        _character.MoveAlong(path);
                }
            }
        }
    }
}
