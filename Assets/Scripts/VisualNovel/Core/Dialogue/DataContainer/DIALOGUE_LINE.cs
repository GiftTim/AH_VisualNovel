namespace DIALOGUE
{
    /*
     * DIALOGUE_LINE
     * ─────────────────────────────────────────────────────────────────────
     * DialogueParser.Parse()가 원본 텍스트 한 줄을 파싱한 최종 결과물.
     * 하나의 대화 라인은 세 가지 파트로 구성된다:
     *   - speakerData  : 누가 말하는가 (화자 정보, 없으면 null)
     *   - dialogueData : 무엇을 말하는가 (대사 텍스트, 없으면 null)
     *   - commandData  : 어떤 명령어를 실행하는가 (명령어 목록, 없으면 null)
     * rawData에는 파싱 이전의 원본 텍스트가 보존된다.
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DIALOGUE_LINE
    {
        // 파싱 전 원본 텍스트. 디버깅 및 세이브/로드 시 참조용으로 보존.
        public string rawData { get; private set; } = string.Empty;

        // 화자 정보. 화자가 없는 라인(순수 명령어 라인 등)은 null.
        public DL_SPEAKER_DATA speakerData;

        // 대사 텍스트 정보. 대사가 없는 라인은 null.
        public DL_DIALOGUE_DATA dialogueData;

        // 명령어 정보. 명령어가 없는 라인은 null.
        public DL_COMMAND_DATA commandData;

        // null 체크를 간편하게 하기 위한 편의 프로퍼티들
        public bool hasSpeaker  => speakerData  != null;
        public bool hasDialogue => dialogueData != null;
        public bool hasCommands => commandData  != null;

        /*
         * 생성자
         * speaker / dialogue / commands 가 빈 문자열이면 해당 파트를 null로 저장.
         * → hasSpeaker / hasDialogue / hasCommands 프로퍼티로 null 여부를 안전하게 확인 가능.
         */
        public DIALOGUE_LINE(string rawLine, string speaker, string dialogue, string commands)
        {
            rawData = rawLine;
            this.speakerData   = (string.IsNullOrWhiteSpace(speaker)   ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogueData  = (string.IsNullOrWhiteSpace(dialogue)  ? null : new DL_DIALOGUE_DATA(dialogue));
            this.commandData   = (string.IsNullOrWhiteSpace(commands)  ? null : new DL_COMMAND_DATA(commands));
        }
    }
}


