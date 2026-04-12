using System.Text.RegularExpressions;

namespace DIALOGUE
{
    /*
     * DialogueParser
     * ─────────────────────────────────────────────────────────────────────
     * 원본 텍스트 한 줄을 받아 DIALOGUE_LINE으로 변환하는 파서.
     *
     * 파싱 흐름:
     *   1) RipContent()로 rawLine에서 speaker / dialogue / commands 분리
     *   2) TagManager.Inject()로 commands에 태그/변수 즉시 주입
     *   3) DIALOGUE_LINE 생성 후 반환
     *      (speaker / dialogue의 태그 주입은 ConversationManager가 나중에 처리)
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DialogueParser
    {
        /*
         * commandRegexPattern
         * 명령어 시작을 감지하는 정규식:
         *   [\w\[\]]*   → 알파벳, 숫자, '[', ']' 로 구성된 명령어 이름
         *   [^\s]       → 공백이 아닌 문자 (이름 끝 확인)
         *   \(          → 여는 괄호 (명령어 호출 시작)
         * 예) moveCharacter(, playSound(, [wait]fadeIn(
         */
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";


        /*
         * Parse
         * rawLine 한 줄을 파싱해 DIALOGUE_LINE을 반환한다.
         *
         * 주의: commands에 대한 TagManager.Inject 호출이 두 번 있다.
         *   - 첫 번째 호출(아래 주석 참고): 중복 호출. 실질적으로 동작하지만 의도되지 않음.
         *   - 두 번째 호출(블록 주석 아래): 실제로 의도된 주입 호출.
         *   코드 로직을 변경하지 않으므로 두 호출 모두 유지한다.
         */
        public static DIALOGUE_LINE Parse(string rawLine)
        {
            (string speaker, string dialogue, string commands) = RipContent(rawLine);

            // [주의] 이 호출은 중복 호출임. 실제 의도된 주입은 아래 두 번째 호출.
            commands = TagManager.Inject(commands);

            //Debug.Log($"Parsing line - '{rawLine}'");
            //Debug.Log($"Speak='{speaker}' \nDialogue = '{dialogue}'\nCommands = '{commands}'");

            /*
            We have to inject tags and variables into the speaker and dialogue separately because there are initial checks that have to be performed.
            But commands neeed no checks, so we can inject the variables in them right now.
            화자와 대화에는 태그와 변수를 별도로 주입해야 함
            초기 검증(checks)이 필요하기 때문
            하지만 명령은 검증이 필요 없으므로 즉시 변수 주입 가능
            */
            // [실제 주입] commands에 태그 및 변수를 치환한다
            commands = TagManager.Inject(commands);

            return new DIALOGUE_LINE(rawLine, speaker, dialogue, commands);
        }

        /*
         * RipContent
         * rawLine에서 speaker / dialogue / commands 를 분리하는 핵심 메서드.
         *
         * 4가지 분기:
         *   1) 대화("...") 없이 명령어만 있는 경우
         *      → commands = rawLine 전체
         *   2) 대화가 있고 명령어가 없거나 대화 뒤에 있는 경우 (정상적인 대화 라인)
         *      → speaker = 따옴표 이전, dialogue = 따옴표 안, commands = 대화 뒤
         *   3) 명령어가 대화 시작 따옴표보다 앞에 있는 경우
         *      → commands = rawLine 전체
         *   4) 기타 (따옴표도 명령어도 없는 순수 대사)
         *      → dialogue = rawLine 전체
         */
        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker="", dialogue="", commands = "";

            int dialogueStart = -1; // 여는 따옴표 위치
            int dialogueEnd   = -1; // 닫는 따옴표 위치
            bool isEscaped = false; // 이스케이프(\") 처리 플래그

            for(int i = 0; i < rawLine.Length; i++)
            {
                char current = rawLine[i];

                if (current == '\\')
                    isEscaped = !isEscaped;             // '\' 를 만나면 이스케이프 상태 토글
                else if (current == '"' && !isEscaped)
                {
                    // 이스케이프되지 않은 따옴표만 대화 범위로 인식
                    if (dialogueStart == -1)
                        dialogueStart = i;
                    else if (dialogueEnd == -1)
                        dialogueEnd = i;
                }
                else
                    isEscaped = false;                  // 이스케이프 대상 문자가 아니면 플래그 리셋
            }

            //Identify Command Pattern
            // 명령어 패턴의 시작 위치를 찾는다 (대화 범위 안에 있는 것은 제외)
            Regex commandRegex = new Regex(commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(rawLine);
            int commandStart = -1;
            foreach (Match match in matches)
            {
                if (match.Index < dialogueStart || match.Index > dialogueEnd)
                {
                    commandStart = match.Index;
                    break;
                }
            }

            // 분기 1: 따옴표(대화)는 없고 명령어만 있는 경우
            if (commandStart != -1 && (dialogueStart == -1 && dialogueEnd == -1))
                return ("", "", rawLine.Trim());

            // 분기 2: 따옴표 범위가 유효하고 명령어가 없거나 대화 뒤에 있는 경우
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                // We know that we have valid dialogueData
                // 따옴표 이전이 화자, 따옴표 안이 대사, 대화 뒤가 명령어
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\""); // \" → " 으로 언이스케이프
                if (commandStart != -1)
                    commands = rawLine.Substring(commandStart).Trim();
            }
            // 분기 3: 명령어가 대화 따옴표보다 앞에 있는 경우
            else if (commandStart != -1 && dialogueStart > commandStart)
            {
                commands = rawLine;
            }
            // 분기 4: 따옴표도 명령어도 없는 순수 대사
            else
                dialogue = rawLine;

            return (speaker, dialogue, commands);

        }
    }
}
