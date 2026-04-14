using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatManager;

/// <summary>
/// 임무 하나의 설정값을 정의하는 ScriptableObject.
/// 임무 이름, 설명, 보상, 요구 스탯, 소요 시간 등 정적 데이터를 담는다.
/// 런타임 임무 인스턴스는 MissionUnit이 담당하며 MissionSO를 참조한다.
/// Assets/Configuration/StrategyGame/ 에 인스턴스를 생성한다.
///
/// Phase C에서는 DaySO.MissionSOs 필드 타입으로만 사용되어 컴파일을 위해 포함됨.
/// Phase 8 (A조합) 에서 DayMissionManager와 함께 실제로 활성화된다.
/// </summary>
[CreateAssetMenu(fileName = "Mission", menuName = "StrategySystem/Mission")]
public class MissionSO : ScriptableObject
{
    /// <summary>임무 종류 열거형.</summary>
    public enum MissionType
    {
        Rescure,            // 구조
        Attack,             // 공격
        Negociation,        // 협상
        Assault,            // 강습
        Investigation,      // 조사
        Pursuit,            // 추격
        Minor_Inconvenience,// 소소한 문제
        Social_Event,       // 사교 이벤트
        Security,           // 경호
        Red_Ring            // 레드 링 (특수)
    }

    [Header("Mission")]
    public string Name;                                           // 임무 이름
    public string Description;                                    // 임무 설명
    public List<MissionRequirement> RequirementDescriptionItems;  // 표시용 요구 조건 설명
    public int DifficultyLevel;                                   // 난이도
    public StatManager RequiredStats;                             // 임무 성공에 필요한 최소 스탯
    public int MaxTeamSize;                                       // 파견 가능한 최대 팀 인원
    public int RewardExperience;                                  // 성공 시 캐릭터 경험치 보상
    public int RewardGold;                                        // 성공 시 골드 보상
    public int RewardReputation;                                  // 성공/실패 시 평판 변화
    public int TimeToAccept;                                      // 임무 수락 가능 시간(초). 초과 시 임무 소멸
    public int TimeToComplete;                                    // 임무 완료까지 소요 시간(초)
    public int TimeToAnswerEvent;                                 // 랜덤 이벤트 응답 가능 시간(초)

    [Header("Addiciona Info")]
    public Sprite ClientArt;       // 의뢰인 이미지
    public Sprite EnvironmentArt;  // 임무 배경 이미지
    public MissionType Type;       // 임무 종류

    [Header("Addicional Events")]
    /// <summary>임무 진행 중 발생할 수 있는 랜덤 이벤트 목록. 비어 있으면 이벤트 없음.</summary>
    public List<RandomMissionEvent> RandomMissionEvents = new List<RandomMissionEvent>();

    /// <summary>임무 도중 발생하는 랜덤 이벤트 데이터.</summary>
    [Serializable]
    public class RandomMissionEvent
    {
        public string Title;                    // 이벤트 제목
        public string Description;              // 이벤트 설명
        public List<MissionChoice> MissionChoices; // 선택지 목록
    }

    /// <summary>랜덤 이벤트의 선택지 데이터.</summary>
    [Serializable]
    public class MissionChoice
    {
        [Header("Choice Info")]
        public MissionRequirement Requirement;    // 선택지 요구 조건
        public int StatAmountRequired;            // 필요 스탯 수치
        public string SucceededDescription;       // 성공 시 설명
        public string FailedDescription;          // 실패 시 설명

        [Header("Hero Required (optional)")]
        public CharacterSO Character;             // 특정 캐릭터가 있어야 선택 가능 (선택적)
    }

    /// <summary>임무/이벤트의 스탯 요구 조건 설명 데이터.</summary>
    [Serializable]
    public class MissionRequirement
    {
        public string Description;   // 요구 조건 설명 텍스트 (UI 표시용)
        public StatType StatType;    // 요구하는 스탯 종류
    }
}
