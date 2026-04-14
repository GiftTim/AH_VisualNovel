using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatManager;

/// <summary>
/// CharacterUnit 생성을 전담하는 팩토리 클래스.
/// CharacterDatabase 의존성을 주입받아 저장 데이터 또는 SO에서 CharacterUnit을 만든다.
/// GameManager.Start()에서 인스턴스를 생성하고 FactoryGameState에 전달한다.
/// </summary>
public class FactoryCharacterUnit
{
    private CharacterDatabase _characterDatabase; // ID로 CharacterSO를 검색할 DB

    /// <summary>CharacterDatabase를 주입받아 팩토리를 초기화한다.</summary>
    public FactoryCharacterUnit(CharacterDatabase characterDatabase)
    {
        _characterDatabase = characterDatabase;
    }

    /// <summary>
    /// 저장 데이터(CharacterUnitData)에서 CharacterUnit을 복원한다.
    /// 게임 불러오기(LoadGame) 시 사용한다.
    /// 저장된 스탯 보너스까지 정확하게 재구성한다.
    /// </summary>
    public CharacterUnit CreateCharacterUnit(CharacterUnitData data)
    {
        // 저장된 ID로 CharacterSO(원형 데이터)를 DB에서 검색
        var baseCharacterSO = _characterDatabase.GetCharacterSO(data.CharacterID);
        var currentXP = data.CurrentXP;
        var currentLevel = data.CurrentLevel;
        var isScheduled = data.IsScheduled;

        // 저장된 스탯 데이터로 StatManager 재구성
        var statManager = new StatManager();
        statManager.SetStat(StatType.Strengh,      new Stat(data.StatManagerData.Strength.BaseValue,     data.StatManagerData.Strength.Bonus));
        statManager.SetStat(StatType.Endurance,    new Stat(data.StatManagerData.Endurance.BaseValue,    data.StatManagerData.Endurance.Bonus));
        statManager.SetStat(StatType.Agility,      new Stat(data.StatManagerData.Agility.BaseValue,      data.StatManagerData.Agility.Bonus));
        statManager.SetStat(StatType.Charisma,     new Stat(data.StatManagerData.Charisma.BaseValue,     data.StatManagerData.Charisma.Bonus));
        statManager.SetStat(StatType.Intelligence, new Stat(data.StatManagerData.Intelligence.BaseValue, data.StatManagerData.Intelligence.Bonus));

        return new CharacterUnit(baseCharacterSO, currentXP, currentLevel, statManager, isScheduled);
    }

    /// <summary>
    /// CharacterSO에서 새 CharacterUnit을 생성한다.
    /// 신규 게임(NewGame) 시 GameStateSO.AvailableCharacters 목록으로 캐릭터를 만들 때 사용한다.
    /// </summary>
    public CharacterUnit CreateCharacterUnit(CharacterSO character)
    {
        return new CharacterUnit(character);
    }
}
