using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue UI ���")]
    [SerializeField] GameObject DialogueObject;
    [SerializeField] GameObject DialogueNameObject;

    [SerializeField] TextMeshProUGUI txt_Dialogue;
    [SerializeField] TextMeshProUGUI txt_DialogueName;

    [Header("DialogueTextPrinter (��� ��±�)")]
    private DialogueTextPrinter printer;

    // Ÿ�� ���� ����
    private Coroutine typingCo;
    private string lastText;


    private void Awake()
    {
        if (DialogueObject == null || DialogueNameObject == null || txt_Dialogue == null || txt_DialogueName == null)
        {
            Debug.LogError("DialogueUI: �ʼ� UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        printer = new DialogueTextPrinter();
    }

    public void ShowDialogue()
    {
        // �����ϰ� ����
        StopTypingIfAny();  
        txt_Dialogue.text = "";
        txt_DialogueName.text = "";
        DialogueObject.SetActive(false);
        DialogueNameObject.SetActive(false);
        // ���̾Ʒα� Ȱ��ȭ
        DialogueObject.SetActive(true);
        DialogueNameObject.SetActive(true);
    }

    public void HideDialogue()
    {
        StopTypingIfAny(); // ��Ȱ��ȭ ���� �ڷ�ƾ �ߴ�
        DialogueObject.SetActive(false);
        DialogueNameObject.SetActive(false);
        txt_Dialogue.text = "";
        txt_DialogueName.text = "";
    }


    /// <summary>��� Ÿ�� ���. �Ϸ� �� onComplete ȣ��</summary>
    public void Print(string text, float delay, Action onComplete = null)
    {
        if (!isActiveAndEnabled || txt_Dialogue == null)
        {
            SetInstant(text);
            onComplete?.Invoke();
            return;
        }

        StopTypingIfAny();           // ���� ���� ���� Ÿ�ڸ� ����
        lastText = text ?? string.Empty;

        typingCo = StartCoroutine(PrintRoutine(lastText, delay, onComplete));
    }

    public void SetName(string name)
    {
        txt_DialogueName.text = name ?? "";
    }

    /// <summary>��� ��ü ǥ�� (Ÿ�� ȿ�� ����)</summary>
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

    /// <summary>���� Ÿ�� ���� ������</summary>
    public bool IsTyping => typingCo != null;

    /// <summary>Ÿ�� ��ŵ: ��� �ϼ� + onComplete ȣ����� ����</summary>
    public void SkipTyping()
    {
        if (!IsTyping) return;

        StopTypingIfAny();                              // �ڷ�ƾ �ߴ�
        printer.SetTextInstant(txt_Dialogue, lastText); // ��� �ϼ�
    }

    // ---- ���� ��ƿ ----

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