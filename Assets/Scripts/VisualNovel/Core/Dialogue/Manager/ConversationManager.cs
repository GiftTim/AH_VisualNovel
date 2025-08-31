using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class ConversationManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.instance;
        private Coroutine process = null;
        public bool isRunning => process != null;
        private TextArchitect architect = null;
        private bool userPrompt = false;

        public ConversationManager(TextArchitect architect)
        { 
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += onUserPrompt_Next;
        }
        
        private void onUserPrompt_Next()
        {
            userPrompt = true;
        }

        public void StartConversation(List<string> conversation)
        {
            StopConversation();

            process = dialogueSystem.StartCoroutine(RunningConversation(conversation));
        }

        public void StopConversation()
        {
            if (!isRunning)
                return;

            dialogueSystem.StopCoroutine(process);
            process = null;
        }

        IEnumerator RunningConversation(List<string> conversation)
        {
            for (int i = 0; i < conversation.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(conversation[i]))
                    continue;

                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);

                //Show Danganronpa_Dialogue
                if (line.hasDialogue)
                {
                    yield return Line_RunDialogue(line);
                }

                // Execute Commands
                if (line.hasCommands)
                {
                    yield return LINE_RunCommands(line);
                }

            }

        }

        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {    
            if(line.hasSpeaker)
                dialogueSystem.ShowSpeakerName(line.speaker);

            //build dialogue
            yield return BuildDialogue(line.dialogue);

            //wait for user Input
            yield return WaitForUserInput();

        }

        IEnumerator LINE_RunCommands(DIALOGUE_LINE line)
        {
            Debug.Log(line.commands);
            yield return null;
        }

        IEnumerator BuildDialogue(string dialogue)
        {
            architect.Build(dialogue);

            while (architect.isBuilding)
            {
                if (userPrompt)
                {
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else
                        architect.ForceComplete();

                    userPrompt = false;
                }
                yield return null;

            }
        }

        IEnumerator WaitForUserInput()
        {
            while(!userPrompt)
                yield return null;

            userPrompt = false;
        }
    }
}