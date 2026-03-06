using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    public static class LogicalLineUtils
    {
        public static class Encapsulation
        {

            public struct EncapsulatedData
            {
                public List<string> lines;
                public int startingIndex;
                public int endingIndex;

            }

            private const char ENCAPSULATION_START   = '{';
            private const char ENCAPSULATION_END     = '}';

            public static EncapsulatedData RipEncapsulationData(Conversation conversation, int startingIndex, bool ripHeaderAndEncapsulators = false)
            {
                int encapsulationDepth = 0;
                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startingIndex = startingIndex, endingIndex = 0 };

                for (int i = startingIndex; i < conversation.Count; i++)
                {
                    string line = conversation.GetLines()[i];

                    if (ripHeaderAndEncapsulators || (encapsulationDepth > 0 && !IsEncapsulationEnd(line)))
                    {
                        data.lines.Add(line);
                    }

                    if (IsEncapsulationStart(line))
                    {
                        encapsulationDepth++;
                        continue;
                    }

                    if (IsEncapsulationEnd(line))
                    {
                        encapsulationDepth--;
                        if (encapsulationDepth == 0)
                        {
                            data.endingIndex = i;
                            break;
                        }
                    }
                }

                return data;
            }

            public static bool IsEncapsulationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START);
            public static bool IsEncapsulationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END);
        }
    
        public static class Expressions
        {
            public static HashSet<string> OPERATORS = new HashSet<string>() { "-", "-=", "+", "+=", "*", "/", "=" };
        }
    }
}