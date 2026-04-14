using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스탯 하나(Stat)를 JSON으로 저장하기 위한 직렬화 구조.
/// Stat은 런타임 전용이고, StatData는 저장/불러오기 전용이다.
/// 불러올 때: new Stat(statData.BaseValue, statData.Bonus) 으로 복원한다.
/// </summary>
[Serializable]
public class StatData
{
    public int BaseValue;     // 기본 스탯 수치
    public List<int> Bonus;   // 보너스 수치 목록 (스탯 포인트 투자, 버프 등)

    /// <summary>런타임 Stat에서 저장 데이터를 생성한다.</summary>
    public StatData(Stat stat)
    {
        BaseValue = stat.BaseValue;
        Bonus = stat.Bonus; // Stat의 Bonus 리스트를 그대로 참조 (JSON 변환 시 복사됨)
    }
}
