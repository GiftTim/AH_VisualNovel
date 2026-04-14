using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스탯 하나의 데이터를 담는 클래스.
/// 기본 값(BaseValue)과 추가 보너스 목록(Bonus)으로 구성된다.
/// 최종 값 = BaseValue + 모든 Bonus의 합계
/// </summary>
[Serializable] // Inspector에 표시되고 JSON 직렬화가 가능하도록 설정
public class Stat
{
    public int BaseValue; // 캐릭터 기본 스탯 수치 (레벨업, 아이템 등에 의해 변하지 않는 기초값)
    public List<int> Bonus; // 임시 보너스 목록 (장비, 버프, 포인트 투자 등으로 추가된 값들)

    /// <summary>기본 값과 보너스 목록을 직접 지정해 생성한다.</summary>
    public Stat(int baseValue, List<int> bonus)
    {
        BaseValue = baseValue;
        Bonus = bonus;
    }

    /// <summary>다른 Stat을 복사해 새 인스턴스를 만든다 (깊은 복사).</summary>
    public Stat(Stat stat)
    {
        BaseValue = stat.BaseValue;
        Bonus = new List<int>(stat.Bonus); // 리스트를 새로 생성해 원본과 독립적으로 관리
    }

    /// <summary>기본 값과 모든 보너스를 합산한 최종 스탯 수치를 반환한다.</summary>
    public int GetValue()
    {
        var value = BaseValue;

        Bonus.ForEach(x => value += x); // 모든 보너스를 더함

        return value;
    }

    /// <summary>보너스 목록에 값을 추가한다 (스탯 포인트 투자, 버프 등).</summary>
    public void AddBonus(int bonus)
    {
        Bonus.Add(bonus);
    }

    /// <summary>보너스 목록에서 값을 제거한다 (디버프 해제, 장비 해제 등).</summary>
    public void RmvBonus(int bonus)
    {
        Bonus.Remove(bonus);
    }
}
