using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 캐릭터 한 명의 런타임 데이터와 상태를 관리하는 클래스.
/// MonoBehaviour가 아니므로 씬 오브젝트가 아닌 순수 C# 객체로 생성된다.
/// (new CharacterUnit(so) 처럼 직접 생성)
/// </summary>
public class CharacterUnit
{
    /// <summary>
    /// 캐릭터의 현재 행동 상태.
    /// Available → 대기 중 (임무 수락 가능)
    /// Going     → 임무지로 이동 중
    /// InMission → 임무 수행 중
    /// Returning → 임무 완료 후 귀환 중
    /// Resting   → 휴식 중 (TimeToRest 이후 Available로 복귀)
    /// </summary>
    public enum CharacterStatus { Available, Going, InMission, Returning, Resting }

    private CharacterSO _baseCharacter;      // 이 캐릭터의 기본 설정값 (ScriptableObject)
    private StatManager _statManager;        // 현재 스탯 상태 (성장으로 변동 가능)
    private CharacterStatus _status;         // 현재 상태
    private int _currentXP;                 // 현재 누적 경험치
    private int _expToLevelUp;              // 다음 레벨까지 필요한 경험치
    private int _currentLevel;             // 현재 레벨
    private int _availablePoints;          // 미투자 스탯 포인트 수
    private bool _isScheduled;             // 파견(임무 배정) 여부

    // 외부에서 구독할 수 있는 이벤트들 (UI 갱신 등에 사용)
    public UnityEvent<CharacterUnit> OnCharacterInAvailable = new UnityEvent<CharacterUnit>();    // 대기 상태 진입 시
    public UnityEvent<CharacterUnit> OnCharacterInMission = new UnityEvent<CharacterUnit>();     // 임무 중 상태 진입 시
    public UnityEvent<CharacterUnit> OnCharacterInResting = new UnityEvent<CharacterUnit>();     // 휴식 상태 진입 시
    public UnityEvent<CharacterUnit> OnCharacterLevelUP = new UnityEvent<CharacterUnit>();       // 레벨업 시
    public UnityEvent<CharacterUnit> OnCharacterEXPChanged = new UnityEvent<CharacterUnit>();    // 경험치 변화 시
    public UnityEvent<CharacterUnit> OnCharacterStatChanged = new UnityEvent<CharacterUnit>();   // 스탯 변화 시
    public UnityEvent<CharacterUnit> OnCharacterGoingToMission = new UnityEvent<CharacterUnit>(); // 이동 상태 진입 시
    public UnityEvent<CharacterUnit> OnCharacterReturning = new UnityEvent<CharacterUnit>();     // 귀환 상태 진입 시

    // 휴식/이동 시간 계산에 사용하는 시작 시점 기록
    private float _startTime;

    /// <summary>ScriptableObject 설정값으로 새 캐릭터를 생성한다 (신규 게임용).</summary>
    public CharacterUnit(CharacterSO baseCharacter)
    {
        _baseCharacter = baseCharacter;

        _status = CharacterStatus.Available;
        _statManager = new StatManager(baseCharacter.BaseStats); // SO의 기본 스탯을 복사

        _currentLevel = 1;
        _currentXP = _availablePoints = 0;

        _expToLevelUp = _baseCharacter.LevelProgression.GetXPForLevel(_currentLevel);
        _isScheduled = false;
    }

    /// <summary>저장 데이터에서 캐릭터를 복원한다 (불러오기용).</summary>
    public CharacterUnit(CharacterSO baseCharacter, int currentLevel, int currentXP, StatManager statManager, bool isScheduled)
    {
        _baseCharacter = baseCharacter;

        _status = CharacterStatus.Available;
        _statManager = statManager; // 저장된 스탯 그대로 사용

        _currentLevel = currentLevel;
        _currentXP = currentXP;

        _expToLevelUp = _baseCharacter.LevelProgression.GetXPForLevel(currentLevel);
        _isScheduled = isScheduled;
    }

    // ─── 상태 전환 메서드들 ───────────────────────────────────────────

    /// <summary>대기 상태로 변경하고 OnCharacterInAvailable 이벤트를 발행한다.</summary>
    public void SetStatusToAvailable()
    {
        _status = CharacterStatus.Available;
        OnCharacterInAvailable?.Invoke(this);
    }

    /// <summary>임무 수행 상태로 변경한다.</summary>
    public void SetStatusToInMission()
    {
        _status = CharacterStatus.InMission;
        OnCharacterInMission?.Invoke(this);
    }

    /// <summary>임무지로 이동 중 상태로 변경한다.</summary>
    public void SetStatusToInGoingToMission()
    {
        _status = CharacterStatus.Going;
        OnCharacterGoingToMission?.Invoke(this);
    }

    /// <summary>귀환 중 상태로 변경한다.</summary>
    public void SetStatusToReturning()
    {
        _status = CharacterStatus.Returning;
        OnCharacterReturning?.Invoke(this);
    }

    /// <summary>
    /// 임무 완료 처리. 경험치를 추가하고 귀환 상태로 전환한다.
    /// DayCharacterManager.HandleTeamCompleteMission()에서 호출한다.
    /// </summary>
    /// <param name="exp">획득할 경험치 (실패 시 0)</param>
    /// <param name="currentTime">현재 게임 내 시간 (귀환 시간 계산에 사용)</param>
    public void HandleMissionCompleted(int exp, float currentTime)
    {
        _status = CharacterStatus.Returning;
        AddExp(exp);

        OnCharacterReturning?.Invoke(this);
    }

    /// <summary>
    /// 휴식 상태로 전환한다. _startTime을 기록해 휴식 완료 시간을 추적한다.
    /// DayManager.HandleCharacterArriveBase()에서 호출한다.
    /// </summary>
    public void SetCharacterResting(float currentTime)
    {
        _startTime = currentTime;
        _status = CharacterStatus.Resting;
        OnCharacterInResting?.Invoke(this);
    }

    /// <summary>
    /// 스탯 포인트를 해당 스탯에 1 투자한다.
    /// _availablePoints가 없으면 아무것도 하지 않는다.
    /// </summary>
    public void AddPointInStat(StatManager.StatType stat)
    {
        if (_availablePoints == 0) return;

        _statManager.AddBonusToStat(stat, 1);

        _availablePoints = Math.Max(0, _availablePoints - 1); // 음수 방지

        OnCharacterStatChanged?.Invoke(this);
    }

    // ─── 매 틱 업데이트 ───────────────────────────────────────────────

    /// <summary>
    /// DayCharacterManager에서 매 0.5초마다 호출하는 업데이트.
    /// 휴식 중이라면 TimeToRest가 지났는지 체크해 Available로 복귀시킨다.
    /// </summary>
    public void UpdateCharacter(float currentTime)
    {
        if (IsResting())
        {
            if (currentTime - _startTime > _baseCharacter.TimeToRest)
            {
                SetStatusToAvailable(); // 휴식 완료 → 대기 상태로
            }
        }
    }

    /// <summary>
    /// 현재 상태 기준으로 완료까지의 진행률(0~1)을 반환한다.
    /// Resting: 휴식 완료까지의 비율 / 그 외: 항상 0
    /// UI 프로그레스 바 등에 사용한다.
    /// </summary>
    public float GetNormalizedTimeToBeAvailable(float currentTime)
    {
        var timeDiff = currentTime - _startTime;

        switch (_status)
        {
            case CharacterStatus.Resting: return timeDiff / _baseCharacter.TimeToRest;
            default : return 0f;
        }
    }

    // ─── 경험치/레벨업 ───────────────────────────────────────────────

    /// <summary>
    /// 경험치를 추가하고 레벨업을 처리한다.
    /// 레벨업 시 _availablePoints가 1씩 증가한다 (스탯 포인트 투자 가능).
    /// 한 번에 여러 레벨업도 처리한다 (while 루프).
    /// </summary>
    public void AddExp(int exp)
    {
        _currentXP += exp;

        if (_currentXP >= _expToLevelUp)
        {
            while (_currentXP >= _expToLevelUp)
            {
                _currentXP -= _expToLevelUp;       // 초과 경험치를 다음 레벨에 이월

                _currentLevel += 1;
                _availablePoints += 1;             // 레벨업마다 스탯 포인트 1 지급

                _expToLevelUp = _baseCharacter.LevelProgression.GetXPForLevel(_currentLevel);
            }

            OnCharacterLevelUP?.Invoke(this);
        }

        OnCharacterEXPChanged?.Invoke(this);
    }

    /// <summary>파견 배정 여부를 변경한다.</summary>
    public void SetScheduledCharater(bool isScheduled)
    {
        _isScheduled = isScheduled;
    }

    // ─── 상태 확인 헬퍼 ───────────────────────────────────────────────
    public bool IsInMission() => _status == CharacterStatus.InMission;
    public bool IsResting() => _status == CharacterStatus.Resting;
    public bool IsAvailable() => _status == CharacterStatus.Available;

    // ─── 읽기 전용 프로퍼티 ───────────────────────────────────────────
    public CharacterStatus Status { get { return _status; } }

    /// <summary>현재 경험치의 다음 레벨 대비 비율 (0~1). UI 경험치 바에 사용.</summary>
    public float NormalizedExp => _currentXP / (float)_expToLevelUp;
    public int Level => _currentLevel;
    public int AvailablePoints => _availablePoints;
    public int CurrentXP => _currentXP;

    public string Name => _baseCharacter.Name;
    public Sprite FaceArt => _baseCharacter.FaceArt;
    public Sprite BodyArt => _baseCharacter.BodyArt;
    public Sprite FullArt => _baseCharacter.FullArt;
    public bool IsScheduled => _isScheduled;
    public CharacterSO BaseCharacterSO => _baseCharacter;
    public StatManager StatManager { get { return _statManager; } }
}
