using System.Collections;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Conditions;

namespace DIALOGUE.LogicalLines
{
    /*
     * LL_Condition
     * ─────────────────────────────────────────────────────────────────────
     * "if(조건)" 형태의 조건 분기를 처리하는 논리 라인 구현체.
     * 조건이 참이면 if 블록을, 거짓이면 else 블록을 우선 실행한다.
     *
     * 대화 스크립트 문법:
     *   if($score >= 10)
     *   {
     *       KIM "합격입니다!"
     *   }
     *   else
     *   {
     *       KIM "불합격입니다."
     *   }
     *
     * else 블록은 선택 사항이다. 없으면 조건이 false일 때 아무것도 실행하지 않음.
     * ─────────────────────────────────────────────────────────────────────
     */
    public class LL_Condition : ILogicalLine
    {
        // "if"로 시작하는 라인을 처리 (Matches에서 rawData.StartsWith로 감지)
        public  string keyword => "if";

        // else 블록 구분 키워드
        private readonly string ELSE = "else";
        // if(조건) 에서 조건 추출 시 사용하는 여는/닫는 괄호 문자
        private readonly string[] CONTAINERS = new string[] {"(", ")"};

        /*
         * Execute
         * if/else 조건 분기를 실행하는 코루틴.
         *
         * 처리 흐름:
         *   1) ExtractCondition()으로 "if($score >= 10)" 에서 "$score >= 10" 추출
         *   2) EvaluateCondition()으로 조건 평가 (true/false)
         *   3) RipEncapsulationData()로 if { } 블록 내용 추출
         *   4) if 블록 바로 다음 줄이 "else"이면 else { } 블록도 추출
         *   5) SetProgress()로 if/else 전체 블록을 건너뛰도록 진행 인덱스 설정
         *      (else가 있으면 else 블록 끝, 없으면 if 블록 끝으로 점프)
         *   6) 조건에 맞는 블록(selData)이 있으면 EnqueuePriority로 우선 실행
         *
         * startingIndex += 2 이유:
         *   추출된 selData에는 헤더 줄("if(...)" 또는 "else")과 여는 "{" 줄이 포함되어 있음.
         *   2를 더해 이 두 줄을 건너뛰고 실제 실행할 줄부터 시작.
         * endingIndex -= 1 이유:
         *   닫는 "}" 줄을 제외하기 위함.
         */
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            // "if($score >= 10)" 에서 조건 문자열 "$score >= 10" 추출
            string rawCondition  = ExtractCondition (line.rawData.Trim());
            bool conditionResult = EvaluateCondition(rawCondition); // 조건 평가

            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.instance.conversationManager.conversationProgress;

            // if { } 블록 내용 추출 (헤더+{ } 줄 제외, false 전달)
            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, false, parentStartingIndex: currentConversation.fileStartIndex);
            EncapsulatedData elseData = new EncapsulatedData() ; // else 블록 없으면 isNull = true


            // if 블록 다음 줄이 "else"인지 확인
            if (ifData.endingIndex + 1 < currentConversation.Count)
            {
                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
                if(nextLine == ELSE)
                {
                    // "else" 다음 줄부터 else { } 블록 추출
                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false, parentStartingIndex: currentConversation.fileStartIndex);
                }
            }

            // if/else 전체 블록 이후 줄로 진행 인덱스를 점프
            // else가 있으면 else 블록 끝, 없으면 if 블록 끝으로 설정
            currentConversation.SetProgress(elseData.isNull ? ifData.endingIndex: elseData.endingIndex);

            // 조건에 맞는 블록 선택 (true면 if, false면 else)
            EncapsulatedData selData = conditionResult ? ifData : elseData;

            if (!selData.isNull && selData.lines.Count > 0)
            {
                //Remove the header and encapsulator lines from the conversation indexes
                selData.startingIndex +=2; // 헤더 줄("if(...)" 또는 "else")과 "{" 줄 제외
                selData.endingIndex -= 1;  // 닫는 "}" 줄 제외

                // 선택된 블록 내용을 새 Conversation으로 생성해 우선 실행
                Conversation newConversation = new Conversation(selData.lines, file: currentConversation.file, fileStartIndex: selData.startingIndex, fileEndIndex: selData.endingIndex);
                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
            }

            yield return null; // 코루틴 인터페이스 요건 충족 (실질적 대기 없음)
        }

        /*
         * Matches
         * 라인의 rawData가 "if"로 시작하는지 확인한다.
         * 화자(speakerData.name) 기준이 아닌 rawData 기준으로 감지하는 이유:
         *   "if($score >= 10)"는 화자 없이 시작하는 순수 명령 형태이기 때문.
         */
        public bool Matches(DIALOGUE_LINE line)
        {
            return line.rawData.Trim().StartsWith(keyword);
        }

        /*
         * ExtractCondition
         * "if($score >= 10)" 형태의 문자열에서 괄호 안의 조건 문자열만 추출한다.
         * IndexOf('(')와 IndexOf(')')의 위치를 찾아 그 사이 문자열을 반환.
         * 예) "if($score >= 10)" → "$score >= 10"
         */
        private string ExtractCondition(string line)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1; // '(' 다음 위치
            int endIndex = line.IndexOf(CONTAINERS[1]);       // ')' 위치

            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}


