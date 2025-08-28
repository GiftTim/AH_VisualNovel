using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Interaction에 관련된 스크립트")]
    [SerializeField] private InteractionDetector interactionDetector;
    [SerializeField] private InteractionUI interactionUI;

    [Header("참조하는 스크립트")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private MouseManager mouseManager;

    [Header("입력/진행 설정")]
    [SerializeField, Min(0.05f)] private float holdSeconds = 1.0f; // 홀드 완료까지 걸리는 시간(초)
    [SerializeField] private string tagNPC = "Interaction_NPC";
    [SerializeField] private string tagItem = "Interaction_Item";

    // 내부 상태(입력/흐름)
    private bool indialogueActive = false;
    private bool holding;
    private float holdElapsed;
    private string activeTag = "";

    private void Awake()
    {
        if (!dialogueManager) dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Update()
    {
        bool isInteractiveHit = interactionDetector.IsHit &&
                        (interactionDetector.CurrentTag == tagNPC || interactionDetector.CurrentTag == tagItem);

        if (indialogueActive == false)
        {
            if (isInteractiveHit) // NPC/Item에 Hit일 때만
                interactionUI.ShowInteractionCursor(interactionDetector.HitInfo, interactionDetector.CurrentTag);
            else
                interactionUI.ShowNomalCursor(); // 그 외엔 항상 노멀 커서

            // 펄스는 오직 NPC/Item 히트일 때만
            interactionUI.SetPulse(isInteractiveHit);


            // 진행 중인데 태그 바뀌면 취소
            if (holding && interactionDetector.CurrentTag != activeTag)
            {
                CancelHold();
            }

            // 입력 처리 (좌클릭 홀드 → 진행/완료/취소)
            bool canInteract = interactionDetector.IsHit &&
                (interactionDetector.CurrentTag == tagNPC || interactionDetector.CurrentTag == tagItem);

            if (!holding)
            {
                if (canInteract && Input.GetMouseButtonDown(0))
                {
                    StartHold(interactionDetector.CurrentTag);
                }
            }
            else
            {
                if (canInteract && Input.GetMouseButton(0))
                {
                    TickHold();
                }
                else
                {
                    CancelHold();
                }
            }
        }

    }

    private void StartHold(string tag)
    {
        holding = true;
        holdElapsed = 0f;
        activeTag = tag;

        interactionUI.ShowGauge();
        interactionUI.SetGauge01(0f);
    }

    private void TickHold()
    {
        holdElapsed += Time.deltaTime;
        interactionUI.SetGauge01(holdElapsed / holdSeconds);

        if (holdElapsed >= holdSeconds)
        {
            FinishHoldAndExecute();
        }
    }

    private void CancelHold()
    {
        holding = false;
        holdElapsed = 0f;
        activeTag = "";
        interactionUI.HideGauge();
    }

    private void FinishHoldAndExecute()
    {
        indialogueActive = true;
        holding = false;
        interactionUI.HideGauge();

        switch (activeTag)
        {
            case var t when t == tagNPC:

                if (dialogueManager && interactionDetector.HitInfo.transform.TryGetComponent(out InteractionEvent ev))
                {

                }
                else
                {
                    Debug.Log("NPC 상호작용: DialogueManager 또는 InteractionEvent 없음");
                    interactionUI.ShowAllCursors();
                }
                break;

            case var t when t == tagItem:
                break;
        }

        activeTag = "";
    }

    /// <summary>
    /// DialogueManager에서 대화 종료 시 호출
    /// </summary>
    public void OnDialogueClosed()
    {
        indialogueActive = false;

        mouseManager.EnableMovement();
        interactionUI.ShowAllCursors();
    }
}