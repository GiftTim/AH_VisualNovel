using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpotType
{
    HQ,          // 본부
    Idle,        // 사건 없는 건물
    Incident,    // 사건 발생 건물
    Unselected,  // 선택 안 된 건물 (미션 만료)
    Selected,    // 선택된 건물 (미션 진행 중)
    Completed    // 사건 완료 건물
}

[System.Serializable]
public class SpotVisualConfig
{
    public SpotType type;
    public Color color = Color.white;
    public Sprite sprite; // null이면 스프라이트 유지
}

public class Spot : MonoBehaviour
{
    [SerializeField] SpotType _spotType;
    [SerializeField] List<Spot> _neighbors;
    [SerializeField] Image _icon;
    [SerializeField] SpotVisualConfig[] _visualConfigs;

    public SpotType SpotType => _spotType;
    public List<Spot> Neighbors => _neighbors;

    void Start() => ApplyVisual();

    public void SetType(SpotType type)
    {
        _spotType = type;
        ApplyVisual();
    }

    void ApplyVisual()
    {
        foreach (var config in _visualConfigs)
        {
            if (config.type != _spotType) continue;
            _icon.color = config.color;
            if (config.sprite != null)
                _icon.sprite = config.sprite;
            return;
        }
    }
}
