using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DIALOGUE
{
    public class DL_COMMAND_DATA
    {
        public List<Command> commands;
        private const string COMMAND_PATTERN = @"([\w\.|\d+|\[|\]])*\(([^)]*)\),?";
        private const string WAITCOMMAND_ID = "[wait]";

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

        private List<Command> RipCommands(string rawCommands)
        {
            MatchCollection data = Regex.Matches(rawCommands, COMMAND_PATTERN);
            List<Command> result = new List<Command>();

            foreach (Match cmd in data)
            {
                Command command = new Command();
                string[] parts = cmd.Value.Split('(');

                command.name = parts[0].Trim();

                if (command.name.ToLower().StartsWith(WAITCOMMAND_ID))
                {
                    command.name = command.name.Substring(WAITCOMMAND_ID.Length);
                    command.waitForCompletion = true;
                }
                else
                    command.waitForCompletion = false;

                string arguments = parts[1].TrimEnd(')', ',');
                command.arguments = GetArgs(arguments);

                result.Add(command);
            }

            return result;
        }

        private string[] GetArgs(string args)
        {
            List<string> argList = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && args[i] == ' ')
                {
                    argList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }

                currentArg.Append(args[i]);
            }

            if (currentArg.Length > 0)
                argList.Add(currentArg.ToString());

            return argList.ToArray();
        }
    }
}
