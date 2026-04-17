using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    HQ,          // 본부건물
    Normal,      // 기본 건물
}

public enum BuildingState
{
    Idle,        // 대기 중인 건물
    Incident,    // 사건 발생 건물
    Unselected,  // 선택 안 된 건물 (미션 만료)
    Selected,    // 선택된 건물 (미션 진행 중)
    Completed    // 사건 완료 건물
}

[System.Serializable]
public class SpotVisualConfig
{
    public BuildingType _buildingType;
    public BuildingState _buildingState;
    public Image _icon;
    public Color color = Color.white;
}

public class Spot : MonoBehaviour
{
    [SerializeField] BuildingType _spotType;

    public BuildingType SpotType => _spotType;


    public void SetType(BuildingType type)
    {
        _spotType = type;

    }


}
