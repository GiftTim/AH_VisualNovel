using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue UI 요소")]
    [SerializeField] GameObject DialogueObject;
    [SerializeField] GameObject DialogueNameObject;

    [SerializeField] TextMeshProUGUI txt_Dialogue;
    [SerializeField] TextMeshProUGUI txt_DialogueName;

    [Header("DialogueTextPrinter (대사 출력기)")]
    private DialogueTextPrinter printer;

    // 타자 진행 관리
    private Coroutine typingCo;
    private string lastText;


    private void Awake()
    {
        if (DialogueObject == null || DialogueNameObject == null || txt_Dialogue == null || txt_DialogueName == null)
        {
            Debug.LogError("DialogueUI: 필수 UI 요소가 할당되지 않았습니다.");
            return;
        }

        printer = new DialogueTextPrinter();
    }

    public void ShowDialogue()
    {
        // 안전하게 정리
        StopTypingIfAny();  
        txt_Dialogue.text = "";
        txt_DialogueName.text = "";
        DialogueObject.SetActive(false);
        DialogueNameObject.SetActive(false);
        // 다이아로그 활성화
        DialogueObject.SetActive(true);
        DialogueNameObject.SetActive(true);
    }

    public void HideDialogue()
    {
        StopTypingIfAny(); // 비활성화 전에 코루틴 중단
        DialogueObject.SetActive(false);
        DialogueNameObject.SetActive(false);
        txt_Dialogue.text = "";
        txt_DialogueName.text = "";
    }


    /// <summary>대사 타자 출력. 완료 시 onComplete 호출</summary>
    public void Print(string text, float delay, Action onComplete = null)
    {
        if (!isActiveAndEnabled || txt_Dialogue == null)
        {
            SetInstant(text);
            onComplete?.Invoke();
            return;
        }

        StopTypingIfAny();           // 기존 진행 중인 타자를 정리
        lastText = text ?? string.Empty;

        typingCo = StartCoroutine(PrintRoutine(lastText, delay, onComplete));
    }

    public void SetName(string name)
    {
        txt_DialogueName.text = name ?? "";
    }

    /// <summary>즉시 전체 표시 (타자 효과 없이)</summary>
    public void SetInstant(string text)
    {
        StopTypingIfAny();
        printer.SetTextInstant(txt_Dialogue, text ?? string.Empty);
    }

    public void Clear()
    {
        StopTypingIfAny();
        printer.Clear(txt_Dialogue);
        txt_DialogueName.text = "";
    }

    /// <summary>현재 타자 진행 중인지</summary>
    public bool IsTyping => typingCo != null;

    /// <summary>타자 스킵: 즉시 완성 + onComplete 호출까지 보장</summary>
    public void SkipTyping()
    {
        if (!IsTyping) return;

        StopTypingIfAny();                              // 코루틴 중단
        printer.SetTextInstant(txt_Dialogue, lastText); // 즉시 완성
    }

    // ---- 내부 유틸 ----

    private IEnumerator PrintRoutine(string text, float delay, Action onComplete)
    {
        yield return printer.PrintText(txt_Dialogue, text, delay, null);

        typingCo = null;

    }

    private void StopTypingIfAny()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }
    }

    
}