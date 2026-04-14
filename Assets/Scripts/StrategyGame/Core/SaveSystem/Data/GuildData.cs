using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 길드 데이터를 저장하기 위한 직렬화 구조.
/// Guild(런타임) → GuildData(저장) → Guild(복원) 흐름으로 사용된다.
/// </summary>
[Serializable]
public class GuildData
{
    public string Name;                            // 길드 이름
    public int CurrentLevel;                       // 현재 길드 레벨
    public int CurrentExperience;                  // 현재 길드 경험치
    public int Balance;                            // 보유 골드
    public int Reputation;                         // 평판 수치
    public List<CharacterUnitData> AllCharacters;  // 소속 캐릭터 저장 데이터 목록

    /// <summary>런타임 Guild에서 저장 데이터를 생성한다.</summary>
    public GuildData(Guild guild)
    {
        Name = guild.PlayerName;
        Balance = guild.Balance;
        Reputation = guild.Reputation;
        CurrentLevel = guild.CurrentLevel;
        CurrentExperience = guild.CurrentExperience;

        // 모든 캐릭터 런타임 데이터를 직렬화 가능한 형태로 변환
        AllCharacters = guild.AllCharacters.Select(c => new CharacterUnitData(c)).ToList();
    }

    /// <summary>기본값으로 초기화한다 (역직렬화용 기본 생성자).</summary>
    public GuildData()
    {
        Name = "ERROR";
        Balance = 0;
        Reputation = 0;
        CurrentLevel = 1;
        CurrentExperience = 0;
        AllCharacters = new List<CharacterUnitData>();
    }
}
