using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputPanel : MonoBehaviour
{
    public static InputPanel instance { get; private set; } = null;

    [Header("UI References")]
    [SerializeField] private CanvasGroup    canvasGroup;
    [SerializeField] private TMP_Text       titleText;
    [SerializeField] private Button         acceptButton;
    [SerializeField] private TMP_InputField inputField;

    private CanvasGroupController cg;

    public string lastInput { get; private set; } = string.Empty;
    public bool isWaitingOnUserInput { get; private set; }

    private void Awake()
    {
        instance = this;

        // 필수 참조 누락 방지
        if (canvasGroup == null) Debug.LogError($"{nameof(InputPanel)}: canvasGroup is not assigned.", this);
        if (titleText == null) Debug.LogError($"{nameof(InputPanel)}: titleText is not assigned.", this);
        if (acceptButton == null) Debug.LogError($"{nameof(InputPanel)}: acceptButton is not assigned.", this);
        if (inputField == null) Debug.LogError($"{nameof(InputPanel)}: inputField is not assigned.", this);
    }

    private void Start()
    {
        cg = new CanvasGroupController(this, canvasGroup);

        // 초기 상태
        cg.alpha = 0f;
        cg.SetInteractableState(active: false);
        inputField.text = string.Empty;
        acceptButton.interactable = false;
        isWaitingOnUserInput = false;

        // 리스너 등록
        inputField.onValueChanged.AddListener(OnInputChanged);
        inputField.onSubmit.AddListener(OnInputSubmitted); // 엔터 입력 지원 (TMP_InputField 설정에 따라 동작)
        acceptButton.onClick.AddListener(OnAcceptInput);
    }

    private void OnDestroy()
    {
        // 리스너 정리 (재생성/씬 전환 시 안전)
        inputField.onValueChanged.RemoveListener(OnInputChanged);
        inputField.onSubmit.RemoveListener(OnInputSubmitted);
        acceptButton.onClick.RemoveListener(OnAcceptInput);
    }

    public void Show(string title)
    {
        titleText.text = title;

        // 매번 열릴 때 상태 초기화
        inputField.text = string.Empty;
        acceptButton.interactable = false;

        cg.Show();
        cg.SetInteractableState(true);

        isWaitingOnUserInput = true;

        // 입력 포커스 (다음 프레임에서 안정적으로 적용)
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void Hide()
    {
        cg.Hide();
        cg.SetInteractableState(false);

        isWaitingOnUserInput = false;
    }

    public void OnAcceptInput()
    {
        if (!HasValidText())
            return;

        // 앞뒤 공백 제거해서 저장 (원문 유지가 필요하면 Trim 제거)
        lastInput = inputField.text.Trim();
        Hide();
    }

    private void OnInputSubmitted(string value)
    {
        // 엔터로 제출 가능하게 처리
        if (isWaitingOnUserInput && HasValidText())
        {
            OnAcceptInput();
        }
    }



    public void OnInputChanged(string value)
    {
        acceptButton.interactable = HasValidText();
    }

    private bool HasValidText()
    {
        return !string.IsNullOrWhiteSpace(inputField.text);
    }

    // 필요 시 외부에서 이전 입력값 초기화할 수 있도록 제공
    public void ClearLastInput()
    {
        lastInput = string.Empty;
    }
}