using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 새 게임 시작 시 길드의 초기 상태를 정의하는 ScriptableObject.
/// GameManager.NewGame() 또는 저장 파일 없을 때의 기본값으로 사용된다.
/// Assets/Configuration/StrategyGame/Game/ 에 인스턴스를 생성한다.
/// </summary>
[CreateAssetMenu(fileName = "DefaultGameState", menuName = "StrategySystem/Game State")]
public class GameStateSO : ScriptableObject
{
    /// <summary>처음부터 길드에 속해 있는 캐릭터 목록. Inspector에서 CharacterSO를 연결한다.</summary>
    public List<CharacterSO> AvailableCharacters;

    public int Balance;           // 시작 골드
    public int Reputation;        // 시작 평판
    public int CurrentDay;        // 시작 날짜 (보통 1)
    public int CurrentLevel;      // 시작 길드 레벨 (보통 1)
    public int CurrentExperience; // 시작 길드 경험치 (보통 0)
}
