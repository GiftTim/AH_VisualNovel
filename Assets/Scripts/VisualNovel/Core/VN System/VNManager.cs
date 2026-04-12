using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace VISUALNOVEL
{
    /*
     * ============================================================
     * VNManager (Visual Novel Manager)
     * ============================================================
     * 비주얼 노벨 시스템 전체의 진입점(Entry Point) 클래스.
     *
     * [역할]
     * - 씬이 시작될 때 가장 먼저 실행되어 모든 시스템을 초기화한다.
     * - 싱글톤(Singleton) 패턴으로 구현되어 있어,
     *   씬 어디에서든 VNManager.instance 로 접근할 수 있다.
     * - 게임 시작 시 "새 게임"인지 "이어하기(로드)"인지 판단하여
     *   적절한 대화 흐름을 시작한다.
     *
     * [씬 내 위치]
     * - 씬의 루트 오브젝트에 배치되어야 하며,
     *   동일 오브젝트에 VNDatabaseLinkSetup 컴포넌트도 함께 붙어 있어야 한다.
     *
     * [초기화 순서]
     *   Awake: 싱글톤 보호 → DB 링크 설정 → 세이브 파일 초기화
     *   Start: 게임 로드 (새 게임 or 이어하기)
     * ============================================================
     */
    public class VNManager : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────
        // 싱글톤 인스턴스
        // 씬 어디에서든 VNManager.instance 로 접근 가능.
        // private set: 외부에서 덮어쓰는 것을 방지.
        // ─────────────────────────────────────────────────────────
        public static VNManager instance { get; private set; }

        // ─────────────────────────────────────────────────────────
        // [SerializeField] config : VisualNovelSO (ScriptableObject)
        // 인스펙터에서 할당하는 게임 설정 파일.
        // 새 게임 시작 시 어떤 대화 파일을 처음 재생할지
        // (startingFile) 정보가 여기에 담겨 있다.
        // ─────────────────────────────────────────────────────────
        [SerializeField] private VisualNovelSO config;

        // ─────────────────────────────────────────────────────────
        // mainCamera : 세이브 시 스크린샷을 캡처할 때 사용하는 카메라.
        // VNGameSave.Save() 내부의 ScreenShotFunction이 이 카메라를 참조한다.
        // ─────────────────────────────────────────────────────────
        public Camera mainCamera;

        /*
         * Awake()
         * ────────────────────────────────────────────────────────
         * Unity 생명주기에서 Start()보다 먼저 호출된다.
         * 다른 오브젝트의 Start()가 실행되기 전에 반드시 완료되어야 하는
         * 초기화 작업을 여기서 처리한다.
         *
         * 처리 순서:
         *  1) 싱글톤 중복 방지
         *     - 이미 instance가 존재하면 이 게임오브젝트는 즉시 파괴.
         *     - 씬 전환 등으로 VNManager가 두 개 생기는 것을 막는다.
         *
         *  2) VNDatabaseLinkSetup.SetupExtrnalLinks() 호출
         *     - VariableStore에 플레이어 이름 등 외부 데이터와
         *       연결된 특수 변수를 등록한다.
         *     - 이 과정이 끝나야 대화 스크립트에서 $VN.mainCharacterName
         *       같은 변수를 사용할 수 있다.
         *
         *  3) VNGameSave.activeFile 초기화
         *     - activeFile이 null이면 새 VNGameSave 인스턴스를 생성.
         *     - null이 아닌 경우(메뉴 씬에서 이미 로드된 경우 등)는
         *       기존 값을 유지한다.
         * ────────────────────────────────────────────────────────
         */
        private void Awake()
        {
            // ── 싱글톤 보호: 이미 인스턴스가 존재하면 이 오브젝트 제거 ──
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                DestroyImmediate(gameObject); // 중복 인스턴스를 즉시 파괴
                return;
            }

            // ── DB 링크 설정: 변수 ↔ 실제 데이터 연결 ──
            // 같은 게임오브젝트에 붙어 있는 VNDatabaseLinkSetup 컴포넌트를 가져온다.
            VNDatabaseLinkSetup linkSetup = GetComponent<VNDatabaseLinkSetup>();
            linkSetup.SetupExtrnalLinks();

            // ── 세이브 파일 초기화 ──
            // activeFile이 null인 경우에만 새로 생성 (이미 존재하면 유지).
            if (VNGameSave.activeFile == null)
            {
                VNGameSave.activeFile = new VNGameSave();
            }
        }

        /*
         * Start()
         * ────────────────────────────────────────────────────────
         * Awake() 이후, 첫 프레임 렌더링 전에 호출된다.
         * Awake에서 초기화가 완전히 끝난 뒤 게임을 시작해야 하므로
         * Start에서 LoadGame()을 호출한다.
         * ────────────────────────────────────────────────────────
         */
        private void Start()
        {
            LoadGame();
        }

        /*
         * LoadGame()
         * ────────────────────────────────────────────────────────
         * 새 게임인지, 저장 파일에서 이어하기인지 판단하여
         * 적절한 대화 흐름을 시작한다.
         *
         * [새 게임인 경우] VNGameSave.activeFile.newGame == true
         *  - VisualNovelSO(config)에 지정된 startingFile(TextAsset)을 읽어
         *    첫 대화 Conversation 을 생성하고 DialogueSystem에 넘긴다.
         *
         * [이어하기인 경우] VNGameSave.activeFile.newGame == false
         *  - VNGameSave.Activate()를 호출하여 저장된 게임 상태를 복원한다.
         *    (대화 큐, 변수, 히스토리, 화면 상태 등이 복원됨)
         * ────────────────────────────────────────────────────────
         */
        private void LoadGame()
        {
            if (VNGameSave.activeFile.newGame)
            {
                // 새 게임: startingFile에 지정된 텍스트 파일을 줄 단위로 읽는다.
                List<string> lines = FileManager.ReadTextAsset(config.startingFile);

                // 읽어온 줄 목록으로 Conversation(대화 시퀀스) 객체를 생성.
                Conversation start = new Conversation(lines);

                // DialogueSystem에 첫 대화를 전달하여 재생 시작.
                DialogueSystem.instance.Say(start);
            }
            else
            {
                // 이어하기: 저장된 게임 상태 전체를 복원한다.
                VNGameSave.activeFile.Activate();
            }
        }
    }
}
