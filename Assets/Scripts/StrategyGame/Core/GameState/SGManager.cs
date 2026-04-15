using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static Guild;

/// <summary>
/// 게임 전체의 허브 역할을 하는 싱글턴 Manager.
/// 새 게임 생성, 저장/불러오기, 하루 완료 처리를 담당한다.
///
/// 의존 관계:
///   SGManager → CharacterDatabase (싱글턴, 씬에 배치 필요)
///   SGManager → CharacterLevelDatabase (싱글턴, 씬에 배치 필요)
///   SGManager → FactoryCharacterUnit → FactoryGameState → GameState
///   SGManager → SaveSystem (static)
/// </summary>
public class SGManager : Singleton<SGManager>
{
    /// <summary>
    /// 새 게임 또는 저장 파일 없을 때의 초기 게임 설정.
    /// Inspector에서 GameStateSO 인스턴스를 연결한다.
    /// </summary>
    [SerializeField] private GameStateSO DefaultGameState;

    private GameState _gameState; // 현재 게임 상태 (런타임)

    // 캐릭터 생성 팩토리와 게임 상태 생성 팩토리
    private FactoryCharacterUnit _factoryCharacterUnit;
    private FactoryGameState _factoryGameState;

    /// <summary>Awake에서 팩토리들을 초기화한다. FactoryCharacterUnit은 레퍼런스만 저장하므로 CharacterDatabase.Awake()와 순서 무관하게 안전하다.</summary>
    private void Awake()
    {
        _factoryCharacterUnit = new FactoryCharacterUnit(CharacterDatabase.Instance);
        _factoryGameState = new FactoryGameState(_factoryCharacterUnit);
    }

    /// <summary>게임 종료 시 발행되는 이벤트 (UI 종료 화면 등에 활용 가능).</summary>
    public UnityEvent OnQuitGame;

    /// <summary>
    /// 저장 파일을 불러와 _gameState를 초기화한다.
    /// 파일 없음 → 기본 설정으로 새 게임 생성
    /// 파일 형식 오류 → 기본 설정으로 새 게임 생성
    /// </summary>
    private void LoadData(string saveFile)
    {
        Debug.Log($"[{GetType()}][LoadData] Loading GameData ...");

        try
        {
            var gameStateData = SaveSystem.Load(saveFile);                           // JSON 불러오기
            _gameState = _factoryGameState.CreateGameState(saveFile, gameStateData); // 복원
            Debug.Log($"[{GetType()}][LoadData]         GameData Loaded!");
        }
        catch (GameNotFoundException ex)
        {
            Debug.LogWarning($"[{GetType()}][LoadData] Game Not found!"); // 파일 없음 → 기본값
            _gameState = _factoryGameState.CreateGameState(saveFile, "Guilda", DefaultGameState);
        }
        catch (BadFormatGameException ex)
        {
            Debug.LogWarning($"[{GetType()}][LoadData] Bad format game data!"); // 형식 오류 → 기본값
            _gameState = _factoryGameState.CreateGameState(saveFile, "Guilda", DefaultGameState);
        }
    }

    /// <summary>
    /// 하루 종료 처리. 날짜를 1 증가시키고 길드 경험치/레벨업을 처리한다.
    /// DayManager의 OnDayEnd 이벤트에 연결해 호출한다.
    /// </summary>
    public LevelUPDescription CompleteDay(DayReport dayReport)
    {
        _gameState.IncrementDay();                                                    // 날짜 +1
        return _gameState.Guild.HandleDayReport(dayReport, CharacterLevelDatabase.Instance); // 길드 경험치 처리
    }

    /// <summary>애플리케이션 종료 처리.</summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>앱 종료 시 현재 게임 상태를 자동 저장한다.</summary>
    private void OnApplicationQuit()
    {
        if (_gameState != null)
        {
            SaveSystem.Save(new GameStateData(_gameState)); // _gameState → JSON 저장
        }
    }

    /// <summary>지정한 저장 파일을 불러온다. UI에서 세이브 목록 선택 시 호출한다.</summary>
    public void LoadSave(string currentSave)
    {
        LoadData(currentSave);
    }

    /// <summary>
    /// 새 게임을 시작한다.
    /// guildName으로 저장 파일명을 만들고 기본 설정으로 GameState를 초기화한다.
    /// 예) "My Guild" → 저장 파일: "My_Guild.json"
    /// </summary>
    public void NewGame(string guildName)
    {
        var saveFile = guildName.Replace(" ", "_") + ".json"; // 공백을 _로 변환
        _gameState = _factoryGameState.CreateGameState(saveFile, guildName, DefaultGameState);
    }

    /// <summary>존재하는 세이브 파일 이름 목록을 반환한다. 세이브 선택 UI에 표시할 때 사용한다.</summary>
    public List<string> GetSaves()
    {
        return SaveSystem.GetAllSaveFileNames();
    }

    /// <summary>지정한 세이브 파일을 삭제한다.</summary>
    public void DeleteSave(string saveFile)
    {
        SaveSystem.DeleteSave(saveFile);
    }

    /// <summary>현재 게임 상태. DayManager 등이 SGManager.Instance.GameState로 접근한다.</summary>
    public GameState GameState => _gameState;
}
