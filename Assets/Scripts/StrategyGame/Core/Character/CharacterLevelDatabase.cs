using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 길드(플레이어) 레벨 데이터 싱글턴.
/// Inspector에서 PlayerLevelSO 목록을 연결해두면
/// 레벨 번호로 해당 레벨의 설정값(필요 경험치 등)을 가져올 수 있다.
/// Guild.HandleDayReport()에서 레벨업 판정에 사용한다.
/// </summary>
public class CharacterLevelDatabase : Singleton<CharacterLevelDatabase>
{
    /// <summary>
    /// 레벨별 설정 목록. 인덱스 0 = 레벨 0, 인덱스 1 = 레벨 1 ...
    /// Inspector에서 순서대로 PlayerLevelSO 인스턴스를 연결한다.
    /// </summary>
    [SerializeField] private List<PlayerLevelSO> _levelDescriptionSOs;

    /// <summary>
    /// 레벨 번호에 해당하는 PlayerLevelSO를 반환한다.
    /// 범위를 벗어난 레벨은 Clamp로 안전하게 처리한다.
    /// </summary>
    public PlayerLevelSO GetLevel(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 0, _levelDescriptionSOs.Count); // 범위 초과 방지

        return _levelDescriptionSOs[clampedLevel];
    }
}
