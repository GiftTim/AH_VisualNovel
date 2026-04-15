using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 하루 동안 임무 생성·배분·추적을 담당하는 MonoBehaviour.
/// DayManager의 지시를 받으며 MissionUnit 인스턴스를 생성하고 관리한다.
///
/// 흐름:
///   Init() → 임무 목록 구성 + 등장 시간 배분
///   UpdateMissions() → 매 0.5초마다 호출, 등장 시간 도달 시 임무를 활성화
///   HasMissions() → 처리되지 않은 임무가 남아있으면 true (DayLoopCoroutine 종료 조건)
///   OnMissionMiss → MissionUnit이 Lost 상태가 됐을 때 발행
///
/// 씬에 [DayMissionManager] 오브젝트를 만들고 이 컴포넌트를 부착한다.
/// DayManager Inspector의 _missionManager 필드에 연결해야 한다.
/// </summary>
public class DayMissionManager : MonoBehaviour
{
    /// <summary>임무가 시간 초과로 소멸될 때 발행. DayManager.HandleMissionMiss()가 구독한다.</summary>
    public UnityEvent<MissionUnit> OnMissionMiss = new UnityEvent<MissionUnit>();

    /// <summary>Init()에서 생성된 전체 임무와 등장 예정 시간 목록.</summary>
    private List<(MissionUnit mission, float spawnTime)> _pendingMissions;

    /// <summary>이미 등장해 처리 대기 중이거나 수행 중인 임무 목록.</summary>
    private List<MissionUnit> _activeMissions;

    /// <summary>새 임무가 등장할 때 DayManager에게 알리는 콜백.</summary>
    private Action<List<MissionUnit>> _newMissionsCallback;

    /// <summary>
    /// 하루 시작 시 DayManager가 호출한다.
    /// missionSOs에서 missionAmount 개를 선택해 등장 시간(spawnTime)을
    /// dayDuration 범위 내에 고르게 배분한다.
    /// </summary>
    /// <param name="missionSOs">이날 등장 가능한 임무 SO 목록</param>
    /// <param name="missionAmount">이날 실제로 등장할 임무 수</param>
    /// <param name="dayDuration">하루 지속 시간(초). 임무 등장 시간 배분 범위로 사용</param>
    /// <param name="newMissionsCallback">새 임무 등장 시 DayManager에 알리는 콜백</param>
    public void Init(List<MissionSO> missionSOs, int missionAmount,
                     float dayDuration,
                     Action<List<MissionUnit>> newMissionsCallback)
    {
        _pendingMissions = new List<(MissionUnit, float)>();
        _activeMissions = new List<MissionUnit>();
        _newMissionsCallback = newMissionsCallback;

        // missionSOs에서 missionAmount 개를 셔플 후 선택
        var selectedSOs = new List<MissionSO>(missionSOs);
        for (int i = selectedSOs.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            MissionSO temp = selectedSOs[i];
            selectedSOs[i] = selectedSOs[j];
            selectedSOs[j] = temp;
        }
        if (missionAmount < selectedSOs.Count)
            selectedSOs = selectedSOs.GetRange(0, missionAmount);

        // 등장 시간을 dayDuration 범위 내에 고르게 배분
        // 예: 임무 3개, dayDuration=120 → 30초, 60초, 90초에 등장
        float interval = dayDuration / (selectedSOs.Count + 1);
        for (int i = 0; i < selectedSOs.Count; i++)
        {
            float spawnTime = interval * (i + 1);
            // startTime = spawnTime: 임무가 등장한 순간부터 수락 타이머가 시작됨
            var mission = new MissionUnit(i, selectedSOs[i], null, spawnTime);
            mission.OnMissioMiss.AddListener(HandleMissionLost);
            _pendingMissions.Add((mission, spawnTime));
        }
    }

    /// <summary>
    /// DayLoopCoroutine에서 0.5초마다 호출한다.
    /// spawnTime에 도달한 임무를 활성화하고 _newMissionsCallback으로 DayManager에 알린다.
    /// 활성화된 임무의 UpdateMission()을 호출해 상태를 갱신한다.
    /// </summary>
    /// <param name="elapsedTime">하루 시작부터 경과한 시간(초)</param>
    public void UpdateMissions(float elapsedTime)
    {
        // 등장 시간에 도달한 임무를 찾아 활성화
        var toActivate = new List<(MissionUnit mission, float spawnTime)>();
        foreach (var entry in _pendingMissions)
        {
            if (elapsedTime >= entry.spawnTime)
                toActivate.Add(entry);
        }

        if (toActivate.Count > 0)
        {
            var newMissions = new List<MissionUnit>();
            foreach (var entry in toActivate)
            {
                _pendingMissions.Remove(entry);
                _activeMissions.Add(entry.mission);
                newMissions.Add(entry.mission);
                Debug.Log($"[DayMissionManager] 임무 등장: {entry.mission.MissionSO.Name} (spawnTime={entry.spawnTime:F1}s)");
            }
            _newMissionsCallback?.Invoke(newMissions);
        }

        // 활성화된 임무 상태 갱신 (수락 타이머, 완료 타이머 등)
        foreach (var mission in _activeMissions)
        {
            mission.UpdateMission(elapsedTime);
        }
    }

    /// <summary>
    /// 아직 처리되지 않은 임무(Claimed/Lost가 아닌 상태)가 하나라도 남아있으면 true.
    /// DayLoopCoroutine의 루프 종료 조건에 사용된다.
    /// </summary>
    public bool HasMissions()
    {
        // 아직 등장하지 않은 임무가 있으면 계속
        if (_pendingMissions.Count > 0) return true;

        // 활성화된 임무 중 미처리된 임무가 있으면 계속
        foreach (var mission in _activeMissions)
        {
            if (!mission.IsMissionClaimed() && !mission.IsMissionLost())
                return true;
        }

        return false;
    }

    /// <summary>MissionUnit의 OnMissioMiss 이벤트를 받아 OnMissionMiss를 발행한다.</summary>
    private void HandleMissionLost(MissionUnit missionUnit)
    {
        Debug.Log($"[DayMissionManager] 임무 소멸(Lost): {missionUnit.MissionSO.Name}");
        OnMissionMiss?.Invoke(missionUnit);
    }
}
