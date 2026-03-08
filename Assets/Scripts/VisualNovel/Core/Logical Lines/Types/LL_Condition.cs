using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Expressions;

namespace DIALOGUE.LogicalLines
{
    public class LL_Condition : ILogicalLine
    {
        public  string keyword => "if";
        private readonly string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] {"(", ")"};

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string rawCondition = ExtractCondition(line.rawData.Trim());
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return line.rawData.Trim().StartsWith(keyword);
        }

        private string ExtractCondition(string line)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.IndexOf(CONTAINERS[1]);

            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}

