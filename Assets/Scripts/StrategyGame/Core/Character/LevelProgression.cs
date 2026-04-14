using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨별 필요 경험치 곡선을 정의하는 ScriptableObject.
/// AnimationCurve로 경험치 증가 패턴을 자유롭게 설정할 수 있다.
/// 각 CharacterSO에 연결해 캐릭터마다 다른 성장 곡선을 가질 수 있다.
/// </summary>
[CreateAssetMenu(fileName = "LevelProgression", menuName = "StrategySystem/LevelProgression")]
public class LevelProgression : ScriptableObject
{
    /// <summary>
    /// 레벨별 경험치 요구량 곡선.
    /// X축: 레벨 비율(0~1), Y축: 경험치 비율(0~1)
    /// 커브를 위로 볼록하게 만들면 고레벨일수록 더 많은 경험치 필요.
    /// </summary>
    public AnimationCurve XPCurve;

    /// <summary>최대 레벨 (커브의 X축이 0~MaxLevel로 매핑됨).</summary>
    public int MaxLevel = 10;

    /// <summary>
    /// 경험치 배율. 커브 Y값(0~1)에 이 값을 곱해 실제 경험치 요구량을 산출한다.
    /// 예) 커브 Y=0.5, MultiplyXPBy=1000 → 해당 레벨 필요 경험치 = 500
    /// </summary>
    public int MultiplyXPBy = 1000;

    /// <summary>
    /// 특정 레벨에서 다음 레벨로 올라가기 위해 필요한 경험치를 반환한다.
    /// 레벨을 1~MaxLevel로 클램프해 범위를 벗어나도 안전하게 처리한다.
    /// </summary>
    public int GetXPForLevel(int level)
    {
        var clampedLevel = Mathf.Clamp(level, 1, MaxLevel);                          // 레벨 범위 제한
        var normalizedLevel = (float)(clampedLevel) / (MaxLevel - 1);                // 0~1로 정규화
        return Mathf.RoundToInt(XPCurve.Evaluate(normalizedLevel) * MultiplyXPBy);  // 커브 값 × 배율
    }
}
