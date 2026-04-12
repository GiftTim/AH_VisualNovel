using System.Collections;

namespace DIALOGUE.LogicalLines
{
    /*
     * ILogicalLine
     * ─────────────────────────────────────────────────────────────────────
     * 모든 '논리 라인(Logical Line)' 처리기의 공통 계약(Contract)을 정의하는 인터페이스.
     *
     * 논리 라인이란?
     *   대화 스크립트에서 단순 대사(dialogue)나 명령어(command)가 아닌,
     *   조건 분기(if/else), 선택지(choice), 입력(input), 변수 연산($var = ...)
     *   과 같이 게임 흐름을 제어하는 특수 라인을 말한다.
     *
     * 사용 흐름:
     *   ConversationManager.RunningConversation()
     *     → LogicalLineManager.TryGetLogic(line)
     *       → 등록된 ILogicalLine 구현체 중 Matches()가 true인 것을 찾아
     *         Execute()를 코루틴으로 실행
     *
     * 새 논리 라인을 추가하려면:
     *   1) 이 인터페이스를 구현하는 클래스(예: LL_XXX)를 생성
     *   2) LogicalLineManager가 Reflection으로 자동 탐지하므로 별도 등록 불필요
     * ─────────────────────────────────────────────────────────────────────
     */
    public interface ILogicalLine
    {
        // 이 논리 라인을 식별하는 키워드 문자열.
        // Matches()에서 키워드 비교에 사용하거나, regex 기반 감지 시 미구현(NotImplementedException)으로 남길 수 있음.
        string keyword { get; }

        /*
         * Matches
         * 이 구현체가 주어진 DIALOGUE_LINE을 처리할 수 있는지 판단한다.
         * ConversationManager가 각 줄마다 이 메서드를 호출해 적합한 핸들러를 찾는다.
         * 판단 책임을 구현체에 위임함으로써, 키워드 비교·regex 감지 등 다양한 방식을 지원.
         */
        bool Matches(DIALOGUE_LINE line);

        /*
         * Execute
         * 논리 라인의 실제 로직을 실행하는 코루틴.
         * IEnumerator를 반환해 Unity 코루틴으로 실행 → yield return으로 프레임 대기 가능.
         * (예: 사용자 입력 대기, 패널 표시 대기 등 비동기 처리)
         */
        IEnumerator Execute(DIALOGUE_LINE line);
    }
}
