using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터의 직업/특성을 나타내는 키워드 열거형.
/// 임무 요구 조건 판단 등에 활용될 수 있다.
/// </summary>
public enum KeywordValue
{
    Warrior,  // 전사
    Mage,     // 마법사
    Cleric,   // 성직자
    Archer,   // 궁수
    Barbarian // 야만전사
}

/// <summary>
/// 캐릭터의 직업/특성 키워드 ScriptableObject.
/// CharacterSO.Keywords 리스트에 등록해 캐릭터 특성을 정의한다.
/// </summary>
[CreateAssetMenu(fileName = "Keyword_", menuName = "StrategySystem/Character/Keyword")]
public class KeywordSO : ScriptableObject
{
    public string Name;             // 키워드 표시 이름
    public KeywordValue KeywordValue; // 키워드 종류 (열거형)
    public Sprite Art;              // 키워드 아이콘 이미지 (UI용)
}
