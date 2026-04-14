using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 진행 중인 게임 전체의 런타임 상태를 담는 컨테이너.
/// 저장 파일명, 현재 날짜(Day), 길드 정보를 보관한다.
/// GameManager가 소유하며, 저장 시 GameStateData로 직렬화된다.
/// </summary>
public class GameState
{
    public string _saveFile;  // 이 게임의 저장 파일명 (예: "길드명.json")
    private Guild _guild;     // 길드 데이터 (캐릭터 목록, 골드, 평판 등)
    private int _currentDay;  // 현재 날짜 (1일차부터 시작)

    /// <summary>저장 파일명, 시작 날짜, 길드 데이터로 게임 상태를 초기화한다.</summary>
    public GameState(string saveFile, int day, Guild guild)
    {
        _saveFile = saveFile;
        _guild = guild;
        _currentDay = day;
    }

    /// <summary>하루가 끝날 때 호출해 날짜를 1 증가시킨다.</summary>
    public void IncrementDay()
    {
        _currentDay++;
    }

    public int Day => _currentDay;       // 현재 날짜
    public Guild Guild => _guild;        // 길드 데이터
    public string SaveFile => _saveFile; // 저장 파일명
}
