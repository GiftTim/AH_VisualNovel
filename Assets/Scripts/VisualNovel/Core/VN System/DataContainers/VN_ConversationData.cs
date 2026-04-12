using System.Collections.Generic;

namespace VISUALNOVEL
{
    /*
     * ============================================================
     * VN_ConversationData (Visual Novel Conversation Data - Full)
     * ============================================================
     * 대화(Conversation) 하나를 세이브 파일에 저장하기 위한
     * "전체 저장" 포맷 데이터 컨테이너.
     *
     * [언제 사용하나?]
     * - 대화가 파일(.txt)에 기반하지 않는 경우.
     *   예) choice 명령어 결과로 동적으로 생성된 대화.
     * - 파일 경로가 없으므로 대화 라인 내용 전체를 직접 저장해야 한다.
     *
     * [VN_ConversationDataCompressed와의 차이]
     * - 이 클래스: 대화 라인 전체를 저장 → 파일이 없어도 복원 가능.
     * - Compressed: 파일명+인덱스만 저장 → 용량 절약, 파일 필요.
     *
     * [직렬화]
     * - [System.Serializable] 어트리뷰트로 JsonUtility가 직렬화 가능.
     * - VNGameSave.GetConversationData()에서 JsonUtility.ToJson()으로 변환.
     * - VNGameSave.SetConversationData()에서 JsonUtility.FromJson()으로 복원.
     * ============================================================
     */
    [System.Serializable]
    public class VN_ConversationData
    {
        // ─────────────────────────────────────────────────────────
        // conversation: 대화를 구성하는 텍스트 라인 목록.
        //   각 원소는 대화 스크립트 한 줄에 해당한다.
        //   예) ["Hiro \"안녕하세요.\"", "choice \"어떻게 할까?\" { ... }"]
        // ─────────────────────────────────────────────────────────
        public List<string> conversation = new List<string>();

        // ─────────────────────────────────────────────────────────
        // progress: 저장 시점에 몇 번째 라인까지 진행되었는지 저장하는 인덱스.
        //   로드 시 이 위치부터 대화를 재개한다.
        //   0이면 처음부터, 3이면 4번째 줄(인덱스 3)부터 시작.
        // ─────────────────────────────────────────────────────────
        public int progress;
    }
}
