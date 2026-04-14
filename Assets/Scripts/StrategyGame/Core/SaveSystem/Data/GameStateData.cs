using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 상태를 JSON으로 저장하기 위한 직렬화 구조체.
/// [Serializable] 어노테이션 덕분에 JsonUtility.ToJson()으로 변환 가능하다.
/// GameState(런타임 데이터) ↔ GameStateData(저장 데이터) 간 변환이 이루어진다.
/// </summary>
[Serializable]
public class GameStateData
{
    public string SaveFile;   // 이 데이터가 저장될 파일명 (예: "MyGuild.json")
    public int CurrentDay;    // 현재 날짜
    public GuildData GuildData; // 길드 저장 데이터

    /// <summary>런타임 GameState에서 저장 데이터를 생성한다 (저장 시 사용).</summary>
    public GameStateData(GameState gameState)
    {
        SaveFile = gameState.SaveFile;
        CurrentDay = gameState.Day;
        GuildData = new GuildData(gameState.Guild); // Guild 런타임 → GuildData 저장용
    }

    /// <summary>기본값으로 초기화한다 (JsonUtility 역직렬화 등에 필요한 기본 생성자).</summary>
    public GameStateData()
    {
        SaveFile = "defaultSaveFile.json";
        CurrentDay = 0;
        GuildData = new GuildData();
    }
}
