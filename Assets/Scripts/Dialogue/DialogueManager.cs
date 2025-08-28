using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("대화에서 참조하는 스크립트")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private CameraController cameraController;

    [Header("참조하는 다른 스크립트")]
    [SerializeField] private MouseManager mouseManager;
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private InteractionManager interactionManager;

    [Header("대화 속도 설정")]
    [SerializeField] private float defaultTextSpeed = 0.05f;

    private bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            DisplayNextContext();
        }
    }

    /// <summary>InteractionManager가 호출할 대사 시작 함수 (Excel Importer SO)</summary>
    public void StartDialogue()
    {

    }

    private void DisplayNextContext()
    {

    }


     private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.HideDialogue();

        interactionManager?.OnDialogueClosed();
    }

    //=== 예시용 ===
    public void DisplayDialogueLine(string line)
    {
        dialogueUI.Print(line, defaultTextSpeed);
    }
}