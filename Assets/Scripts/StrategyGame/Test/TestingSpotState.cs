using UnityEngine;

namespace TESTING
{
    /// <summary>
    /// MissionDispatcher를 통해 파견/귀환을 테스트하는 컴포넌트.
    /// Mission 시스템 도입 전 임시 테스트용. 추후 UI 버튼으로 교체 예정.
    /// </summary>
    public class TestingSpotState : MonoBehaviour
    {
        /// <summary>상태를 변경할 목표 건물.</summary>
        [SerializeField] Spot _targetSpot;
        /// <summary>파견/귀환 로직을 위임할 컴포넌트.</summary>
        [SerializeField] MissionDispatcher _dispatcher;
        /// <summary>경로를 따라 이동할 캐릭터.</summary>
        [SerializeField] CharacterAgent _character;

        void Update()
        {
            // A 키: Idle 상태로 변경
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Setting target spot to Idle");

                if (_targetSpot != null)
                    _targetSpot.SetState(BuildingState.Idle);
            }

            // B 키: 목표 건물 상태를 Incident로 변경
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("Setting target spot to Incident");

                if (_targetSpot != null)
                    _targetSpot.SetState(BuildingState.Incident);
            }

            // C 키: 목표 건물 상태를 Selected로 변경하고 캐릭터를 파견
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Setting target spot to Selected");

                if (_targetSpot != null)
                {
                    _targetSpot.SetState(BuildingState.Selected);
                    _dispatcher.Dispatch(_character, _targetSpot);
                }
            }

            // D 키: 목표 건물 상태를 Completed로 변경하고 캐릭터를 HQ로 귀환
            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("Setting target spot to Completed");

                if (_targetSpot != null)
                {
                    _targetSpot.SetState(BuildingState.Completed);
                }
            }


        }
    }
}
