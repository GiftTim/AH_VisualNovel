using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하루 동안의 임무 결과를 집계하는 데이터 클래스.
/// DayManager가 소유하며 하루 시작 시 초기화, 임무 이벤트마다 수치가 누적된다.
/// 하루 종료 시 OnDayEnd 이벤트로 전달되어 길드 경험치 계산 등에 사용된다.
/// </summary>
public class DayReport
{
    public int MissionsAccepted { get; private set; }       // 수락한 임무 수
    public int MissionMisses { get; private set; }          // 시간 초과로 놓친 임무 수
    public int MissionSucceded { get; private set; }        // 성공한 임무 수
    public int MissionFailed { get; private set; }          // 실패한 임무 수
    public int TotalGoldGained { get; private set; }        // 이날 획득한 총 골드
    public int TotalReputationGained { get; private set; }  // 이날 획득한 총 평판
    public int TotalCalls { get; private set; }             // 이날 등장한 총 임무 수 (성공+실패+놓침)

    // 캐릭터별 수락 임무 횟수 (UI 통계 등에 활용 가능)
    private Dictionary<CharacterUnit, int> MissionAcceptedPerCharacters;

    public int LevelGained { get; private set; }            // 이날로 인해 올라간 레벨 수 (현재 미사용)
    public float LevelPercLostByFail { get; private set; }  // 실패로 잃은 경험치 비율 (현재 미사용)
    public float LevelPercLostByMissed { get; private set; }// 미스로 잃은 경험치 비율 (현재 미사용)

    /// <summary>하루 시작 시 모든 카운터를 0으로 초기화한다.</summary>
    public DayReport()
    {
        MissionsAccepted = 0;
        MissionMisses = 0;
        MissionSucceded = 0;
        MissionFailed = 0;
        TotalGoldGained = 0;
        TotalReputationGained = 0;
        TotalCalls = MissionSucceded + MissionFailed + MissionMisses; // 초기값 0
        MissionAcceptedPerCharacters = new Dictionary<CharacterUnit, int>();
    }

    /// <summary>임무를 수락했을 때 호출. 캐릭터별 수락 횟수도 누적한다.</summary>
    public void HandleMissionAccepted(List<CharacterUnit> characters)
    {
        MissionsAccepted++;

        foreach (var character in characters)
        {
            if (!MissionAcceptedPerCharacters.ContainsKey(character))
            {
                MissionAcceptedPerCharacters[character] = 0;
            }

            MissionAcceptedPerCharacters[character] += 1;
        }
    }

    /// <summary>임무를 시간 내에 수락하지 못해 놓쳤을 때 호출.</summary>
    public void HandleMissionMiss()
    {
        MissionMisses++;
    }

    /// <summary>임무에 성공했을 때 호출. 골드도 함께 누적한다.</summary>
    public void HandleMissionSucceded(int gold)
    {
        MissionSucceded++;
        TotalGoldGained += gold;
    }

    /// <summary>임무에 실패했을 때 호출.</summary>
    public void HandleMissionFailed()
    {
        MissionFailed++;
    }

    /// <summary>이날 등장한 임무 수를 추가한다. DayMissionManager가 새 임무를 생성할 때 호출한다.</summary>
    public void AddCalls(int callAmount)
    {
        TotalCalls += callAmount;
    }
}
