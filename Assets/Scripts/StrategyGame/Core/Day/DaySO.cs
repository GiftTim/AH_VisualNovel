using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하루의 난이도와 상태를 나타내는 열거형.
/// 날짜가 지날수록 Danger에 가까워지는 식으로 활용할 수 있다.
/// </summary>
public enum DayStatus
{
    Normal,  // 일반 (여유 있음)
    Warning, // 경고 (약간 긴박)
    Danger   // 위험 (긴박)
}

/// <summary>
/// 하루의 설정값을 정의하는 ScriptableObject.
/// 날짜마다 다른 임무 목록과 하루 길이를 설정할 수 있다.
/// DayManager._daySOs 목록에 날짜 순서대로 등록하거나,
/// _defaultDaySO에 기본값을 설정해 목록을 벗어난 날짜에 사용한다.
/// Assets/Configuration/StrategyGame/Days/ 에 인스턴스를 생성한다.
/// </summary>
[CreateAssetMenu(fileName = "Day", menuName = "StrategySystem/Day")]
public class DaySO : ScriptableObject
{
    /// <summary>이날 등장할 임무 목록 (MissionSO 인스턴스를 연결한다).</summary>
    public List<MissionSO> MissionSOs;

    /// <summary>true이면 MissionSOs 전체를 사용, false이면 MissionAmount 개수만 무작위 선택.</summary>
    public bool UseAllMissions;

    /// <summary>UseAllMissions가 false일 때 이날 등장할 임무 수.</summary>
    public int MissionAmount;

    /// <summary>하루의 지속 시간(초). Phase C에서는 이 값이 하루 루프 종료 조건으로 사용된다.</summary>
    public int DayDurationInSeconds;

    /// <summary>이날의 전반적 난이도/분위기.</summary>
    public DayStatus Status;
}
