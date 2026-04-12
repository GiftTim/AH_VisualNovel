using System.Collections.Generic;
using System.Collections;
using System.Linq;


using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;

namespace DIALOGUE.LogicalLines
{
    /*
     * LL_Choice
     * ─────────────────────────────────────────────────────────────────────
     * "choice" 키워드로 분기 선택지를 처리하는 논리 라인 구현체.
     * { } 블록 내 '-' 로 시작하는 줄을 선택지로 파싱해 ChoicePanel에 표시하고,
     * 사용자가 선택하면 해당 결과 줄을 우선 큐(EnqueuePriority)에 삽입한다.
     *
     * 대화 스크립트 문법:
     *   choice "어떻게 할까요?"
     *   {
     *       - 선택지 A
     *         KIM "A를 선택했습니다"
     *       - 선택지 B
     *         KIM "B를 선택했습니다"
     *   }
     *
     * 주의: '-' 줄 자체는 선택지 제목이 되고, 그 다음 줄부터 다음 '-' 전까지가 결과 줄.
     * ─────────────────────────────────────────────────────────────────────
     */
    public class LL_Choice : ILogicalLine
    {
        // "choice"가 화자(speaker) 이름 위치에 오면 이 핸들러가 처리
        public  string     keyword => "choice";

        // '-' 로 시작하는 줄이 선택지 항목의 시작을 나타내는 식별자
        private const char CHOICE_IDENTIFIER = '-';

        /*
         * Execute
         * choice 블록 전체를 파싱해 ChoicePanel을 표시하고,
         * 사용자의 선택 결과를 우선 큐에 삽입한다.
         *
         * 처리 흐름:
         *   1) RipEncapsulationData()로 choice { } 블록 전체 추출
         *      (ripHeaderAndEncapsulators: true → "choice ..." 헤더와 { } 줄 포함)
         *   2) GetChoicesFromData()로 '-' 기준 Choice 목록 파싱
         *   3) ChoicePanel.Show(제목, 선택지 배열)로 UI 표시
         *   4) panel.isWaitingOnUserChoice가 false가 될 때까지 yield return null로 대기
         *      (사용자가 선택하면 ChoicePanel이 플래그를 변경)
         *   5) 선택된 Choice.resultLines로 새 Conversation 생성
         *   6) SetProgress()로 현재 대화 진행 인덱스를 블록 끝(data.endingIndex)으로 점프
         *      (choice 블록 전체를 건너뛰기 위해)
         *   7) EnqueuePriority()로 선택 결과를 현재 대화보다 먼저 실행
         */
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            var currentConversation = DialogueSystem.instance.conversationManager.conversation;
            var progress = DialogueSystem.instance.conversationManager.conversationProgress;

            // choice { } 블록 전체 추출 (헤더+{ } 줄 포함)
            EncapsulatedData data = RipEncapsulationData(currentConversation, progress, ripHeaderAndEncapsulators: true, parentStartingIndex: currentConversation.fileStartIndex);

            // '-' 기준으로 선택지 목록 파싱
            List<Choice> choice = GetChoicesFromData(data);

            string title = line.dialogueData.rawData; // "choice" 뒤의 대사가 선택지 제목
            ChoicePanel panel = ChoicePanel.instance;
            string[] choiceTitles = choice.Select(c => c.title).ToArray(); // 각 선택지 제목 배열

            panel.Show(title, choiceTitles); // ChoicePanel 표시

            // 사용자가 선택을 완료할 때까지 매 프레임 대기
            while (panel.isWaitingOnUserChoice)
            {
                yield return null;
            }

            // 사용자가 선택한 Choice 가져오기
            Choice selectedChoice = choice[panel.lastDecision.answerIndex];

            // 선택된 결과 줄을 새 Conversation으로 생성
            Conversation newConversation = new Conversation(selectedChoice.resultLines, file: currentConversation.file, fileStartIndex: selectedChoice.startIndex, fileEndIndex: selectedChoice.endIndex);

            // choice 블록 전체를 건너뛰도록 진행 인덱스를 블록 끝으로 점프
            DialogueSystem.instance.conversationManager.conversation.SetProgress(data.endingIndex - currentConversation.fileStartIndex);

            // 선택 결과를 현재 대화보다 먼저 실행 (우선 큐에 삽입)
            DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);

            AutoReader autoReader = DialogueSystem.instance.autoReader; // [미사용 변수] 향후 확장용으로 남겨둠

        }

        /*
         * Matches
         * 화자 이름이 "choice"인 라인을 처리 대상으로 판별한다.
         */
        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.name.ToLower() == keyword);
        }


        /*
         * GetChoicesFromData
         * EncapsulatedData에서 '-' 로 시작하는 줄을 기준으로 Choice 목록을 파싱한다.
         *
         * 파싱 흐름:
         *   - data.lines[0]은 "choice ..." 헤더 줄이므로 i=1부터 시작
         *   - '-' 로 시작하는 줄(encapsulationDepth==1 일 때)이 새 선택지의 시작
         *   - encapsulationDepth == 1 조건: 중첩 블록 안의 '-' 는 선택지가 아님
         *   - isFirstChoice 플래그: 첫 '-' 이전에 이전 선택지를 닫지 않기 위한 보호
         *   - startIndex / endIndex: 세이브/로드 시 파일 기준 절대 인덱스 계산에 사용
         *   - 루프 종료 후 마지막 선택지를 직접 추가 (루프 안에서 다음 '-' 를 만날 때만 추가하므로)
         */
        private List<Choice> GetChoicesFromData(EncapsulatedData data)
        {
            List<Choice> choices = new List<Choice>();
            int encapsulationDepth = 0;
            bool isFirstChoice = true; // 첫 번째 선택지 이전에 이전 선택지를 닫지 않기 위한 플래그

            Choice choice = new Choice
            {
                title = string.Empty,
                resultLines = new List<string>()
            };

            int choiceIndex = 0, i = 0;
            for (i=1;i<data.lines.Count;i++) // i=1: data.lines[0]은 "choice ..." 헤더 줄
            {
                var line = data.lines[i];

                // '-' 로 시작하고 현재 블록 깊이가 1 (최외각 블록)인 경우만 새 선택지
                if (IsChoiceStart(line) && encapsulationDepth == 1)
                {
                    if (!isFirstChoice)
                    {
                        // 이전 선택지의 인덱스 범위를 기록하고 목록에 추가
                        choice.startIndex = data.startingIndex + (choiceIndex+1);
                        choice.endIndex = data.startingIndex + (i - 1);

                        choices.Add(choice);
                        // 새 선택지를 위한 Choice 초기화
                        choice = new Choice
                        {
                            title = string.Empty,
                            resultLines = new List<string>()
                        };
                    }

                    choiceIndex = i;
                    choice.title = line.Trim().Substring(1); // '-' 제거해 선택지 제목 추출
                    isFirstChoice = false;
                    continue;
                }

                // '-' 가 아닌 줄은 현재 선택지의 결과 줄로 추가 (중첩 블록 처리 포함)
                AddLineToResults(line, ref choice, ref encapsulationDepth);

            }

            // 루프 종료 후 마지막 선택지가 목록에 없으면 추가
            if(!choices.Contains(choice))
            {
                choice.startIndex = data.startingIndex + (choiceIndex + 1);
                choice.endIndex = data.startingIndex + (i - 2);
                choices.Add(choice);
            }

            return choices;
        }

        /*
         * AddLineToResults
         * 중첩 블록 처리를 포함해 줄을 현재 선택지의 resultLines에 추가한다.
         *
         * 처리 규칙:
         *   - IsEncapsulationStart('{') : encapsulationDepth 증가
         *     → depth > 0 (최외각 블록 안)이면 '{' 줄도 resultLines에 포함
         *     → depth == 0 (최외각 '{') 일 때는 제외 (choice 블록 자체의 '{')
         *   - IsEncapsulationEnd('}') : encapsulationDepth 감소
         *     → depth > 0 이면 '}' 줄도 resultLines에 포함 (내부 블록의 '}')
         *     → depth == 0 이면 제외 (choice 블록 자체의 '}')
         *   - 그 외 일반 줄: resultLines에 그대로 추가
         */
        private void AddLineToResults(string line, ref Choice choice, ref int encapsulationDepth)
        {
            line = line.Trim();

            if (IsEncapsulationStart(line))
            {
                if (encapsulationDepth > 0)
                {
                    // 내부 중첩 블록의 '{' → 결과에 포함
                    choice.resultLines.Add(line);
                }

                encapsulationDepth++; // 블록 깊이 증가
                return;
            }

            if (IsEncapsulationEnd(line))
            {
                encapsulationDepth--; // 블록 깊이 감소

                if (encapsulationDepth > 0)
                {
                    // 내부 중첩 블록의 '}' → 결과에 포함
                    choice.resultLines.Add(line);
                }

                return;
                // encapsulationDepth == 0이면 최외각 '}' → 결과에서 제외
            }

            // 일반 줄은 결과에 추가
            choice.resultLines.Add(line);
        }

        // '-' 로 시작하는 줄인지 판별 (선택지 항목 구분자 감지)
        private bool IsChoiceStart(string line) => line.Trim().StartsWith(CHOICE_IDENTIFIER);



        /*
         * Choice 구조체
         * 하나의 선택지 항목 데이터를 담는 구조체.
         *   title       : '-' 뒤의 선택지 텍스트 (ChoicePanel에 표시)
         *   resultLines : 이 선택지를 골랐을 때 실행할 대화 줄 목록
         *   startIndex  : 파일 기준 이 선택지 결과의 시작 줄 인덱스 (세이브용)
         *   endIndex    : 파일 기준 이 선택지 결과의 끝 줄 인덱스 (세이브용)
         */
        private struct Choice
        {
            public string title;
            public List<string> resultLines;
            public int startIndex;
            public int endIndex;


        }
    }
}
