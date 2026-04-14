using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StatManager의 5가지 스탯을 저장하기 위한 직렬화 구조.
/// StatManager(런타임) → StatManagerData(저장) → StatManager(복원) 흐름으로 사용된다.
/// 각 스탯은 StatData(BaseValue + Bonus 목록)로 저장된다.
/// </summary>
[Serializable]
public class StatManagerData
{
    public StatData Strength;     // 근력 저장 데이터
    public StatData Endurance;    // 인내 저장 데이터
    public StatData Agility;      // 민첩 저장 데이터
    public StatData Charisma;     // 카리스마 저장 데이터
    public StatData Intelligence; // 지능 저장 데이터

    /// <summary>런타임 StatManager에서 저장 데이터를 생성한다.</summary>
    public StatManagerData(StatManager statManager)
    {
        // StatType 열거형으로 각 스탯을 꺼내 StatData로 변환
        Strength     = new StatData(statManager.GetStat(StatManager.StatType.Strengh));
        Endurance    = new StatData(statManager.GetStat(StatManager.StatType.Endurance));
        Agility      = new StatData(statManager.GetStat(StatManager.StatType.Agility));
        Charisma     = new StatData(statManager.GetStat(StatManager.StatType.Charisma));
        Intelligence = new StatData(statManager.GetStat(StatManager.StatType.Intelligence));
    }

    /// <summary>기본값으로 초기화한다 (역직렬화용 기본 생성자).</summary>
    public StatManagerData() { }
}
