using DG.Tweening;
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
    public Color color = Color.white;
}

public class Spot : MonoBehaviour
{
    [SerializeField] BuildingType _spotType;
    [SerializeField] BuildingState _buildingState = BuildingState.Idle;

    [SerializeField] private Image _buildingImage;
    [SerializeField] private float _fadeInDuration = 1.0f;

    public BuildingType SpotType => _spotType;
    public BuildingState CurrentState => _buildingState;

    private void Awake()
    {
        if (_buildingImage != null)
        {
            Color c = _buildingImage.color;
            c.a = 0f;
            _buildingImage.color = c;
        }
    }

    public void SetType(BuildingType type)
    {
        _spotType = type;
    }

    public void SetState(BuildingState state)
    {
        BuildingState prev = _buildingState;
        _buildingState = state;

        // * → Incident: 페이드인
        if (prev != BuildingState.Incident && state == BuildingState.Incident)
        {
            PlayIncidentEffect();
        }
        // * → Idle: alpha 페이드아웃 (경로 무관)
        else if (state == BuildingState.Idle)
        {
            if (_buildingImage != null)
            {
                _buildingImage.DOKill();
                _buildingImage.DOFade(0f, _fadeInDuration);
            }
        }
        // Incident → Selected: 유지 (처리 없음)
    }

    private void PlayIncidentEffect()
    {
        if (_buildingImage == null) return;

        _buildingImage.DOKill();

        Color c = _buildingImage.color;
        c.a = 0f;
        _buildingImage.color = c;

        _buildingImage.DOFade(1f, _fadeInDuration);
    }
}
