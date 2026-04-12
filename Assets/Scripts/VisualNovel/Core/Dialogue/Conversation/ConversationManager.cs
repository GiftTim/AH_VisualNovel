using CHARACTERS;
using COMMANDS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE.LogicalLines;

namespace DIALOGUE
{
    /*
     * ConversationManager
     * ─────────────────────────────────────────────────────────────────────
     * 대화 루프(RunningConversation) 전체를 총괄하는 클래스.
     * DialogueSystem으로부터 TextArchitect를 주입받아 동작하며,
     * 코루틴 기반으로 대화를 순서대로 실행한다.
     *
     * 주요 이벤트 연결:
     *   DialogueSystem.onUserPrompt_Next → onUserPrompt_Next()
     *   → userPrompt 플래그를 true로 설정 → 코루틴 내부에서 소비
     * ─────────────────────────────────────────────────────────────────────
     */
    public class ConversationManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.instance;

        // 실행 중인 대화 코루틴. null이면 대화가 진행 중이 아님.
        private Coroutine process = null;
        // process != null 이면 대화가 진행 중
        public bool isRunning => process != null;
        // 현재 LogicalLine(choice/if/input 등)을 처리 중이면 true
        public bool isOnLogicalLine {get; private set;} = false;

        // 텍스트를 타이핑하는 TextArchitect 인스턴스 (DialogueSystem이 주입)
        public TextArchitect architect = null;

        /*
         * userPrompt 플래그
         * 사용자 입력(클릭 등) 이벤트가 발생하면 true로 설정된다.
         * Unity 코루틴은 단일 스레드이므로 실제 동시성 문제는 없음.
         * 단, 코루틴이 yield return 하는 순간에만 다른 코드가 실행된다.
         * - 설정 타이밍: onUserPrompt_Next() 호출 시
         * - 소비 타이밍: WaitForUserInput(), BuildDialogue(), LINE_RunCommands() 등에서 확인 후 false로 초기화
         */
        private bool userPrompt = false;

        // choice / if / input 등의 논리 라인을 처리하는 매니저
        private LogicalLineManager logicalLineManager;

        // 현재 진행 중인 대화 (큐가 비어있으면 null)
        public Conversation conversation => (conversationQueue.IsEmpty() ? null : conversationQueue.top);
        // 현재 대화의 진행 인덱스 (큐가 비어있으면 -1)
        public int conversationProgress => (conversationQueue.IsEmpty() ? -1 : conversationQueue.top.GetProgress());
        // 대화 대기열
        private ConversationQueue conversationQueue;

        /*
         * allowUserPrompts
         * false이면 사용자 입력(클릭)이 들어와도 userPrompt를 true로 설정하지 않는다.
         * 히스토리 보기 중에 대화가 멋대로 진행되는 것을 막기 위해 사용.
         */
        public bool allowUserPrompts = true;

        /*
         * 생성자
         * TextArchitect를 받아 저장하고, DialogueSystem의 onUserPrompt_Next 이벤트를 구독.
         */
        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += onUserPrompt_Next;

            logicalLineManager = new LogicalLineManager();

            conversationQueue = new ConversationQueue();
        }

        // 현재 대화 큐의 스냅샷을 배열로 반환 (읽기 전용)
        public Conversation[] GetConversationQueue() => conversationQueue.GetReadOnly();

        // 대화를 큐 맨 뒤에 추가
        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);
        // 대화를 큐 맨 앞에 추가 (우선 실행)
        public void EnqueuePriority(Conversation conversation) => conversationQueue.EnqueuePriority(conversation);

        /*
         * onUserPrompt_Next
         * DialogueSystem.onUserPrompt_Next 이벤트 핸들러.
         * allowUserPrompts가 true일 때만 userPrompt 플래그를 설정한다.
         */
        private void onUserPrompt_Next()
        {
            if(allowUserPrompts)
                userPrompt = true;
        }

        /*
         * StartConversation
         * 기존 대화를 중단하고 큐를 비운 뒤, 새 대화를 시작한다.
         * StopConversation → Clear → Enqueue → 코루틴 시작 순서로 동작.
         */
        public Coroutine StartConversation(Conversation conversation)
        {
            StopConversation();
            conversationQueue.Clear();

            Enqueue(conversation);

            process = dialogueSystem.StartCoroutine(RunningConversation());

            return process;
        }

        /*
         * StopConversation
         * 실행 중인 대화 코루틴을 중단하고 process를 null로 초기화.
         */
        public void StopConversation()
        {
            if (!isRunning)
                return;

            dialogueSystem.StopCoroutine(process);
            process = null;
        }

        /*
         * RunningConversation (메인 대화 루프)
         * 큐가 빌 때까지 대화를 순서대로 처리한다.
         *
         * 한 줄 처리 흐름:
         *   1) 현재 대화가 끝에 도달했으면 큐에서 제거 후 다음 대화로
         *   2) 현재 줄을 파싱 (DialogueParser.Parse)
         *   3) LogicalLine(choice/if 등)이면 해당 로직 실행
         *   4) 대화가 있으면 Line_RunDialogue()
         *   5) 명령어가 있으면 LINE_RunCommands()
         *   6) 대화가 있었으면 WaitForUserInput() 후 화면 초기화
         *   7) TryAdvanceConversation()으로 진행 인덱스 증가
         */
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

                        dialogueSystem.OnSystemPrompt_Clear();
                    }
                }

                    TryAdvanceConversation(currentConversation);
                    isOnLogicalLine = false;
            }

            process = null;
    }

        /*
         * TryAdvanceConversation
         * 현재 대화의 진행 인덱스를 1 증가시킨다.
         * 큐 변경 경쟁 조건 체크:
         *   LogicalLine 처리 중에 큐가 변경될 수 있으므로,
         *   현재 대화가 여전히 큐의 top인지 확인 후 종료 여부를 처리한다.
         */
        private void TryAdvanceConversation(Conversation conversation)
        {
            conversation.IncrementProgress();

            // 큐가 변경되어 이 대화가 top이 아니면 Dequeue 하지 않음
            if (conversation != conversationQueue.top)
            {
                return;
            }
            if (conversation.HasReachedEnd())
            {
                conversationQueue.Dequeue();
            }
        }

        /*
         * Line_RunDialogue
         * 대사 한 줄을 실행하는 코루틴.
         *   1) 화자가 있으면 HandleSpeakerLogic()으로 캐릭터/이름/스타일 설정
         *   2) dialogueContainer가 보이지 않으면 Show()
         *   3) BuildLineSegments()로 세그먼트 순서대로 텍스트 출력
         */
        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            // Show or hide the speaker name if there is one present.
            if (line.hasSpeaker)
            {
                HandleSpeakerLogic(line.speakerData);
            }

            // 대화창이 숨겨져 있으면 표시 (첫 대화 시작 시, 또는 Hide() 후)
            if(!dialogueSystem.dialogueContainer.isVisible)
            {
                dialogueSystem.dialogueContainer.Show();
            }

            //build dialogueData
            yield return BuildLineSegments(line.dialogueData);
        }

        /*
         * HandleSpeakerLogic
         * 화자 데이터를 바탕으로 캐릭터 관련 동작을 처리한다.
         *
         * characterMustBeCreated 조건:
         *   enter 키워드, 표정 캐스팅, 위치 캐스팅 중 하나라도 있으면
         *   캐릭터가 없어도 새로 생성한다.
         *   (이름만 표시하는 경우라면 굳이 생성할 필요 없음)
         */
        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            bool characterMustBeCreated = (speakerData.makeCharacerEnter || speakerData.isCastingExpression || speakerData.isCastingPosition);

            Character character = CharacterManager.instance.GetCharacter(speakerData.name, createIfDoesNotExist: characterMustBeCreated);

            // enter 키워드가 있고 아직 표시 중이 아니면 캐릭터를 등장시킴
            if (speakerData.makeCharacerEnter && (!character.isVisible && !character.isRevealing))
            {
                character.Show();
            }

            //Add character name to the UI.
            // 화자 이름을 TagManager로 치환해 대화창 이름 UI에 표시
            dialogueSystem.ShowSpeakerName(TagManager.Inject(speakerData.displayname));

            //Now customize the dialogue for this character - if applicable.
            // 캐릭터 설정(폰트, 색상 등)을 대화창 UI에 반영
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

        /*
         * LINE_RunCommands
         * 명령어 목록을 순서대로 실행하는 코루틴.
         *
         * waitForCompletion = true 또는 명령어 이름이 "wait"이면:
         *   → 명령어가 완료될 때까지 대기 (CoroutineWrapper.IsDone 폴링)
         *   → 대기 중 userPrompt가 true이면 현재 명령어를 강제 중단
         * 그 외:
         *   → 명령어를 fire-and-forget 방식으로 즉시 실행하고 다음으로 진행
         */
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
                    CommandManager.instance.Execute(command.name, command.arguments); // fire-and-forget
            }
            yield return null;
        }

        /*
         * BuildLineSegments
         * DIALOGUE_DATA의 세그먼트 목록을 순서대로 처리한다.
         * 각 세그먼트마다:
         *   1) WaitForDialogueSegmentSignalToBeTriggered() 로 신호 대기
         *   2) StartSignal.N이면 텍스트 앞에 줄바꿈 추가
         *   3) BuildDialogue()로 실제 텍스트 출력
         */
        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            for (int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];

                yield return WaitForDialogueSegmentSignalToBeTriggered(segment);

                string dialogueText = segment.dialogue;

                // N 신호: 줄바꿈을 앞에 붙여 이어붙이기
                if (segment.startSignal == DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.N)
                {
                    dialogueText = "\n" + dialogueText;
                }

                yield return BuildDialogue(dialogueText, segment.appendText);
            }
        }

        // WC / WA 대기 타이머가 동작 중이면 true (AutoReader에서 참조)
        public bool isWaitingOnAutoTimer { get; private set; } = false;

        /*
         * WaitForDialogueSegmentSignalToBeTriggered
         * 세그먼트의 StartSignal에 따라 다음 세그먼트 시작 전 어떻게 대기할지 결정한다.
         *
         *   NONE : 신호 없음, 즉시 다음 세그먼트로 진행
         *   C    : 사용자 클릭 대기 → 클릭 후 텍스트 초기화(Clear) 신호 발송
         *   A    : 사용자 클릭 대기 → 클릭 후 이어붙이기 (Clear 없음)
         *   WC   : signalDelay 초 대기 → 타이머 완료 후 텍스트 초기화
         *   WA   : signalDelay 초 대기 → 타이머 완료 후 이어붙이기
         *   N    : 사용자 클릭 대기 → 클릭 후 줄바꿈 이어붙이기 (Clear 없음)
         *
         * WC / WA 대기 중에도 userPrompt가 true이면 즉시 대기를 종료한다.
         */
        IEnumerator WaitForDialogueSegmentSignalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                    yield return WaitForUserInput();
                    dialogueSystem.OnSystemPrompt_Clear();
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
                    dialogueSystem.OnSystemPrompt_Clear();
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

        /*
         * BuildDialogue
         * 실제 텍스트를 TextArchitect로 출력하는 코루틴.
         *
         * append = false → architect.Build()  (기존 텍스트 초기화 후 새로 출력)
         * append = true  → architect.Append() (기존 텍스트 뒤에 이어붙이기)
         *
         * 2단계 클릭으로 타이핑을 빠르게/완료:
         *   1클릭: hurryUp = true  → 타이핑 속도 최대로
         *   2클릭: ForceComplete() → 즉시 완성
         */
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
                        architect.hurryUp = true;   // 1클릭: 빠르게
                    else
                        architect.ForceComplete();  // 2클릭: 즉시 완성

                    userPrompt = false;
                }
                yield return null;

            }
        }

        /*
         * WaitForUserInput
         * 사용자 입력(클릭 등)을 기다리는 코루틴.
         * prompt(▶ 아이콘)를 표시한 뒤 userPrompt가 true가 될 때까지 대기하고,
         * 입력이 들어오면 prompt를 숨기고 userPrompt를 false로 소비한다.
         */
        IEnumerator WaitForUserInput()
        {
            dialogueSystem.prompt.Show(); // ▶ 아이콘 표시

            while (!userPrompt)
                yield return null;

            dialogueSystem.prompt.Hide(); // ▶ 아이콘 숨김

            userPrompt = false; // 신호 소비
        }
    }
}
