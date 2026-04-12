using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DIALOGUE
{
    /*
     * DL_COMMAND_DATA
     * ─────────────────────────────────────────────────────────────────────
     * 대화 라인에 포함된 명령어 문자열을 파싱해 Command 목록으로 저장한다.
     * 한 라인에 여러 명령어를 동시에 선언할 수 있다.
     *
     * 명령어 문법 예시:
     *   playSound("bgm_main"),         → 단순 실행 (fire-and-forget)
     *   [wait]moveCharacter("KIM" 0.5) → 완료 대기 후 다음으로 진행
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DL_COMMAND_DATA
    {
        // 파싱된 명령어 목록
        public List<Command> commands;

        /*
         * COMMAND_PATTERN
         * 명령어 한 개를 감지하는 정규식:
         *   ([\w\.|\d+|\[|\]])*  → 명령어 이름 부분 (알파벳, 숫자, '.', '[', ']' 허용)
         *   \(([^)]*)\)          → 괄호 안의 인수 부분
         *   ,?                   → 명령어 구분을 위한 선택적 쉼표
         */
        private const string COMMAND_PATTERN = @"([\w\.|\d+|\[|\]])*\(([^)]*)\),?";

        /*
         * WAITCOMMAND_ID
         * "[wait]" 접두사가 붙은 명령어는 완료될 때까지 대화 진행을 멈춘다.
         * 이 접두사는 파싱 후 이름에서 제거되고 waitForCompletion = true 로 저장.
         */
        private const string WAITCOMMAND_ID = "[wait]";

        /*
         * Command 구조체
         *   name              : 명령어 이름 (예: "moveCharacter", "playSound")
         *   arguments         : 명령어 인수 배열 (따옴표 안 공백은 분리하지 않음)
         *   waitForCompletion : true이면 명령어 완료 전까지 대화를 정지
         */
        public struct Command
        {
            public string name;
            public string[] arguments;
            public bool waitForCompletion;
        }

        public DL_COMMAND_DATA(string rawCommands)
        {
            commands = RipCommands(rawCommands);
        }

        /*
         * RipCommands
         * COMMAND_PATTERN 정규식으로 rawCommands에서 모든 명령어를 추출한다.
         * 각 매치에서 이름과 인수를 분리하고, [wait] 접두사 여부를 확인해
         * waitForCompletion 플래그를 설정한다.
         */
        private List<Command> RipCommands(string rawCommands)
        {
            MatchCollection data = Regex.Matches(rawCommands, COMMAND_PATTERN);
            List<Command> result = new List<Command>();

            foreach (Match cmd in data)
            {
                Command command = new Command();
                // "name(" 형태로 분리: parts[0]=이름, parts[1]=인수(닫는 괄호 포함)
                string[] parts = cmd.Value.Split('(');

                command.name = parts[0].Trim();

                // [wait] 접두사가 있으면 이름에서 제거하고 완료 대기 플래그 설정
                if (command.name.ToLower().StartsWith(WAITCOMMAND_ID))
                {
                    command.name = command.name.Substring(WAITCOMMAND_ID.Length);
                    command.waitForCompletion = true;
                }
                else
                    command.waitForCompletion = false;

                // 인수 문자열에서 닫는 ')' 와 ','를 제거 후 파싱
                string arguments = parts[1].TrimEnd(')', ',');
                command.arguments = GetArgs(arguments);

                result.Add(command);
            }

            return result;
        }

        /*
         * GetArgs
         * 인수 문자열을 공백으로 분리하되, 큰따옴표("") 안의 공백은 분리하지 않는다.
         * 예) "Hello World" 0.5  →  ["Hello World", "0.5"]
         *     (따옴표 문자 자체는 결과에서 제거됨)
         */
        private string[] GetArgs(string args)
        {
            List<string> argList = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            bool inQuotes = false; // 현재 따옴표 안에 있는지 여부

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == '"')
                {
                    inQuotes = !inQuotes; // 따옴표 진입/탈출 토글 (따옴표 문자 자체는 결과에 포함 안 함)
                    continue;
                }

                if (!inQuotes && args[i] == ' ')
                {
                    // 따옴표 밖의 공백은 인수 구분자로 처리
                    argList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }

                currentArg.Append(args[i]);
            }

            // 마지막 인수 추가
            if (currentArg.Length > 0)
                argList.Add(currentArg.ToString());

            return argList.ToArray();
        }
    }
}
