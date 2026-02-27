using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private TMP_InputField inputField;

    private CanvasGroupController cg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cg= new CanvasGroupController(this, canvasGroup);

        canvasGroup.alpha = 0;
    }

    public void Show()
    {

    }

    public void Hide()
    {

    }

    private void SetCanvasState(bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

}
