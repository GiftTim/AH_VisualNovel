using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어가 운영하는 길드의 데이터와 로직을 담당하는 클래스.
/// 골드(Balance), 평판(Reputation), 길드 레벨/경험치, 소속 캐릭터 목록을 관리한다.
/// GameState의 구성 요소로 GameManager → GameState → Guild 구조로 접근한다.
/// </summary>
public class Guild
{
    private string _playerName;            // 길드 이름
    private int _balance;                  // 보유 골드
    private int _reputation;               // 평판 수치
    private int _currentLevel;            // 길드 레벨
    private int _currentExperience;       // 현재 길드 경험치
    private List<CharacterUnit> _allCharacters; // 길드 소속 전체 캐릭터

    /// <summary>길드를 초기 데이터로 생성한다.</summary>
    public Guild(string name, int balance, int popularity, int currentLevel, int currentExperience, List<CharacterUnit> characters)
    {
        _playerName = name;
        _balance = balance;
        _reputation = popularity;
        _currentLevel = currentLevel;
        _currentExperience = currentExperience;
        _allCharacters = characters;
    }

    /// <summary>캐릭터의 파견 배정 상태를 변경한다.</summary>
    public void SetScheduledCharacter(CharacterUnit character, bool isScheduled)
    {
        character.SetScheduledCharater(isScheduled);
    }

    /// <summary>길드 골드를 증가시킨다 (임무 보상 등).</summary>
    public void AddGold(int gold)
    {
        _balance += gold;
    }

    /// <summary>길드 평판을 증가시킨다.</summary>
    public void AddReputation(int reputation)
    {
        _reputation += reputation;
    }

    /// <summary>길드 평판을 감소시킨다 (임무 실패 페널티 등).</summary>
    public void RmvReputation(int reputation)
    {
        _reputation -= reputation;
    }

    /// <summary>
    /// 하루 임무 결과(DayReport)를 처리해 길드 경험치와 레벨업을 반영한다.
    /// LevelUPDescription(레벨업 UI에 표시할 결과 정보)을 반환한다.
    ///
    /// 경험치 계산 방식:
    ///   총 경험치 = 성공한 임무 수
    ///   차감 = 실패한 임무 수 + 놓친 임무 수
    ///   최종 획득 경험치 = Max(0, 총 경험치 - 차감)
    /// </summary>
    public LevelUPDescription HandleDayReport(DayReport dayReport, CharacterLevelDatabase levelDatabase)
    {
        var levelSO = levelDatabase.GetLevel(CurrentLevel);
        var oldExpPerc = _currentExperience / (float)levelSO.ExpToLevelUp; // UI 이전 비율 저장

        var totalExp = dayReport.MissionSucceded;       // 성공 임무 수 = 획득 경험치
        var expLostByFailures = dayReport.MissionFailed; // 실패 임무 수 = 경험치 차감
        var expLostByMisses = dayReport.MissionMisses;   // 놓친 임무 수 = 경험치 차감

        var levelGained = 0;
        var expToAdd = Mathf.Max(0, totalExp - expLostByFailures - expLostByMisses); // 음수 방지

        _currentExperience += expToAdd;

        Debug.Log($"Call {dayReport.TotalCalls}   Success={dayReport.MissionSucceded} Fail={dayReport.MissionFailed} Misses={dayReport.MissionMisses}");
        Debug.Log($"ExpToAdd={expToAdd}  Experience{_currentExperience}  {levelSO.ExpToLevelUp}  {_currentLevel}");

        // 레벨업 처리 (한 번에 여러 레벨업 가능)
        while (_currentExperience >= levelSO.ExpToLevelUp)
        {
            _currentLevel++;
            _currentExperience -= levelSO.ExpToLevelUp;

            levelSO = levelDatabase.GetLevel(_currentLevel); // 새 레벨 SO로 갱신
            levelGained++;
        }

        // UI에 표시할 경험치 비율들 계산 (레벨업 애니메이션용)
        var currentPerc = _currentExperience / (float)levelSO.ExpToLevelUp;
        var successPerc = dayReport.MissionSucceded / (float)levelSO.ExpToLevelUp;
        var failPerc = dayReport.MissionFailed / (float)levelSO.ExpToLevelUp;
        var missPerc = dayReport.MissionMisses / (float)levelSO.ExpToLevelUp;

        var expWithSuccesPerc = currentPerc + failPerc + missPerc;
        var expWithoutFailuresPerc = Mathf.Max(0, expWithSuccesPerc - failPerc);
        var expWithoutMissesPerc = Mathf.Max(0, expWithoutFailuresPerc - missPerc);

        Debug.Log($"Old={oldExpPerc} CurrentPerc={currentPerc} SuccessPerc={successPerc} FailPerc={failPerc} MissPerc={missPerc}");
        Debug.Log($"expS={expWithSuccesPerc}  expWithoutFail={expWithoutFailuresPerc}  ExpWithoutMiss={expWithoutMissesPerc}");

        return new LevelUPDescription(oldExpPerc, expWithSuccesPerc, expWithoutFailuresPerc, expWithoutMissesPerc, levelGained, levelSO);
    }

    /// <summary>현재 길드 경험치를 0~1 비율로 반환한다. UI 경험치 바에 사용.</summary>
    public float GetCurrentExperienceNormalized(CharacterLevelDatabase characterLevelDatabse)
    {
        return _currentExperience / (float)characterLevelDatabse.GetLevel(_currentLevel).ExpToLevelUp;
    }

    // ─── 읽기 전용 프로퍼티 ───────────────────────────────────────────
    public string PlayerName => _playerName;
    public int Balance => _balance;
    public int Reputation => _reputation;
    public int CurrentLevel => _currentLevel;
    public int CurrentExperience => _currentExperience;
    public List<CharacterUnit> AllCharacters => _allCharacters;

    /// <summary>파견 중이지 않은 캐릭터만 필터링해 반환한다.</summary>
    public List<CharacterUnit> AvailableCharacters => _allCharacters.Where(c => !c.IsScheduled).ToList();

    /// <summary>현재 파견 중인 캐릭터만 필터링해 반환한다.</summary>
    public List<CharacterUnit> ScheduledCharacters => _allCharacters.Where(c => c.IsScheduled).ToList();

    /// <summary>
    /// 하루 결과 UI에 표시할 레벨업 관련 정보를 담는 내부 클래스.
    /// HandleDayReport()의 반환값으로, 경험치 변화 애니메이션 등에 활용한다.
    /// </summary>
    public class LevelUPDescription
    {
        public float OldExpPerc { get; }                             // 하루 시작 전 경험치 비율
        public float ExpWithSuccess { get; }                         // 성공 경험치 적용 후 비율
        public float ExpWithSuccessWithoutFailures { get; }          // 실패 차감 후 비율
        public float ExpWithSuccessWithoutFailuresAndMisses { get; } // 미스 차감 후 최종 비율
        public int LevelGained { get; }                              // 이번에 올라간 레벨 수
        public PlayerLevelSO LevelSO { get; }                        // 현재 레벨의 SO

        public LevelUPDescription(float oldExpPerc, float expWithSuccess, float expWithSuccessWithoutFailures,
            float expWithSuccessWithoutFailuresAndMisses, int levelGained, PlayerLevelSO levelSO)
        {
            OldExpPerc = oldExpPerc;
            ExpWithSuccess = expWithSuccess;
            ExpWithSuccessWithoutFailures = expWithSuccessWithoutFailures;
            ExpWithSuccessWithoutFailuresAndMisses = expWithSuccessWithoutFailuresAndMisses;
            LevelGained = levelGained;
            LevelSO = levelSO;
        }
    }
}
