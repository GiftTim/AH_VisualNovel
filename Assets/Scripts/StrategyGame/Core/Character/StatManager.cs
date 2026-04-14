using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터의 5가지 스탯(근력/인내/민첩/카리스마/지능)을 관리하는 클래스.
/// 각 스탯은 Stat 클래스로 개별 관리되며, 팀 스탯 합산이나 저장/불러오기 시 사용된다.
/// </summary>
[Serializable]
public class StatManager
{
    private const string EXPCETION_MESSAGE_STAT_NOT_FOUND = "Stat '{0}' not found in method call '{1}'";

    /// <summary>스탯의 종류를 나타내는 열거형.</summary>
    public enum StatType
    {
        Strengh,       // 근력 (물리 전투력)
        Endurance,     // 인내 (체력, 지속력)
        Agility,       // 민첩 (속도, 회피)
        Charisma,      // 카리스마 (협상, 사교)
        Intelligence   // 지능 (마법, 분석)
    }

    // 5가지 스탯 각각을 Stat 클래스로 보관
    [SerializeField] private Stat _strengh;
    [SerializeField] private Stat _endurance;
    [SerializeField] private Stat _agility;
    [SerializeField] private Stat _charisma;
    [SerializeField] private Stat _intelligence;

    // 스탯의 최대치 (GetValues()에서 정규화할 때 기준으로 사용)
    [SerializeField] private int _maxStatValue = 10;

    /// <summary>모든 스탯을 0으로 초기화해 생성한다.</summary>
    public StatManager()
    {
        _strengh = new Stat(0, new List<int>());
        _endurance = new Stat(0, new List<int>());
        _agility = new Stat(0, new List<int>());
        _charisma = new Stat(0, new List<int>());
        _intelligence = new Stat(0, new List<int>());
    }

    /// <summary>다른 StatManager를 복사해 새 인스턴스를 만든다 (깊은 복사).</summary>
    public StatManager(StatManager statManager)
    {
        _strengh = new Stat(statManager._strengh);
        _endurance = new Stat(statManager._endurance);
        _agility = new Stat(statManager._agility);
        _charisma = new Stat(statManager._charisma);
        _intelligence = new Stat(statManager._intelligence);
    }

    /// <summary>StatType 열거형으로 해당 스탯 객체를 가져온다.</summary>
    public Stat GetStat(StatType statType)
    {
        // C# switch 식 (간결한 패턴 매칭)
        return statType switch
        {
            StatType.Strengh => _strengh,
            StatType.Endurance => _endurance,
            StatType.Agility => _agility,
            StatType.Charisma => _charisma,
            StatType.Intelligence => _intelligence,
            _ => throw new ArgumentOutOfRangeException(nameof(statType), statType,
                string.Format(EXPCETION_MESSAGE_STAT_NOT_FOUND, statType, "GetStat"))
        };
    }

    /// <summary>
    /// 다른 StatManager의 스탯 값들을 현재 스탯에 합산한다.
    /// 팀 전체 스탯을 계산할 때 사용한다 (Team.GetTeamStats).
    /// </summary>
    public void ExpendStats(StatManager stats)
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            var currentValue = GetStat(statType).BaseValue;
            var currentBonus = GetStat(statType).Bonus;

            var statToAddValue = stats.GetStat(statType).BaseValue;
            var statToAddBonus = stats.GetStat(statType).Bonus;

            var newBaseValue = currentValue + statToAddValue;
            var newBonus = new List<int>();
            newBonus.AddRange(currentBonus);
            newBonus.AddRange(statToAddBonus);

            var newStat = new Stat(newBaseValue, newBonus);

            SetStat(statType, newStat);
        }
    }

    /// <summary>StatType 열거형으로 해당 스탯 객체를 교체한다.</summary>
    public void SetStat(StatType statType, Stat newStat)
    {
        switch (statType)
        {
            case StatType.Strengh: _strengh = newStat; break;
            case StatType.Endurance: _endurance = newStat; break;
            case StatType.Agility: _agility = newStat; break;
            case StatType.Charisma: _charisma = newStat; break;
            case StatType.Intelligence: _intelligence = newStat; break;
            default: throw new ArgumentOutOfRangeException(nameof(statType), statType,
                string.Format(EXPCETION_MESSAGE_STAT_NOT_FOUND, statType, "SetStat"));
        }
    }

    /// <summary>
    /// 5가지 스탯의 최종 값을 0~1 사이 정규화된 float 리스트로 반환한다.
    /// 레이더 차트 UI 등에서 비율로 표시할 때 사용한다.
    /// 순서: 근력 → 인내 → 민첩 → 카리스마 → 지능
    /// </summary>
    public List<float> GetValues()
    {
        var order = new List<StatType>()
        {
            StatType.Strengh, StatType.Endurance, StatType.Agility,
            StatType.Charisma, StatType.Intelligence
        };

        var values = new List<float>();
        order.ForEach(stat => values.Add(GetStat(stat).GetValue() / (float)_maxStatValue));

        return values;
    }

    /// <summary>특정 스탯에 보너스 수치를 추가한다 (스탯 포인트 투자 등).</summary>
    public void AddBonusToStat(StatType statType, int bonus)
    {
        var stat = GetStat(statType);
        stat.AddBonus(bonus);
    }

    /// <summary>특정 스탯에서 보너스 수치를 제거한다.</summary>
    public void RmvBonusToStat(StatType statType, int bonus)
    {
        var stat = GetStat(statType);
        stat.RmvBonus(bonus);
    }

    /// <summary>디버그 출력용 문자열 (S/E/A/C/I 최종값 표시).</summary>
    public override string ToString()
    {
        return $"S:{_strengh.GetValue()};E:{_endurance.GetValue()};A:{_agility.GetValue()};C:{_charisma.GetValue()};I:{_intelligence.GetValue()}";
    }
}
