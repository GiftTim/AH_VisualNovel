using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MissionSO;

/// <summary>
/// 임무 하나의 런타임 인스턴스.
/// MissionSO(설정 데이터)를 기반으로 실제 진행 상태를 관리한다.
/// DayMissionManager가 생성하고 상태를 업데이트한다.
///
/// Phase C에서는 DayCharacterManager가 MissionUnit을 파라미터로 받으므로
/// 컴파일을 위해 포함됨. Phase 8 (A조합) 에서 실제로 활성화된다.
/// </summary>
[Serializable]
public class MissionUnit
{
    /// <summary>
    /// 임무의 진행 상태.
    /// WaitingToBeAccepted → 수락 대기 중 (TimeToAccept 초과 시 Lost)
    /// Accepted            → 수락됨 (팀 이동 중)
    /// InProgress          → 팀이 임무 수행 중
    /// HasEvent            → 랜덤 이벤트 발생 (선택 대기)
    /// InProgressWithEvent → 이벤트 선택 후 계속 수행 중
    /// Completed           → 임무 완료 (수령 대기)
    /// CompletedTheEvent   → 이벤트 포함 임무 완료
    /// Claimed             → 보상 수령 완료
    /// Lost                → 시간 초과로 소멸
    /// </summary>
    public enum MissionStatus
    {
        WaitingToBeAccepted,
        Accepted,
        InProgress,
        HasEvent,
        InProgressWithEvent,
        Completed,
        CompletedTheEvent,
        Claimed,
        Lost
    }

    [SerializeField] private int _id;                    // 이 임무 인스턴스의 고유 번호
    [SerializeField] private MissionSO _missionSO;       // 임무 설정 데이터
    [SerializeField] private MissionStatus _missionStatus; // 현재 진행 상태
    [SerializeField] private float _startTime;           // 현재 단계 시작 시각
    [SerializeField] private Team _currentTeam;          // 임무를 수행 중인 팀

    // 각 상태 변화 이벤트 (UI 갱신 등에 활용)
    public UnityEvent<MissionUnit> OnMissionAccepted = new UnityEvent<MissionUnit>();
    public UnityEvent<MissionUnit> OnMissionStarted = new UnityEvent<MissionUnit>();
    public UnityEvent<MissionUnit> OnMissionCompleted = new UnityEvent<MissionUnit>();
    public UnityEvent<MissionUnit> OnMissioMiss = new UnityEvent<MissionUnit>();
    public UnityEvent<MissionUnit> OnMissionClaimed = new UnityEvent<MissionUnit>();
    public UnityEvent<MissionUnit, RandomMissionEvent> OnMissionHasEvent = new UnityEvent<MissionUnit, RandomMissionEvent>();
    public UnityEvent<MissionUnit> OnChoiceMaded = new UnityEvent<MissionUnit>();

    private RandomMissionEvent _randomMissionEvent; // 현재 발생한 랜덤 이벤트
    private MissionChoice _choiceMade;              // 플레이어가 선택한 이벤트 선택지

    /// <summary>임무 인스턴스를 생성한다. DayMissionManager에서 호출한다.</summary>
    /// <param name="id">인스턴스 고유 번호</param>
    /// <param name="missionSO">임무 설정 SO</param>
    /// <param name="location">임무 위치 Transform (맵 표시용)</param>
    /// <param name="startTime">임무가 등장한 시각 (수락 타이머 기준)</param>
    public MissionUnit(int id, MissionSO missionSO, Transform location, float startTime)
    {
        _id = id;
        _missionSO = missionSO;

        _missionStatus = MissionStatus.WaitingToBeAccepted;
        _startTime = startTime;

        Location = location;
    }

    /// <summary>팀이 임무 시작 지점에 도착해 임무를 실제로 시작한다.</summary>
    public void StartMission(float startTime)
    {
        _missionStatus = MissionStatus.InProgress;
        _startTime = startTime; // 완료 타이머 기산 시점 재설정

        OnMissionStarted?.Invoke(this);
    }

    /// <summary>매 틱 호출. 현재 상태에 따라 타이머를 체크하고 상태를 전환한다.</summary>
    public void UpdateMission(float currentTime)
    {
        if (_missionStatus == MissionStatus.WaitingToBeAccepted)
        {
            // 수락 시간 초과 → 임무 소멸
            if (currentTime - _startTime >= _missionSO.TimeToAccept)
            {
                _missionStatus = MissionStatus.Lost;
                OnMissioMiss?.Invoke(this);
            }
        }
        else if (_missionStatus == MissionStatus.InProgress)
        {
            if (_missionSO.RandomMissionEvents.Count != 0)
            {
                // 진행률 50% 도달 시 랜덤 이벤트 발생
                if ((currentTime - _startTime) / _missionSO.TimeToComplete >= 0.5f)
                {
                    _missionStatus = MissionStatus.HasEvent;
                    _startTime = currentTime; // 이벤트 응답 타이머 기산 시점 재설정

                    _randomMissionEvent = _missionSO.RandomMissionEvents[0];
                    OnMissionHasEvent?.Invoke(this, _randomMissionEvent);
                }
            }
            else if (currentTime - _startTime >= _missionSO.TimeToComplete)
            {
                // 이벤트 없는 임무: 시간 완료 → 완료 상태
                _missionStatus = MissionStatus.Completed;
                OnMissionCompleted?.Invoke(this);
            }
        }
        else if (_missionStatus == MissionStatus.InProgressWithEvent)
        {
            // 이벤트 처리 후 계속 진행 중 → 완료 체크
            if (currentTime - _startTime >= _missionSO.TimeToComplete)
            {
                _missionStatus = MissionStatus.CompletedTheEvent;
                OnMissionCompleted?.Invoke(this);
            }
        }
    }

    /// <summary>현재 단계 완료까지의 진행률(0~1)을 반환한다. UI 프로그레스 바에 사용.</summary>
    public float GetTimeLeftToMakeAction(float currentTime)
    {
        var timeDiff = currentTime - _startTime;

        switch (_missionStatus)
        {
            case MissionStatus.WaitingToBeAccepted: return timeDiff / _missionSO.TimeToAccept;
            case MissionStatus.HasEvent: return timeDiff / _missionSO.TimeToAnswerEvent;
            case MissionStatus.InProgress:
            case MissionStatus.InProgressWithEvent: return timeDiff / _missionSO.TimeToComplete;
            default : return 0;
        }
    }

    /// <summary>
    /// 랜덤 이벤트에서 선택지를 선택한다.
    /// _startTime을 조정해 이벤트 이전 진행분을 반영한 완료 타이머를 재계산한다.
    /// </summary>
    public void MakeChoice(float currentTime, MissionChoice choice)
    {
        _choiceMade = choice;

        // 이미 50% 진행됐으므로 _startTime을 50% 전으로 당겨 남은 시간 계산을 이어받음
        _startTime = currentTime - (_missionSO.TimeToComplete * 0.5f);

        _missionStatus = MissionStatus.InProgressWithEvent;

        OnChoiceMaded?.Invoke(this);
    }

    /// <summary>보상 수령 처리. Claimed 상태로 전환한다.</summary>
    public void ClaimMission()
    {
        _missionStatus = MissionStatus.Claimed;
        OnMissionClaimed?.Invoke(this);
    }

    /// <summary>플레이어가 임무를 수락한다. 팀을 배정하고 Accepted 상태로 전환한다.</summary>
    public void AcceptMission(Team team)
    {
        _currentTeam = team;
        _missionStatus = MissionStatus.Accepted;
        OnMissionAccepted?.Invoke(this);
    }

    /// <summary>임무 요구 스탯(StatManager)을 반환한다. 팀 스탯과 비교해 성공률 계산에 사용한다.</summary>
    public StatManager GetRequiredStats()
    {
        return _missionSO.RequiredStats;
    }

    // ─── 상태 확인 헬퍼 ───────────────────────────────────────────────
    public bool IsMissionWaitingToBeAccepted() => _missionStatus == MissionStatus.WaitingToBeAccepted;
    public bool IsMissionInProgress() => _missionStatus == MissionStatus.InProgress;
    public bool IsMissionClaimed() => _missionStatus == MissionStatus.Claimed;
    public bool IsAccepted() => _missionStatus == MissionStatus.Accepted;
    public bool IsMissionLost() => _missionStatus == MissionStatus.Lost;
    public bool IsMissionCompleted() => _missionStatus == MissionStatus.Completed;
    public bool HasRandomEvent() => _missionStatus == MissionStatus.HasEvent;
    public bool IsMissionCompletedTheEvent() => _missionStatus == MissionStatus.CompletedTheEvent;

    // ─── 읽기 전용 프로퍼티 ───────────────────────────────────────────
    public string Name => _missionSO.Name;
    public string Description => _missionSO.Description;
    public int Exp => _missionSO.RewardExperience;          // 캐릭터 경험치 보상
    public int Gold => _missionSO.RewardExperience;         // 골드 보상 (원본 버그: RewardGold가 아닌 RewardExperience 참조)
    public int Reputation => _missionSO.RewardReputation;
    public int MaxTeamSize => _missionSO.MaxTeamSize;
    public int ID => _id;
    public RandomMissionEvent MissionEvent => _randomMissionEvent;
    public MissionChoice MissionChoice => _choiceMade;
    public Team Team => _currentTeam;
    public MissionSO MissionSO => _missionSO;
    public Transform Location { get; private set; }         // 맵 상의 임무 위치
}
