using System;
using System.Collections.Generic;
using System.Linq;
using DIALOGUE;
using History;
using UnityEngine;

namespace VISUALNOVEL
{
    /*
     * ============================================================
     * VNGameSave (Visual Novel Game Save)
     * ============================================================
     * 비주얼 노벨의 게임 진행 상태를 저장하고 불러오는 클래스.
     *
     * [저장 데이터]
     *  - 플레이어 이름, 슬롯 번호
     *  - 현재 대화 큐(ConversationQueue) 상태
     *  - 화면/캐릭터/오디오 상태 스냅샷 (HistoryState)
     *  - 히스토리 로그 (되감기용 스냅샷 배열)
     *  - 전역 변수 목록 (VariableStore)
     *  - 저장 시각
     *
     * [저장 형식]
     *  - JSON 직렬화 → 텍스트 파일 (.vns)
     *  - 선택적 XOR 암호화 (ENCRYPT 상수로 제어)
     *  - 스크린샷 별도 저장 (.jpg)
     *
     * [싱글톤처럼 사용되는 activeFile]
     *  - static 필드 activeFile에 현재 플레이 중인 세이브를 보관.
     *  - 씬 어디서든 VNGameSave.activeFile 로 접근 가능.
     *
     * [핵심 흐름]
     *  저장: Save() → 상태 캡처 → JSON 직렬화 → 파일 쓰기
     *  불러오기: Load() → 파일 읽기 → JSON 역직렬화 → Activate()로 상태 복원
     * ============================================================
     */
    [System.Serializable]
    public class VNGameSave
    {
        // ─────────────────────────────────────────────────────────
        // 현재 활성 세이브 파일.
        // 씬 전환 등에 관계없이 전역에서 접근할 수 있도록 static으로 선언.
        // VNManager.Awake()에서 null이면 새 인스턴스로 초기화된다.
        // ─────────────────────────────────────────────────────────
        public static VNGameSave activeFile = null;

        // ─────────────────────────────────────────────────────────
        // ENCRYPT: 세이브 파일을 XOR 암호화할지 여부.
        //          true이면 일반 텍스트 에디터로 세이브 파일을 수정하기 어렵다.
        //          false이면 JSON 그대로 저장되어 디버깅이 편하다.
        // FILE_TYPE: 세이브 파일 확장자. (.vns = Visual Novel Save)
        // SCREENSHOT_FILE_TYPE: 세이브 슬롯에 표시될 스크린샷 확장자.
        // SCREENSHOT_DOWNSCALE_AMOUNT: 스크린샷을 원본 해상도의 25%로 축소.
        //   (저장 용량 절약. 슬롯 UI용 썸네일이므로 작아도 충분하다.)
        // ─────────────────────────────────────────────────────────
        public const bool ENCRYPT = false;
        public const string FILE_TYPE = ".vns";
        public const string SCREENSHOT_FILE_TYPE = ".jpg";
        public const float SCREENSHOT_DOWNSCALE_AMOUNT = 0.25f;

        // ─────────────────────────────────────────────────────────
        // filePath: 이 세이브 슬롯의 파일 저장 경로.
        //           예) "GameSaves/1.vns"
        // screenshotPath: 스크린샷 저장 경로.
        //                 예) "GameSaves/1.jpg"
        // slotNumber를 파일명으로 사용하므로, 슬롯마다 다른 파일이 생성된다.
        // ─────────────────────────────────────────────────────────
        public string filePath => $"{FilePaths.gameSaves}{slotNumber}{FILE_TYPE}";
        public string screenshotPath => $"{FilePaths.gameSaves}{slotNumber}{SCREENSHOT_FILE_TYPE}";

        // ─────────────────────────────────────────────────────────
        // playerName: 플레이어(주인공)가 입력한 이름.
        //             VNDatabaseLinkSetup을 통해 $VN.mainCharacterName 변수와 연결됨.
        // slotNumber: 세이브 슬롯 번호. 1부터 시작. 파일명으로도 사용됨.
        // ─────────────────────────────────────────────────────────
        public string playerName;
        public int slotNumber = 1;

        // ─────────────────────────────────────────────────────────
        // newGame: 아직 한 번도 저장하지 않은 새 게임인지 여부.
        //          true  → VNManager.LoadGame()에서 startingFile부터 시작.
        //          false → VNManager.LoadGame()에서 Activate()로 상태 복원.
        //          Save() 호출 시 자동으로 false로 변경된다.
        //
        // activeConversations: 저장 시점의 대화 큐 직렬화 데이터.
        //          각 원소는 VN_ConversationData 또는
        //          VN_ConversationDataCompressed를 JSON화한 문자열.
        //
        // activeState: 저장 시점의 화면 상태 스냅샷 (HistoryState).
        //          캐릭터 위치/표정, 배경, 오디오 재생 상태 등이 포함됨.
        //
        // historyLog: 되감기(히스토리)용 상태 스냅샷 배열.
        //          HistoryManager가 관리하는 이력을 통째로 저장.
        //
        // variables: 저장 시점의 전역 변수 목록.
        //          VariableStore에 등록된 모든 변수를 직렬화한 배열.
        //
        // timeStamp: 저장한 날짜/시간 문자열. 세이브 슬롯 UI에 표시할 때 사용.
        // ─────────────────────────────────────────────────────────
        public bool newGame = true;
        public string[] activeConversations;
        public HistoryState activeState;
        public HistoryState[] historyLog;
        public VN_VariableData[] variables;

        public string timeStamp;

        /*
         * Load(filePath, activateOnLoad)  [정적 메서드]
         * ────────────────────────────────────────────────────────
         * 지정한 파일 경로에서 세이브 데이터를 읽어 반환한다.
         * 동시에 activeFile을 읽어온 세이브 데이터로 교체한다.
         *
         * [매개변수]
         *  filePath       : 읽어올 세이브 파일 경로 (.vns)
         *  activateOnLoad : true이면 로드 직후 Activate()까지 자동 호출.
         *                   false이면 호출자가 수동으로 Activate()를 호출해야 함.
         *                   (메뉴에서 슬롯 정보만 미리 읽을 때는 false 사용)
         *
         * [반환값]
         *  로드된 VNGameSave 인스턴스.
         * ────────────────────────────────────────────────────────
         */
        public static VNGameSave Load(string filePath, bool activateOnLoad = false)
        {
            // FileManager를 통해 파일을 읽고 JSON → VNGameSave 역직렬화.
            // ENCRYPT 상수에 따라 복호화 여부가 결정된다.
            VNGameSave save = FileManager.Load<VNGameSave>(filePath, ENCRYPT);

            // 로드한 세이브를 전역 activeFile로 등록.
            activeFile = save;

            // 즉시 게임 상태를 복원해야 하는 경우 Activate() 호출.
            if (activateOnLoad)
            {
                save.Activate();
            }

            return save;
        }

        /*
         * Save()
         * ────────────────────────────────────────────────────────
         * 현재 게임 상태 전체를 파일에 저장한다.
         *
         * 처리 순서:
         *  1) newGame = false  → 이제 이어하기가 가능하다는 표시
         *  2) 화면/캐릭터/오디오 상태 캡처 (HistoryState.Capture)
         *  3) 히스토리 로그 배열로 변환
         *  4) 대화 큐(ConversationQueue) 직렬화
         *  5) 전역 변수 직렬화
         *  6) 저장 시각 기록
         *  7) 스크린샷 캡처 (세이브 슬롯 썸네일용)
         *  8) JSON 직렬화 후 파일 쓰기
         * ────────────────────────────────────────────────────────
         */
        public void Save()
        {
            // 한 번이라도 저장했으므로 더 이상 새 게임이 아님.
            newGame = false;

            // 현재 화면/캐릭터/오디오 상태를 스냅샷으로 캡처.
            activeState = HistoryState.Capture();

            // HistoryManager에 쌓인 되감기 이력을 배열로 복사.
            historyLog = HistoryManager.instance.history.ToArray();

            // 대화 큐에 남은 대화들을 직렬화.
            activeConversations = GetConversationData();

            // VariableStore에 등록된 모든 변수를 직렬화.
            variables = GetVariableData();

            // 저장 시각 기록 (세이브 슬롯 UI에 표시될 문자열).
            timeStamp = DateTime.Now.ToString("yyyy-MM-dd   HH:mm:ss");

            // 세이브 슬롯 썸네일로 사용할 스크린샷을 캡처.
            // mainCamera 뷰를 기준으로 SCREENSHOT_DOWNSCALE_AMOUNT(25%) 크기로 축소 저장.
            ScreenShotFunction.CaptureScreenshot(VNManager.instance.mainCamera, Screen.width, Screen.height, SCREENSHOT_DOWNSCALE_AMOUNT, screenshotPath);

            // 이 인스턴스 전체를 JSON 문자열로 직렬화.
            string saveJSON = JsonUtility.ToJson(this);

            // FileManager를 통해 파일에 저장. ENCRYPT 상수에 따라 암호화 여부 결정.
            FileManager.Save(filePath, saveJSON, ENCRYPT);
        }

        /*
         * Activate()
         * ────────────────────────────────────────────────────────
         * 저장된 게임 상태를 실제 게임에 복원(적용)한다.
         * Load() 이후 또는 VNManager.LoadGame()에서 호출된다.
         *
         * 복원 순서:
         *  1) 화면/캐릭터/오디오 상태 복원 (HistoryState.Load)
         *  2) 히스토리 로그 복원 (HistoryManager에 재등록)
         *  3) 전역 변수 복원 (VariableStore에 값 재주입)
         *  4) 대화 큐 복원 (Conversation 객체 재생성 후 재생 시작)
         *  5) 프롬프트(입력 대기 표시) 숨김
         * ────────────────────────────────────────────────────────
         */
        public void Activate()
        {
            // 1) 저장된 화면 상태 스냅샷을 복원.
            //    null 체크: 최초 저장 전에는 activeState가 없을 수 있음.
            if (activeState != null)
            {
                activeState.Load();
            }

            // 2) 히스토리 로그를 HistoryManager에 복원.
            //    ToList()로 배열을 List로 변환하여 재할당.
            HistoryManager.instance.history = historyLog.ToList();

            // 히스토리 UI 로그를 초기화하고 복원된 데이터로 다시 구성.
            HistoryManager.instance.logManager.Clear();
            HistoryManager.instance.logManager.Rebuild();

            // 3) VariableStore에 저장된 변수 값들을 복원.
            SetVariableData();

            // 4) 대화 큐를 복원하여 저장 시점의 대화부터 다시 시작.
            SetConversationData();

            // 5) 복원 후 즉시 플레이어 입력을 기다리면 어색하므로
            //    프롬프트(다음 대사 진행 표시)를 숨긴다.
            DialogueSystem.instance.prompt.Hide();
        }

        /*
         * GetConversationData()  [private]
         * ────────────────────────────────────────────────────────
         * 현재 대화 큐(ConversationQueue)에 있는 모든 Conversation을
         * JSON 문자열 배열로 직렬화하여 반환한다.
         *
         * [파일 기반 대화 vs 인라인 대화]
         * - 파일 기반(conversation.file != ""):
         *     파일명 + 진행도 + 인덱스 범위만 저장 (VN_ConversationDataCompressed).
         *     파일 전체를 저장하지 않아 용량이 절약된다.
         *     로드 시 파일을 다시 읽어서 복원한다.
         *
         * - 인라인 (conversation.file == ""):
         *     대화 라인 전체를 직접 저장 (VN_ConversationData).
         *     choice 결과 등 동적으로 생성된 대화에 사용된다.
         *     파일 경로가 없으므로 내용을 직접 저장해야 한다.
         *
         * [반환값]
         *  각 대화를 JSON화한 문자열 배열.
         * ────────────────────────────────────────────────────────
         */
        private string[] GetConversationData()
        {
            List<string> returnData = new List<string>();

            // 현재 대화 큐에 쌓인 모든 Conversation을 가져온다.
            var conversations = DialogueSystem.instance.conversationManager.GetConversationQueue();

            for (int i = 0; i < conversations.Length; i++)
            {
                var conversation = conversations[i];
                string data = "";

                if (conversation.file != string.Empty)
                {
                    // ── 파일 기반 대화: 압축 포맷으로 저장 ──
                    // 파일명, 시작/끝 라인 인덱스, 현재 진행도만 저장.
                    // 로드 시에는 파일을 다시 읽어 해당 범위만 추출하여 복원.
                    var compressedData = new VN_ConversationDataCompressed();
                    compressedData.fileName  = conversation.file;           // 대화 파일 경로
                    compressedData.progress  = conversation.GetProgress();  // 현재 진행된 라인 번호
                    compressedData.startIndex = conversation.fileStartIndex; // 이 대화가 시작되는 파일 내 라인 번호
                    compressedData.endIndex  = conversation.fileEndIndex;   // 이 대화가 끝나는 파일 내 라인 번호
                    data = JsonUtility.ToJson(compressedData);
                }
                else
                {
                    // ── 인라인 대화: 전체 라인을 직접 저장 ──
                    // 파일로 존재하지 않는 동적 생성 대화(choice 결과 등)는
                    // 라인 내용 자체를 저장해야 한다.
                    var fullData = new VN_ConversationData();
                    fullData.conversation = conversation.GetLines();        // 대화 라인 전체 목록
                    fullData.progress     = conversation.GetProgress();     // 현재 진행된 라인 번호
                    data = JsonUtility.ToJson(fullData);
                }

                returnData.Add(data);
            }
            return returnData.ToArray();
        }

        /*
         * SetConversationData()  [private]
         * ────────────────────────────────────────────────────────
         * activeConversations 배열에 저장된 JSON 문자열들을
         * 실제 Conversation 객체로 역직렬화하여 대화 큐에 복원한다.
         *
         * [역직렬화 우선순위]
         *  1) VN_ConversationData (인라인 포맷) 시도
         *     → conversation 리스트가 비어있지 않으면 인라인으로 복원.
         *  2) VN_ConversationDataCompressed (압축 포맷) 시도
         *     → fileName이 있으면 파일을 읽어 해당 범위만 추출하여 복원.
         *  3) 둘 다 실패하면 오류 로그 출력 후 해당 항목 건너뜀.
         *
         * [큐 복원 규칙]
         *  - i == 0 (첫 번째): StartConversation()으로 즉시 재생 시작.
         *  - i > 0  (나머지): Enqueue()로 대기 큐에 순서대로 추가.
         * ────────────────────────────────────────────────────────
         */
        private void SetConversationData()
        {
            for (int i = 0; i < activeConversations.Length; i++)
            {
                try
                {
                    string data = activeConversations[i];
                    Conversation conversation = null;

                    // ── 인라인 포맷 시도 ──
                    var fullData = JsonUtility.FromJson<VN_ConversationData>(data);
                    if (fullData != null && fullData.conversation != null && fullData.conversation.Count > 0)
                    {
                        // 인라인 대화: 저장된 라인 목록과 진행도로 Conversation 재생성.
                        conversation = new Conversation(fullData.conversation, fullData.progress);
                    }
                    else
                    {
                        // ── 압축 포맷 시도 ──
                        var compressedData = JsonUtility.FromJson<VN_ConversationDataCompressed>(data);
                        if (compressedData != null && compressedData.fileName != string.Empty)
                        {
                            // 파일 기반 대화: 원본 파일을 다시 읽어 해당 범위만 추출.
                            TextAsset file = Resources.Load<TextAsset>(compressedData.fileName);

                            // endIndex - startIndex + 1 줄을 startIndex부터 읽는다.
                            int count = compressedData.endIndex - compressedData.startIndex;
                            List<string> lines = FileManager.ReadTextAsset(file)
                                                            .Skip(compressedData.startIndex)
                                                            .Take(count + 1)
                                                            .ToList();

                            // 파일명과 인덱스 범위 정보를 함께 전달하여 Conversation 재생성.
                            conversation = new Conversation(lines, compressedData.progress,
                                                            compressedData.fileName,
                                                            compressedData.startIndex,
                                                            compressedData.endIndex);
                        }
                        else
                        {
                            // 두 포맷 모두 실패: 알 수 없는 데이터 형식.
                            Debug.LogError($"Unkown conversation format! unable to reload conversation from VNGameSave using data '{data}'!");
                            continue;
                        }
                    }

                    if (conversation != null && conversation.GetLines().Count > 0)
                    {
                        if (i == 0)
                        {
                            // 첫 번째 대화: 즉시 재생 시작.
                            DialogueSystem.instance.conversationManager.StartConversation(conversation);
                        }
                        else
                        {
                            // 그 이후 대화: 재생 대기 큐에 순서대로 추가.
                            DialogueSystem.instance.conversationManager.Enqueue(conversation);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // 역직렬화 중 예외 발생 시 해당 항목을 건너뛰고 계속 진행.
                    Debug.LogError($"Encountered Error while extracting saved conversation data! : {ex}");
                    continue;
                }
            }
        }

        /*
         * GetVariableData()  [private]
         * ────────────────────────────────────────────────────────
         * VariableStore에 등록된 모든 변수를 직렬화하여 반환한다.
         *
         * [변수 이름 형식]
         *  "DB명.변수명" 으로 저장한다.
         *  예) "Default.playerGold", "VN.mainCharacterName"
         *  로드 시 SetVariableData에서 같은 형식으로 TrySetValue를 호출한다.
         *
         * [타입 저장]
         *  변수 값의 C# 타입 전체 이름을 문자열로 저장한다.
         *  예) bool → "System.Boolean", int → "System.Int32"
         *  로드 시 타입 문자열을 보고 적절하게 파싱한다.
         *
         * [반환값]
         *  직렬화된 VN_VariableData 배열.
         * ────────────────────────────────────────────────────────
         */
        private VN_VariableData[] GetVariableData()
        {
            List<VN_VariableData> returnData = new List<VN_VariableData>();

            // VariableStore의 모든 데이터베이스를 순회.
            foreach (var database in VariableStore.databases.Values)
            {
                // 각 데이터베이스 내의 모든 변수를 순회.
                foreach (var variable in database.variables)
                {
                    VN_VariableData variableData = new VN_VariableData();

                    // 이름: "DB명.변수명" 형식으로 조합.
                    variableData.name = $"{database.name}.{variable.Key}";

                    // 값: 현재 저장된 값을 문자열로 변환.
                    string val = $"{variable.Value.Get()}";
                    variableData.value = val;

                    // 타입: 값이 빈 문자열이면 String으로 처리,
                    //       아니면 실제 런타임 타입의 전체 이름을 저장.
                    variableData.type = val == string.Empty ? "System.String" : variable.Value.Get().GetType().ToString();

                    returnData.Add(variableData);
                }
            }
            return returnData.ToArray();
        }

        /*
         * SetVariableData()  [private]
         * ────────────────────────────────────────────────────────
         * 저장된 VN_VariableData 배열을 읽어 VariableStore에 값을 복원한다.
         *
         * [타입 파싱]
         *  저장된 type 문자열을 기준으로 값을 적절한 C# 타입으로 변환한다.
         *  - "System.Boolean" → bool  로 파싱
         *  - "System.Int32"   → int   로 파싱
         *  - "System.Single"  → float 로 파싱  (Single = C#의 float)
         *  - "System.String"  → string 그대로 사용
         *
         *  파싱 실패 시 오류 로그를 출력한다.
         *
         * VariableStore.TrySetValue(이름, 값):
         *  "DB명.변수명" 형식의 이름으로 해당 변수에 값을 주입한다.
         * ────────────────────────────────────────────────────────
         */
        private void SetVariableData()
        {
            foreach (var variable in variables)
            {
                string val = variable.value;

                switch (variable.type)
                {
                    case "System.Boolean": // bool 타입 복원
                        if (bool.TryParse(val, out bool b_val))
                        {
                            VariableStore.TrySetValue(variable.name, b_val);
                            continue;
                        }
                        break;

                    case "System.Int32": // int 타입 복원
                        if (int.TryParse(val, out int i_val))
                        {
                            VariableStore.TrySetValue(variable.name, i_val);
                            continue;
                        }
                        break;

                    case "System.Single": // float 타입 복원 (C#에서 float의 전체 이름은 System.Single)
                        if (float.TryParse(val, out float f_val))
                        {
                            VariableStore.TrySetValue(variable.name, f_val);
                            continue;
                        }
                        break;

                    case "System.String": // string 타입 복원 (파싱 없이 그대로 사용)
                        VariableStore.TrySetValue(variable.name, val);
                        continue;
                }

                // 위의 모든 case에서 처리되지 못한 경우 오류 로그 출력.
                Debug.LogError($"Could not interpret variable type. \nname: '{variable.name}', \nvalue: '{variable.value}', \ntype: '{variable.type}'");
            }
        }
    }
}
