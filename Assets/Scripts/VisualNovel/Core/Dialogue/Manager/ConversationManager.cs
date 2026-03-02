using CHARACTERS;
using COMMANDS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE.LogicalLines;

namespace DIALOGUE
{
    public class 
        
        ConversationManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.instance;
        private Coroutine process = null;
        public bool isRunning => process != null;
        public TextArchitect architect = null;
        private bool userPrompt = false;

        private TagManager tagManager;
        private LogicalLineManager logicalLineManager;

        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += onUserPrompt_Next;

            tagManager = new TagManager();
            logicalLineManager = new LogicalLineManager();
        }

        private void onUserPrompt_Next()
        {
            userPrompt = true;
        }

        public Coroutine StartConversation(List<string> conversation)
        {
            StopConversation();

            process = dialogueSystem.StartCoroutine(RunningConversation(conversation));

            return process;
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
                //Dont show any black lines or try to run any logic on them.
                if (string.IsNullOrWhiteSpace(conversation[i]))
                    continue;

                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);

                if(logicalLineManager.TryGetLogic(line, out Coroutine logic))
                {
                    yield return logic;
                }
                else
                {
                    //Show dialogue
                    if (line.hasDialogue)
                    {
                        yield return Line_RunDialogue(line);
                    }

                    // Run any commands
                    if (line.hasCommands)
                    {
                        yield return LINE_RunCommands(line);
                    }

                    //Wait for user input if dialogue was in this line
                    if (line.hasDialogue)
                    {
                        //wait for user Input
                        yield return WaitForUserInput();

                        CommandManager.instance.StopAllProcesses();
                    }
                }

            }

        }

        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)// 대사 한 줄을 실행하는 코루틴
        {
            // Show or hide the speaker name if there is one present.
            if (line.hasSpeaker)
            {
                HandleSpeakerLogic(line.speakerData);
            }

            if(!dialogueSystem.dialogueContainer.isVisible)
            {
                dialogueSystem.dialogueContainer.Show();
            }

            //build dialogueData
            yield return BuildLineSegments(line.dialogueData);
        }

        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            bool characterMustBeCreated = (speakerData.makeCharacerEnter || speakerData.isCastingExpression || speakerData.isCastingPosition);

            Character character = CharacterManager.instance.GetCharacter(speakerData.name, createIfDoesNotExist: characterMustBeCreated);

            if (speakerData.makeCharacerEnter && (!character.isVisible && !character.isRevealing))
                character.Show();

            //Add character name to the UI.
            dialogueSystem.ShowSpeakerName(tagManager.Inject(speakerData.displayname));

            //Now customize the dialogue for this character - if applicable.
            DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);

            //Set character Casting position
            if (speakerData.isCastingPosition)
                character.MoveToPosition(speakerData.castPosition);

            //Cast Expression
            if (speakerData.isCastingExpression)
            {
                foreach (var ce in speakerData.CastExpressions)
                {
                    character.OnReceiveCastingExpression(ce.layer, ce.expression);
                }
            }
        }

        IEnumerator LINE_RunCommands(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;

            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                if (command.waitForCompletion || command.name == "wait")
                {
                    CoroutineWrapper cw = CommandManager.instance.Execute(command.name, command.arguments);
                    while (!cw.IsDone)
                    {
                        if (userPrompt) 
                        {
                            CommandManager.instance.StopCurrentProcess();
                            userPrompt = false;
                        }
                        yield return null;
                    }

                }

                else
                    CommandManager.instance.Execute(command.name, command.arguments);
            }
            yield return null;
        }

        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            for (int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];

                yield return WaitForDialogueSegmentSignalToBeTriggered(segment);

                yield return BuildDialogue(segment.dialogue, segment.appendText);
            }
        }

        public bool isWaitingOnSegmentTimer { get; private set; } = false;

        IEnumerator WaitForDialogueSegmentSignalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    isWaitingOnSegmentTimer = true;
                    float timer = 0f;
                    while (timer < segment.signalDelay)
                    {
                        // Skip / Auto 진행 신호가 들어오면 대기 즉시 종료
                        if (userPrompt)
                        { 
                            userPrompt = false; // 신호 소비
                            break;
                        }

                        timer += Time.deltaTime; // 필요하면 Time.unscaledDeltaTime 로 변경 가능
                        yield return null;
                    }

                    isWaitingOnSegmentTimer = false;
                    break;

            }
        }

        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            dialogue = tagManager.Inject(dialogue);

            //Build the dialogueData
            if (!append)
                architect.Build(dialogue);
            else
                architect.Append(dialogue);

            //Wait for the dialogueData to complete
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
            dialogueSystem.prompt.Show();

            while (!userPrompt)
                yield return null;

            dialogueSystem.prompt.Hide();

            userPrompt = false;
        }
    }
}