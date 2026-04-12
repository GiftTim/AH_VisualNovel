using UnityEngine;
using TMPro;

namespace DIALOGUE
{
    /*
     * DialogueContainer
     * ─────────────────────────────────────────────────────────────────────
     * 대화창 UI 전체를 담당하는 컨테이너 클래스.
     * DialogueSystem이 소유하며, 대화창의 표시/숨김, 텍스트 스타일 변경을 담당한다.
     *
     * 구성 요소:
     *   root          : 대화 영역 전체 GameObject (CanvasGroup을 보유)
     *   nameContainer : 화자 이름 표시 UI 박스
     *   dialogueBox   : 대화 텍스트가 표시되는 배경 박스
     *   dialogueText  : 실제 대사 텍스트 (TextMeshProUGUI)
     * ─────────────────────────────────────────────────────────────────────
     */
    [System.Serializable]
    public class DialogueContainer
    {
        // 대화 영역 전체 루트 오브젝트 (CanvasGroup을 통해 페이드 in/out 제어)
        public GameObject root;

        // 화자 이름 표시 UI 박스
        public NameContainer nameContainer;

        // 대화 텍스트가 표시되는 배경 박스
        public GameObject dialogueBox;
        // 실제 대사 텍스트 컴포넌트
        public TextMeshProUGUI dialogueText;

        // 대화창 시각(페이드 인, 아웃) 컨트롤러
        // CanvasGroup의 alpha를 애니메이션하여 부드럽게 표시/숨김
        private CanvasGroupController cgController;

        // 화자 스타일 변경 메서드 (캐릭터 설정에 따라 대화 텍스트 스타일을 변경)
        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;

        private bool initialized = false;

        /*
         * Initialize
         * DialogueSystem 싱글톤이 생성된 후 호출해야 한다.
         * (CanvasGroupController 생성 시 DialogueSystem.instance를 참조하기 때문)
         * 이중 초기화를 방지하기 위해 initialized 플래그를 사용.
         */
        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            cgController = new CanvasGroupController(DialogueSystem.instance, root.GetComponent<CanvasGroup>());

        }

        // 대화창이 현재 보이는 상태인지 여부
        public bool isVisible => cgController.isVisible;

        // 대화창 전체를 서서히 표시 (speed: 애니메이션 속도, immediate: 즉시 표시 여부)
        public Coroutine Show(float speed = 1f, bool immediate = false) => cgController.Show(speed, immediate);
        // 대화창 전체를 서서히 숨김 (일반적으로 대화가 끝날 때 호출)
        public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);
    }
}
