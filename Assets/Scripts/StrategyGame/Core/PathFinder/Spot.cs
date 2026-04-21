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
}

public class Spot : MonoBehaviour
{
    [SerializeField] BuildingType _spotType;
    [SerializeField] BuildingState _buildingState = BuildingState.Idle;

    [SerializeField] private Image _buildingImage;
    [SerializeField] private float _fadeInDuration = 1.0f;

    [Header("버튼 UI")]
    [SerializeField] private GameObject _alarmButton;
    [SerializeField] private GameObject _workButton;
    [SerializeField] private GameObject _checkButton;

    private System.Action _onReturnCallback;

    public BuildingType SpotType => _spotType;
    public BuildingState CurrentState => _buildingState;

    private void Awake()
    {
        if (_spotType == BuildingType.HQ) return;

        if (_buildingImage != null)
        {
            Color c = _buildingImage.color;
            c.a = 0f;
            _buildingImage.color = c;
        }

        SetButtonsAll(false);
    }

    public void SetType(BuildingType type)
    {
        _spotType = type;
    }

    public void SetState(BuildingState state)
    {
        _buildingState = state;

        if (_spotType == BuildingType.HQ) return;

        switch (state)
        {
            case BuildingState.Idle:
                SetButtonsAll(false);
                if (_buildingImage != null)
                {
                    _buildingImage.DOKill();
                    _buildingImage.DOFade(0f, _fadeInDuration);
                }
                break;

            case BuildingState.Incident:
                SetButtonsAll(false);
                PlayIncidentEffect();
                break;

            case BuildingState.Selected:
                // 버튼은 OnDispatchArrived() 도착 시 제어
                break;

            case BuildingState.Completed:
                SetButtonsAll(false);
                _checkButton?.SetActive(true);
                _onReturnCallback?.Invoke();
                _onReturnCallback = null;
                break;
        }
    }

    private void SetButtonsAll(bool active)
    {
        _alarmButton?.SetActive(active);
        _workButton?.SetActive(active);
        _checkButton?.SetActive(active);
    }

    private void PlayIncidentEffect()
    {
        if (_buildingImage == null) return;

        _buildingImage.DOKill();

        Color c = _buildingImage.color;
        c.a = 0f;
        _buildingImage.color = c;

        _buildingImage.DOFade(1f, _fadeInDuration)
            .OnComplete(() =>
            {
                _alarmButton?.SetActive(true);
                SliderCountdown countdown = _alarmButton?.GetComponent<SliderCountdown>();
                countdown?.StartCountdown();
            });
    }

    public void OnDispatchArrived(System.Action onReturn)
    {
        _onReturnCallback = onReturn;

        SetButtonsAll(false);
        _workButton?.SetActive(true);

        SliderCountdown countdown = _workButton.GetComponent<SliderCountdown>();
        if (countdown != null)
        {
            countdown.OnCountdownCompleted += HandleWorkComplete;
            countdown.StartCountdown();
        }
    }

    private void HandleWorkComplete()
    {
        SliderCountdown countdown = _workButton?.GetComponent<SliderCountdown>();
        if (countdown != null)
            countdown.OnCountdownCompleted -= HandleWorkComplete;

        SetState(BuildingState.Completed);
    }
}
