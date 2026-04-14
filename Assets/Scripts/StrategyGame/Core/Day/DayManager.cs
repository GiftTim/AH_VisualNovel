using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ★ Phase C: DayMissionManager 관련 코드 주석 처리됨
// ★ Phase 8 (A조합) 추가 시 주석 해제 후 복원할 것
/// <summary>
/// 하루 사이클 전체를 제어하는 MonoBehaviour.
/// 흐름: StartDay() 호출 → DayLoopCoroutine 실행 → 시간 경과 → OnDayEnd 이벤트 발행
///
/// Phase C (현재): DayMissionManager 없이 DayDurationInSeconds 만큼 시간이 지나면 하루 종료.
/// Phase 8 (A조합): DayMissionManager 복원 후 임무 모두 소진 시 하루 종료로 변경.
///
/// 씬에 [DayManager] 오브젝트를 만들고 이 컴포넌트를 부착한다.
/// Inspector에서 _dayCharacterManager와 _defaultDaySO를 연결해야 한다.
/// </summary>
public class DayManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCharacterManager _dayCharacterManager; // 캐릭터 상태 관리 담당
    // [SerializeField] private DayMissionManager _missionManager;  // Phase 8: A조합에서 복원

    [Header("Day Events")]
    /// <summary>하루가 시작될 때 발행. 파라미터: 이날 활동할 캐릭터 목록.</summary>
    public UnityEvent<List<CharacterUnit>> OnDayStart;
    /// <summary>하루가 완전히 끝났을 때 발행. 파라미터: 이날 임무 결과 리포트.</summary>
    public UnityEvent<DayReport> OnDayEnd;
    /// <summary>매 프레임 경과 시간을 전달하는 이벤트. UI 타이머 등에 사용.</summary>
    public UnityEvent<float> OnTimeUpdated;
    // public UnityEvent<List<MissionUnit>> OnMissionAvailable;  // Phase 8: A조합에서 복원

    [Header("Day Parameters")]
    /// <summary>날짜별 설정 목록. 인덱스 0 = 1일차. 목록 범위를 초과하면 _defaultDaySO를 사용한다.</summary>
    [SerializeField] private List<DaySO> _daySOs;
    /// <summary>_daySOs 목록에 없는 날짜에 사용할 기본 DaySO.</summary>
    [SerializeField] private DaySO _defaultDaySO;

    [Header("Pause")]
    /// <summary>일시정지 상태가 변경될 때 발행. bool: true=일시정지, false=재개.</summary>
    public UnityEvent<bool> OnGamePausedChange;

    [Header("Debug")]
    [SerializeField] private bool _runOnStart = false; // true이면 씬 시작 시 자동으로 하루 시작 (디버그용)

    private float _elapseTime;   // 현재 하루에서 경과한 시간(초)
    private bool _isPaused = false; // 일시정지 여부
    private DayReport _dayReport;   // 이날의 임무 결과 집계

    private GameState _gameState;   // 현재 게임 상태 (GameManager에서 참조)

    private void Start()
    {
        // _missionManager.OnMissionMiss.AddListener(HandleMissionMiss);  // Phase 8: A조합에서 복원
    }

    /// <summary>
    /// 하루를 시작한다. 외부(UI 버튼 등)에서 호출한다.
    /// </summary>
    /// <param name="day">시작할 날짜 번호 (1부터 시작)</param>
    /// <param name="characters">이날 참여할 캐릭터 목록</param>
    public void StartDay(int day, List<CharacterUnit> characters)
    {
        characters.ForEach(c => c.SetStatusToAvailable()); // 전원 대기 상태로 초기화
        _gameState = GameManager.Instance.GameState;        // 현재 게임 상태 참조

        _isPaused = false;
        _dayReport = new DayReport(); // 리포트 초기화

        // 해당 날짜의 DaySO 선택 (없으면 기본값 사용)
        DaySO daySO = (day - 1 < _daySOs.Count) ? _daySOs[day - 1] : _defaultDaySO;
        // var missionAmount = daySO.UseAllMissions ? daySO.MissionSOs.Count : daySO.MissionAmount;  // Phase 8: A조합에서 복원

        _dayCharacterManager.Init(characters);

        // _missionManager.Init(daySO.MissionSOs, missionAmount, daySO.DayDurationInSeconds, HandleNewMissions);  // Phase 8: A조합에서 복원

        StartCoroutine(DayLoopCoroutine(daySO.DayDurationInSeconds));
    }

    /// <summary>
    /// [Phase C] 시간 기반 하루 루프 코루틴.
    /// DayDurationInSeconds만큼 시간이 경과하면 루프를 종료한다.
    /// Phase 8에서는 _missionManager.HasMissions() 기반 루프로 교체 예정.
    /// </summary>
    /// <param name="dayDuration">하루 지속 시간(초)</param>
    private IEnumerator DayLoopCoroutine(float dayDuration)
    {
        OnDayStart?.Invoke(_dayCharacterManager.Characters); // 하루 시작 이벤트

        _elapseTime = 0f;
        var accumTimeToUpdate = 0f; // 0.5초 누적 카운터 (매 프레임 처리 방지)

        // 경과 시간이 하루 길이에 달할 때까지 루프
        while (_elapseTime < dayDuration)
        {
            yield return new WaitForEndOfFrame(); // 한 프레임 대기

            if (!_isPaused)
            {
                _elapseTime += Time.deltaTime;        // 실제 경과 시간 누적
                accumTimeToUpdate += Time.deltaTime;

                // 0.5초마다 캐릭터 상태 업데이트 (휴식 타이머 체크 등)
                if (accumTimeToUpdate > 0.5f)
                {
                    _dayCharacterManager.UpdateCharacters(_elapseTime);
                    // _missionManager.UpdateMissions(_elapseTime);  // Phase 8: A조합에서 복원
                    accumTimeToUpdate = 0f;
                }

                OnTimeUpdated?.Invoke(_elapseTime); // UI 타이머 갱신
            }
        }

        // 하루 시간이 끝나도 임무 중인 캐릭터가 복귀할 때까지 대기
        yield return new WaitUntil(() => _dayCharacterManager.AllCharacterInBase());

        OnDayEnd?.Invoke(_dayReport); // 하루 종료 이벤트 (길드 경험치 처리 등)
    }

    // ─── Phase 8: A조합 복원 시 아래 메서드들 주석 해제 ──────────────
    // public void ClaimMission(MissionUnit missionUnit, bool isSuccess) { ... }
    // public void AcceptMission(MissionUnit mission, Team team) { ... }
    // public void StartMission(MissionUnit mission) { ... }
    // private void HandleMissionMiss(MissionUnit missionUnit) { _dayReport.HandleMissionMiss(); }
    // private void HandleNewMissions(List<MissionUnit> newMissions) { ... }

    /// <summary>게임을 일시정지한다. OnGamePausedChange(true) 이벤트를 발행한다.</summary>
    public void PauseDay()
    {
        _isPaused = true;
        OnGamePausedChange?.Invoke(_isPaused);
    }

    /// <summary>일시정지를 해제한다. OnGamePausedChange(false) 이벤트를 발행한다.</summary>
    public void ResumeDay()
    {
        _isPaused = false;
        OnGamePausedChange?.Invoke(_isPaused);
    }

    /// <summary>
    /// 캐릭터 팀이 기지에 도착했을 때 호출.
    /// 팀원 전체를 휴식 상태로 전환하고 _startTime을 기록한다.
    /// (Phase C에서는 직접 호출할 주체가 없어 미사용. Phase 8에서 활성화.)
    /// </summary>
    public void HandleCharacterArriveBase(Team team, float currentTime)
    {
        team.Members.ForEach(m => m.SetCharacterResting(currentTime));
    }

    /// <summary>현재까지 경과한 시간(초). UI 타이머나 임무 시간 계산에 사용한다.</summary>
    public float CurrentTime => _elapseTime;
}
