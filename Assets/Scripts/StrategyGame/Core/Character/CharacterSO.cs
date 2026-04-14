using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터의 기본 설정값을 담는 ScriptableObject.
/// 실제 런타임 데이터는 CharacterUnit이 담당하며,
/// CharacterSO는 변하지 않는 "캐릭터 원형(템플릿)" 역할을 한다.
/// Assets/Configuration/StrategyGame/Character/ 에 인스턴스를 생성한다.
/// </summary>
[CreateAssetMenu(fileName = "CharacterSO", menuName = "StrategySystem/Character/CharacterSO")]
public class CharacterSO : SavebleSO // SavebleSO 상속으로 ID 필드를 갖는다
{
    public string Name;                   // 캐릭터 이름
    public StatManager BaseStats;         // 초기 기본 스탯 (CharacterUnit 생성 시 복사됨)
    public Sprite FaceArt;                // 얼굴 이미지 (UI 목록 등에 사용)
    public Sprite BodyArt;                // 상반신 이미지
    public Sprite FullArt;                // 전신 이미지 (캐릭터 카드 등에 사용)
    public LevelProgression LevelProgression; // 이 캐릭터의 레벨업 경험치 곡선
    public int TimeToRest;                // 임무 완료 후 회복에 필요한 시간(초)
    public List<KeywordSO> Keywords;      // 이 캐릭터의 직업/특성 키워드 목록
}
