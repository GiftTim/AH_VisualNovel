using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터 한 명의 런타임 상태를 저장하기 위한 직렬화 구조.
/// CharacterUnit(런타임) → CharacterUnitData(저장) → CharacterUnit(복원) 흐름으로 사용된다.
/// CharacterSO 자체는 저장하지 않고 ID만 저장한다.
/// (불러올 때 CharacterDatabase에서 ID로 CharacterSO를 다시 찾는다)
/// </summary>
[Serializable]
public class CharacterUnitData
{
    public string CharacterID;          // CharacterSO의 ID (불러올 때 DB 조회에 사용)
    public int CurrentXP;               // 현재 경험치
    public int CurrentLevel;            // 현재 레벨
    public int AvailablePoints;         // 미투자 스탯 포인트
    public bool IsScheduled;            // 파견 배정 여부
    public StatManagerData StatManagerData; // 스탯 저장 데이터 (보너스 포함)

    /// <summary>런타임 CharacterUnit에서 저장 데이터를 생성한다.</summary>
    public CharacterUnitData(CharacterUnit characterUnit)
    {
        CharacterID = characterUnit.BaseCharacterSO.ID; // SO 전체가 아닌 ID만 저장
        CurrentXP = characterUnit.CurrentXP;
        CurrentLevel = characterUnit.Level;
        AvailablePoints = characterUnit.AvailablePoints;
        IsScheduled = characterUnit.IsScheduled;
        StatManagerData = new StatManagerData(characterUnit.StatManager); // 스탯 저장
    }

    /// <summary>기본값으로 초기화한다 (역직렬화용 기본 생성자).</summary>
    public CharacterUnitData() { }

    /// <summary>디버그 출력용 문자열.</summary>
    public override string ToString()
    {
        return $"{CharacterID};{CurrentXP};{CurrentLevel};{AvailablePoints};{IsScheduled}";
    }
}
