using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Conditions;

namespace DIALOGUE.LogicalLines
{
    public class LL_Condition : ILogicalLine
    {
        public  string keyword => "if";
        private readonly string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] {"(", ")"};

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string rawCondition  = ExtractCondition (line.rawData.Trim());
            bool conditionResult = EvaluateCondition(rawCondition);

            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.instance.conversationManager.conversationProgress;

            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, false, parentStartingIndex: currentConversation.fileStartIndex);
            EncapsulatedData elseData = new EncapsulatedData() ;


            if (ifData.endingIndex + 1 < currentConversation.Count)
            {
                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
                if(nextLine == ELSE)
                {
                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false, parentStartingIndex: currentConversation.fileStartIndex);
                }
            }

            currentConversation.SetProgress(elseData.isNull ? ifData.endingIndex: elseData.endingIndex);

            EncapsulatedData selData = conditionResult ? ifData : elseData;
            
            if (!selData.isNull && selData.lines.Count > 0)
            {
                //Remove the header and encapsulator lines from the conversation indexes
                selData.startingIndex +=2; // Reove header and starting encapsulator
                selData.endingIndex -= 1;// Remove ending encapsulator

                Conversation newConversation = new Conversation(selData.lines, file: currentConversation.file, fileStartIndex: selData.startingIndex, fileEndIndex: selData.endingIndex);
                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
            }

            yield return null;
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

