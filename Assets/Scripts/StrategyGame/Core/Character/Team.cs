using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatManager;

/// <summary>
/// 임무에 함께 파견되는 캐릭터 그룹.
/// 최대 인원(maxSize)이 있으며, 팀 전체의 스탯 합산을 제공한다.
/// </summary>
public class Team
{
    private const string WARNING_MESSAGE_MAX_MEMBER_AMOUNT_REACHED = "Cannot add member: team is at maximum capacity.";

    private List<CharacterUnit> _members; // 팀 구성원 목록
    private int _maxSize;                 // 팀 최대 인원

    /// <summary>최대 인원을 지정해 빈 팀을 생성한다.</summary>
    public Team(int maxSize)
    {
        _maxSize = maxSize;
        _members = new List<CharacterUnit>();
    }

    /// <summary>팀에 멤버를 추가한다. 최대 인원 초과 시 경고만 출력하고 무시한다.</summary>
    public void AddMember(CharacterUnit newMember)
    {
        if (_members.Count < _maxSize)
        {
            _members.Add(newMember);
        }
        else
        {
            Debug.LogWarning(WARNING_MESSAGE_MAX_MEMBER_AMOUNT_REACHED);
        }
    }

    /// <summary>팀에서 멤버를 제거한다.</summary>
    public void RmvMember(CharacterUnit memberToRemove)
    {
        _members.Remove(memberToRemove);
    }

    /// <summary>
    /// 팀 전체 멤버의 스탯을 합산한 StatManager를 반환한다.
    /// 임무 성공 확률 계산 등에 사용한다.
    /// </summary>
    public StatManager GetTeamStats()
    {
        var teamStates = new StatManager(); // 0으로 초기화된 빈 StatManager

        foreach (var member in _members)
        {
            teamStates.ExpendStats(member.StatManager); // 각 멤버 스탯을 더함
        }

        return teamStates;
    }

    /// <summary>특정 캐릭터가 이 팀에 속해 있는지 확인한다.</summary>
    public bool HasMember(CharacterUnit characterUnit) => _members.Contains(characterUnit);

    /// <summary>팀 전체의 특정 스탯 합산값을 반환한다.</summary>
    public Stat GetStat(StatType statType) => GetTeamStats().GetStat(statType);

    public int Size => _members.Count;                         // 현재 팀원 수
    public int MaxSize { get { return _maxSize; } }            // 최대 팀원 수
    public List<CharacterUnit> Members { get { return _members; } } // 팀원 목록
}
