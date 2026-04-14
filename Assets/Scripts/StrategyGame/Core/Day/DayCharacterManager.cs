using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 하루 동안 캐릭터들의 상태를 관리하는 MonoBehaviour.
/// DayManager의 지시를 받아 캐릭터 상태를 변경하고 매 틱마다 업데이트한다.
/// 씬에 [DayCharManager] 오브젝트를 만들고 이 컴포넌트를 부착한다.
/// </summary>
public class DayCharacterManager : MonoBehaviour
{
    /// <summary>캐릭터 상태가 바뀔 때마다 발행되는 이벤트. UI 갱신 등에 활용한다.</summary>
    [Header("Events")]
    public UnityEvent<CharacterUnit> OnCharacterChangeStatus;

    // 이날 활동 중인 캐릭터 목록 (DayManager.StartDay()에서 Init으로 전달받는다)
    private List<CharacterUnit> _availableCharacters;

    /// <summary>하루 시작 시 DayManager가 호출해 관리할 캐릭터 목록을 설정한다.</summary>
    public void Init(List<CharacterUnit> characters)
    {
        _availableCharacters = characters;
    }

    /// <summary>
    /// 팀이 임무를 수락했을 때 호출.
    /// 팀원 전체를 "임무지로 이동 중(Going)" 상태로 전환한다.
    /// </summary>
    public void HandleTeamAcceptMission(Team team)
    {
        foreach (CharacterUnit character in team.Members)
        {
            character.SetStatusToInGoingToMission();

            OnCharacterChangeStatus?.Invoke(character); // UI에 상태 변화 알림
        }
    }

    /// <summary>
    /// 임무가 완료됐을 때 호출.
    /// 성공이면 임무 경험치를, 실패면 0을 각 캐릭터에 지급하고 귀환 상태로 전환한다.
    /// </summary>
    public void HandleTeamCompleteMission(MissionUnit missionUnit, Team team, bool isSuccess, float currentTime)
    {
        team.Members.ForEach(c => c.HandleMissionCompleted(isSuccess ? missionUnit.Exp : 0, currentTime));
    }

    /// <summary>
    /// 팀이 임무를 실제로 시작했을 때 호출.
    /// 팀원 전체를 "임무 수행 중(InMission)" 상태로 전환한다.
    /// </summary>
    public void HandleTeamStartMission(Team team)
    {
        team.Members.ForEach(c => c.SetStatusToInMission());
    }

    /// <summary>
    /// 매 0.5초마다 DayManager(DayLoopCoroutine)에서 호출.
    /// 각 캐릭터의 UpdateCharacter()를 호출해 휴식 타이머 등을 처리한다.
    /// </summary>
    public void UpdateCharacters(float elapseTime)
    {
        _availableCharacters.ForEach(c => c.UpdateCharacter(elapseTime));
    }

    /// <summary>
    /// 모든 캐릭터가 기지(기본 위치)에 돌아왔는지 확인한다.
    /// DayLoopCoroutine의 WaitUntil 조건으로 사용되어, 하루가 완전히 끝나는 시점을 결정한다.
    /// Available(대기) 또는 Resting(휴식) 상태면 기지에 있는 것으로 판단한다.
    /// </summary>
    public bool AllCharacterInBase()
    {
        foreach (var character in _availableCharacters)
        {
            // Going/InMission/Returning 상태면 아직 임무 중 → false
            if (!character.IsAvailable() && !character.IsResting()) return false;
        }

        return true; // 전원 기지 복귀 완료
    }

    /// <summary>현재 관리 중인 캐릭터 목록.</summary>
    public List<CharacterUnit> Characters => _availableCharacters;
}
