using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 길드(플레이어)의 레벨 한 단계를 정의하는 ScriptableObject.
/// CharacterLevelDatabase에 목록으로 등록해 레벨별 경험치 요구량을 설정한다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerLevel_", menuName = "StrategySystem/Player Level")]
public class PlayerLevelSO : ScriptableObject
{
    /// <summary>이 레벨의 이름/설명 (예: "견습 길드", "정예 길드" 등). UI 표시용.</summary>
    public string LevelDescription;

    /// <summary>이 레벨에서 다음 레벨로 올라가기 위해 필요한 경험치.</summary>
    public int ExpToLevelUp;
}
