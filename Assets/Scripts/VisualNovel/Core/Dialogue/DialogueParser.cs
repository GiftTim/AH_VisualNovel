using System.Text.RegularExpressions;

namespace DIALOGUE
{
    public class DialogueParser
    {
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";


        public static DIALOGUE_LINE Parse(string rawLine)
        {
            (string speaker, string dialogue, string commands) = RipContent(rawLine);

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
            commands = TagManager.Inject(commands);

            return new DIALOGUE_LINE(rawLine, speaker, dialogue, commands);
        }

        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker="", dialogue="", commands = "";

            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool isEscaped = false;

            for(int i = 0; i < rawLine.Length; i++)
            {
                char current = rawLine[i];

                if (current == '\\')
                    isEscaped = !isEscaped;
                else if (current == '"' && !isEscaped)
                {
                    if (dialogueStart == -1)
                        dialogueStart = i;
                    else if (dialogueEnd == -1)
                        dialogueEnd = i;
                }
                else
                    isEscaped = false;
            }

            //Identify Command Pattern
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

            if (commandStart != -1 && (dialogueStart == -1 && dialogueEnd == -1))
                return ("", "", rawLine.Trim());

            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                // We know that we have valid dialogueData
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1)
                    commands = rawLine.Substring(commandStart).Trim();
            }
            else if (commandStart != -1 && dialogueStart > commandStart)
            {
                commands = rawLine;
            }
            else
                dialogue = rawLine;

            return (speaker, dialogue, commands);

        }
    }
}