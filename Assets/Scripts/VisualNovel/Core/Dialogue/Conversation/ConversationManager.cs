using CHARACTERS;
using COMMANDS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE.LogicalLines;
using Unity.VisualScripting;

namespace DIALOGUE
{
    public class ConversationManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.instance;
        private Coroutine process = null;
        public bool isRunning => process != null;
        public bool isOnLogicalLine {get; private set;} = false;
        public TextArchitect architect = null;
        private bool userPrompt = false;


        private LogicalLineManager logicalLineManager;

        public Conversation conversation => (conversationQueue.IsEmpty() ? null : conversationQueue.top);
        public int conversationProgress => (conversationQueue.IsEmpty() ? -1 : conversationQueue.top.GetProgress());
        private ConversationQueue conversationQueue;

        public bool allowUserPrompts = true;

        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += onUserPrompt_Next;

            logicalLineManager = new LogicalLineManager();
            
            conversationQueue = new ConversationQueue();
        }

        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation) => conversationQueue.EnqueuePriority(conversation);

        private void onUserPrompt_Next()
        {
            if(allowUserPrompts)
                userPrompt = true;
        }

        public Coroutine StartConversation(Conversation conversation)
        {
            StopConversation();
            conversationQueue.Clear();

            Enqueue(conversation);

            process = dialogueSystem.StartCoroutine(RunningConversation());

            return process;
        }

        public void StopConversation()
        {
            if (!isRunning)
                return;

            dialogueSystem.StopCoroutine(process);
            process = null;
        }

        IEnumerator RunningConversation()
        {
            while (!conversationQueue.IsEmpty())
            {
                Conversation currentConversation = conversation;

                if (currentConversation.HasReachedEnd())
                {
                    conversationQueue.Dequeue();
                    continue;
                }

                string rawLine = currentConversation.CurrentLine();

                // 빈 줄일 경우 건너뛰기
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    TryAdvanceConversation(currentConversation);
                    continue;
                }

                DIALOGUE_LINE line = DialogueParser.Parse(rawLine);

                if (logicalLineManager.TryGetLogic(line, out Coroutine logic))
                {
                    isOnLogicalLine = true;
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

                        dialogueSystem.onSystemPrompt_Clear();
                    }
                }

                    TryAdvanceConversation(currentConversation);
                    isOnLogicalLine = false;
            }

            process = null;
    }

        private void TryAdvanceConversation(Conversation conversation)
        {
            conversation.IncrementProgress();
        
            if (conversation != conversationQueue.top)
            {
                return;
            }
            if (conversation.HasReachedEnd())
            {
                conversationQueue.Dequeue();
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
            dialogueSystem.ShowSpeakerName(TagManager.Inject(speakerData.displayname));

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
                
                string dialogueText = segment.dialogue;

                if (segment.startSignal == DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.N)
                {
                    dialogueText = "\n" + dialogueText;
                }

                yield return BuildDialogue(dialogueText, segment.appendText);
            }
        }

        public bool isWaitingOnAutoTimer { get; private set; } = false;

        IEnumerator WaitForDialogueSegmentSignalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                    yield return WaitForUserInput();
                    dialogueSystem.onSystemPrompt_Clear();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;

                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                    isWaitingOnAutoTimer = true;
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

                    isWaitingOnAutoTimer = false;
                    dialogueSystem.onSystemPrompt_Clear();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    isWaitingOnAutoTimer = true;
                    timer = 0f;
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

                    isWaitingOnAutoTimer = false;
                    break;

                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.N:
                    yield return WaitForUserInput();
                    break;
            }
        }

        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            dialogue = TagManager.Inject(dialogue);

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