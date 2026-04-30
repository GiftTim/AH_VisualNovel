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
    [SerializeField] private float _buttonFadeDuration = 0.4f;

    private CanvasGroup _alarmCanvasGroup;
    private CanvasGroup _workCanvasGroup;
    private CanvasGroup _checkCanvasGroup;
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

        _alarmCanvasGroup = InitCanvasGroup(_alarmButton);
        _workCanvasGroup  = InitCanvasGroup(_workButton);
        _checkCanvasGroup = InitCanvasGroup(_checkButton);

        SetButtonsAll(false);
    }

    private CanvasGroup InitCanvasGroup(GameObject btn)
    {
        if (btn == null) return null;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        btn.SetActive(false);
        return cg;
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
                ShowCheckButton();
                _onReturnCallback?.Invoke();
                _onReturnCallback = null;
                break;
        }
    }

    private void SetButtonsAll(bool active)
    {
        if (active)
        {
            ShowAlarmButton();
            ShowWorkButton();
            ShowCheckButton();
        }
        else
        {
            HideAlarmButton();
            HideWorkButton();
            HideCheckButton();
        }
    }

    private void ShowButton(GameObject btn, CanvasGroup cg)
    {
        if (btn == null || cg == null) return;
        btn.SetActive(true);
        cg.DOKill();
        cg.alpha = 0f;
        cg.DOFade(1f, _buttonFadeDuration);
    }

    private void HideButton(GameObject btn, CanvasGroup cg)
    {
        if (btn == null || cg == null) return;
        cg.DOKill();
        cg.DOFade(0f, _buttonFadeDuration)
            .OnComplete(() => btn.SetActive(false));
    }

    private void ShowAlarmButton() => ShowButton(_alarmButton, _alarmCanvasGroup);
    private void HideAlarmButton() => HideButton(_alarmButton, _alarmCanvasGroup);
    private void ShowWorkButton()  => ShowButton(_workButton,  _workCanvasGroup);
    private void HideWorkButton()  => HideButton(_workButton,  _workCanvasGroup);
    private void ShowCheckButton() => ShowButton(_checkButton, _checkCanvasGroup);
    private void HideCheckButton() => HideButton(_checkButton, _checkCanvasGroup);

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
                ShowAlarmButton();
                SliderCountdown countdown = _alarmButton?.GetComponent<SliderCountdown>();
                countdown?.StartCountdown();
            });
    }

    public void OnDispatchArrived(System.Action onReturn)
    {
        _onReturnCallback = onReturn;

        SetButtonsAll(false);
        ShowWorkButton();

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
