using System.Collections;

namespace DIALOGUE.LogicalLines
{
    /*
     * LL_Input
     * ─────────────────────────────────────────────────────────────────────
     * 사용자 텍스트 입력을 처리하는 논리 라인 구현체.
     * InputPanel을 표시하고 사용자가 입력을 완료할 때까지 대화 진행을 멈춘다.
     *
     * 대화 스크립트 문법:
     *   input "이름을 입력해주세요"
     *
     * 입력 후 활용:
     *   입력된 값은 InputPanel.lastInput에 저장되며,
     *   TagManager의 <input> 태그로 이후 대사에서 참조 가능.
     *   예) KIM "당신의 이름은 <input>이군요!"
     * ─────────────────────────────────────────────────────────────────────
     */
    public class LL_Input : ILogicalLine
    {
        // "input"이 화자(speaker) 이름 위치에 오면 이 핸들러가 처리
        public string keyword => "input";

        /*
         * Execute
         * InputPanel을 표시하고 사용자 입력이 완료될 때까지 대기한다.
         *
         * 흐름:
         *   1) line.dialogueData.rawData → 입력 창에 표시할 제목(title) 추출
         *   2) InputPanel.Show(title) → 입력 UI 활성화
         *   3) panel.isWaitingOnUserInput이 false가 될 때까지 yield return null
         *      (한 프레임씩 대기 → 사용자가 확인을 누르면 isWaitingOnUserInput = false)
         */
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string title = line.dialogueData.rawData; // 대사 부분을 입력 창 제목으로 사용
            InputPanel panel = InputPanel.instance;
            panel.Show(title);

            // 사용자가 입력을 완료할 때까지 매 프레임 대기
            while (panel.isWaitingOnUserInput)
            {
                yield return null;
            }
        }

        /*
         * Matches
         * 화자 이름이 "input"인 라인을 처리 대상으로 판별한다.
         * hasSpeaker: 화자 정보가 있는지 확인 (명령어만 있는 라인 등 제외)
         * speakerData.name.ToLower() == keyword: 대소문자 무관하게 "input" 감지
         */
        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.name.ToLower() == keyword);
        }
    }
}
