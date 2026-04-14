using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// GameState 생성을 전담하는 팩토리 클래스.
/// 두 가지 경로로 GameState를 만들 수 있다:
///   1. 저장 데이터(GameStateData)에서 복원 → LoadGame 시
///   2. 기본 설정(GameStateSO)으로 신규 생성 → NewGame 시
/// </summary>
public class FactoryGameState
{
    private FactoryCharacterUnit _factoryCharacterUnit; // 캐릭터 생성을 위임

    /// <summary>캐릭터 팩토리를 주입받아 초기화한다.</summary>
    public FactoryGameState(FactoryCharacterUnit factoryCharacterUnit)
    {
        _factoryCharacterUnit = factoryCharacterUnit;
    }

    /// <summary>
    /// 저장 데이터에서 GameState를 복원한다 (불러오기용).
    /// GameStateData → Guild → GameState 순으로 재구성한다.
    /// </summary>
    public GameState CreateGameState(string saveFile, GameStateData gameStateData)
    {
        // 저장된 길드 정보 추출
        var guildName = gameStateData.GuildData.Name;
        var balance = gameStateData.GuildData.Balance;
        var reputation = gameStateData.GuildData.Reputation;
        var currentLevel = gameStateData.GuildData.CurrentLevel;
        var currentExperience = gameStateData.GuildData.CurrentExperience;

        // 저장된 캐릭터 데이터를 CharacterUnit으로 복원
        var allCharacters = gameStateData.GuildData.AllCharacters
            .Select(c => _factoryCharacterUnit.CreateCharacterUnit(c))
            .ToList();

        var guild = new Guild(guildName, balance, reputation, currentLevel, currentExperience, allCharacters);

        return new GameState(saveFile, gameStateData.CurrentDay, guild);
    }

    /// <summary>
    /// 기본 설정(GameStateSO)으로 새 GameState를 생성한다 (신규 게임용).
    /// guildName은 플레이어가 직접 입력한 길드 이름이다.
    /// </summary>
    public GameState CreateGameState(string saveFile, string guildName, GameStateSO defaultGameState)
    {
        var balance = defaultGameState.Balance;
        var reputation = defaultGameState.Reputation;
        var currentLevel = defaultGameState.CurrentLevel;
        var currentExperience = defaultGameState.CurrentExperience;

        // GameStateSO에 등록된 초기 캐릭터 목록으로 CharacterUnit 생성
        var allcharacters = defaultGameState.AvailableCharacters
            .Select(c => _factoryCharacterUnit.CreateCharacterUnit(c))
            .ToList();

        var guild = new Guild(guildName, balance, reputation, currentLevel, currentExperience, allcharacters);

        return new GameState(saveFile, defaultGameState.CurrentDay, guild);
    }
}
