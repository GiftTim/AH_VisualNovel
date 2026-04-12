using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;

namespace DIALOGUE
{
    /*
     * DialogueSystem
     * ─────────────────────────────────────────────────────────────────────
     * 비주얼 노벨 대화 시스템 전체를 총괄하는 싱글톤 MonoBehaviour.
     * 씬에 하나만 존재하며, 다른 시스템들이 DialogueSystem.instance로 접근한다.
     *
     * 소유 및 관리 대상:
     *   - ConversationManager : 대화 루프 실행
     *   - TextArchitect       : 텍스트 타이핑 출력
     *   - AutoReader          : Auto/Skip 모드 제어
     *   - DialogueContainer   : 대화창 UI
     *   - cgController        : 전체 UI 표시/숨김 (mainCanvas 페이드)
     *
     * 이벤트:
     *   onUserPrompt_Next : 사용자가 클릭/입력할 때 발생 → ConversationManager가 구독
     *   onClear           : 텍스트 초기화 신호 발생 → 관련 UI 구독 가능
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DialogueSystem : MonoBehaviour
    {
        // DialogueSystemConfigurationSO: 기본 폰트 크기, 스케일 등 전역 설정값을 담는 ScriptableObject
        [SerializeField] private DialogueSystemConfigurationSO _config;

        // 외부에서 읽기 전용으로 접근 가능한 설정 프로퍼티
        public DialogueSystemConfigurationSO config => _config;

        // 대화창 UI 컨테이너 (이름 박스, 대화 텍스트 포함)
        public DialogueContainer dialogueContainer = new DialogueContainer();
        // 대화 루프를 관리하는 매니저
        public ConversationManager conversationManager { get; private set; }
        // 텍스트 타이핑 출력 담당 (ConversationManager에 주입)
        private TextArchitect architect;
        // Auto/Skip 모드 제어 컴포넌트
        public AutoReader autoReader{ get; private set; }

        // 싱글톤 인스턴스
        public static DialogueSystem instance { get; private set; }

        // 전체 대화 UI를 감싸는 CanvasGroup (페이드 in/out으로 전체 UI 표시/숨김)
        [SerializeField] private CanvasGroup mainCanvas;

        // 사용자 입력(클릭) 이벤트 타입 선언
        public delegate void DialogueSystemEvent();
        // 사용자가 직접 클릭/입력해 다음으로 진행할 때 발생
        public event DialogueSystemEvent onUserPrompt_Next;
        // 텍스트 초기화 신호 발생 시 이벤트 (onClear 구독자에게 알림)
        public event DialogueSystemEvent onClear;

        // 외부에서 대화 진행 여부 확인용 (conversationManager.isRunning의 단축 접근)
        public bool isRunningConversation => conversationManager.isRunning;

        // ▶ 계속 아이콘 컴포넌트
        public DialogueContinuePrompt prompt;

        // mainCanvas 전체 UI 페이드 컨트롤러
        private CanvasGroupController cgController;


        private void Awake()
        {
            // 싱글톤 패턴: 처음 생성된 인스턴스만 유지, 중복 생성 시 즉시 파괴
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        /*
         * _initialized 플래그
         * Initialize()가 두 번 이상 호출되는 것을 방지한다.
         * (Awake가 드물게 중복 호출되는 엣지 케이스 대비)
         */
        private bool _initialized = false;

        /*
         * Initialize
         * 모든 하위 시스템을 순서대로 초기화한다.
         * 순서가 중요:
         *   1) TextArchitect 생성 (dialogueContainer.dialogueText 필요)
         *   2) ConversationManager 생성 (architect 필요)
         *   3) cgController 생성 (mainCanvas 필요)
         *   4) dialogueContainer.Initialize() (DialogueSystem.instance 필요 → Awake 이후)
         *   5) AutoReader 초기화 (conversationManager 필요)
         */
        private void Initialize()
        {
            if (_initialized)
                return;

            architect = new TextArchitect(dialogueContainer.dialogueText, TABuilder.BuilderTypes.Typewriter);
            conversationManager = new ConversationManager(architect);

            cgController = new CanvasGroupController(this, mainCanvas);
            dialogueContainer.Initialize();

            autoReader = GetComponent<AutoReader>();
            if(autoReader != null)
            {
                autoReader.Initialize(conversationManager);
            }
        }

        /*
         * OnUserPrompt_Next
         * 사용자가 직접 클릭/입력해 다음으로 진행할 때 호출된다.
         * onUserPrompt_Next 이벤트를 발생시키고,
         * autoReader가 실행 중이면 자동으로 Disable()한다.
         * (사용자가 직접 개입했으므로 Auto 모드를 해제)
         */
        public void OnUserPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();

            if(autoReader != null && autoReader.isOn)
            {
                autoReader.Disable();
            }
        }

        /*
         * OnSystemPrompt_Next
         * 시스템 내부에서 자동으로 다음 줄로 진행할 때 호출된다.
         * (Auto/Skip 모드에서 사용. autoReader를 Disable하지 않음)
         */
        public void OnSystemPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        /*
         * OnSystemPrompt_Clear
         * 텍스트 초기화 신호. onClear 이벤트 구독자에게 알린다.
         * (예: 히스토리 로그 시스템 등이 구독해 clear 시점을 인식)
         */
        public void OnSystemPrompt_Clear()
        {
            onClear?.Invoke();
        }

        /*
         * OnStartViewingHistory
         * 히스토리 보기 시작 시 호출. 다음 항목들을 차단한다:
         *   - prompt 숨김 (▶ 아이콘이 히스토리 위에 보이지 않도록)
         *   - autoReader.allowToggle = false (Auto/Skip 전환 잠금)
         *   - conversationManager.allowUserPrompts = false (클릭 입력 차단)
         *   - autoReader 실행 중이면 Disable
         */
        public void OnStartViewingHistory()
        {
            prompt.Hide();
            autoReader.allowToggle = false;
            conversationManager.allowUserPrompts = false;

            if (autoReader.isOn)
            {
                autoReader.Disable();
            }
        }

        /*
         * OnStopViewingHistory
         * 히스토리 보기 종료 시 호출. 차단했던 항목들을 복원한다.
         */
        public void OnStopViewingHistory()
        {
            prompt.Show();
            autoReader.allowToggle = true;
            conversationManager.allowUserPrompts = true;
        }

        /*
         * ApplySpeakerDataToDialogueContainer (이름으로 조회)
         * 캐릭터 이름으로 CharacterConfigData를 가져와 대화창 스타일에 반영.
         * 캐릭터가 없으면 CharacterManager의 기본 설정을 사용.
         */
        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            Character character = CharacterManager.instance.GetCharacter(speakerName);
            CharacterConfigData config = character != null ? character.config : CharacterManager.instance.GetCharacterConfig(speakerName);

            ApplySpeakerDataToDialogueContainer(config);
        }

        /*
         * ApplySpeakerDataToDialogueContainer (설정 데이터로 직접 적용)
         * CharacterConfigData의 색상, 폰트, 크기를 대화창과 이름 박스에 반영.
         * 폰트 크기는 전역 설정(_config)과 캐릭터 설정(config)의 스케일을 곱해 계산.
         */
        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData config)
        {
            //Set Dialogue details
            dialogueContainer.SetDialogueColor(config.dialogueColor);
            dialogueContainer.SetDialogueFont(config.dialogueFont);
            float fontSize = this.config.defaultDialogueFontSize * this.config.dialogueFontScale * config.dialogueFontScale;
            dialogueContainer.SetDialogueFontSize(fontSize);

            //Set name details
            dialogueContainer.nameContainer.SetNameColor(config.nameColor);
            dialogueContainer.nameContainer.SetNameFont(config.nameFont);
            fontSize = this.config.defaultNameFontSize * config.nameFontScale;
            dialogueContainer.nameContainer.SetNameFontSize(fontSize);
        }

        /*
         * ShowSpeakerName
         * 화자 이름을 대화창 이름 박스에 표시한다.
         * speakerName이 "narrator"이면 이름 박스를 숨기고 텍스트를 초기화한다.
         * (나레이터는 화자 이름을 표시하지 않는 관례)
         */
        public void ShowSpeakerName(string speakerName = "")
        {
            if(speakerName.ToLower() != "narrator")
            {
                dialogueContainer.nameContainer.Show(speakerName);
            }
            else
            {
                HideSpeakerName();
                dialogueContainer.nameContainer.nameText.text = "";
            }
        }

        // 이름 박스 숨기기
        public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

        /*
         * Say 오버로드들
         * 대화를 시작하는 세 가지 진입점:
         *   1) Say(string speaker, string dialogue) : 화자와 대사를 직접 지정
         *   2) Say(List<string> lines, string filePath) : 여러 줄을 리스트로 전달
         *   3) Say(Conversation conversation) : Conversation 객체를 직접 전달
         */
        public Coroutine Say(string speaker, string dialogue)
        {
            List<string>conversation = new List<string> { $"{speaker}\"{dialogue}\"" };
            return Say(conversation);
        }

        public Coroutine Say(List<string> lines, string filePath = "")
        {
            Conversation conversation = new Conversation(lines, file: filePath);
            return conversationManager.StartConversation(conversation);
        }

        public Coroutine Say(Conversation conversation)
        {
            return conversationManager.StartConversation(conversation);
        }

        // 전체 대화 UI(mainCanvas)의 표시 여부
        public bool isVisible => cgController.isVisible;
        // 전체 대화 UI를 서서히 표시 (대화 씬 시작 시 등)
        public Coroutine Show(float speed = 1f, bool immediate = false) => cgController.Show(speed, immediate);
        // 전체 대화 UI를 서서히 숨김 (대화 씬 종료 시 등)
        public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);

    }
}
