using System.Collections.Generic;

namespace DIALOGUE
{
    /*
     * Conversation
     * ─────────────────────────────────────────────────────────────────────
     * 하나의 대화 시퀀스(여러 줄의 텍스트)와 진행 상태를 함께 관리하는 클래스.
     * ConversationManager는 이 클래스를 통해 현재 어느 줄까지 진행했는지 추적한다.
     *
     * file / fileStartIndex / fileEndIndex 는 세이브 데이터 압축 시
     * 전체 줄 내용 대신 파일 경로와 인덱스 범위만 저장하기 위한 정보다.
     * ─────────────────────────────────────────────────────────────────────
     */
    public class Conversation
    {
        // 대화 줄 목록
        private List<string> lines = new List<string>();
        // 현재 진행 인덱스 (0부터 시작, HasReachedEnd()로 종료 여부 확인)
        private int progress = 0;

        // 이 대화가 로드된 파일 경로 (인라인 대화면 빈 문자열)
        public string file { get; private set; }
        // 파일에서 이 대화가 시작하는 줄 인덱스 (세이브 압축용)
        public int fileStartIndex { get; private set; }
        // 파일에서 이 대화가 끝나는 줄 인덱스 (세이브 압축용)
        public int fileEndIndex { get; private set; }

        /*
         * 생성자
         * fileStartIndex / fileEndIndex 기본값이 -1이면 자동으로 계산:
         *   fileStartIndex = 0, fileEndIndex = lines.Count - 1
         * 파일 전체를 하나의 대화로 사용할 때 편리하다.
         */
        public Conversation(List<string> lines, int progress = 0, string file = "", int fileStartIndex = -1, int fileEndIndex = -1)
        {
            this.lines = lines;
            this.progress = progress;
            this.file = file;

            // -1이면 전체 줄을 대상으로 자동 설정
            if (fileStartIndex == -1)
            {
                fileStartIndex = 0;
            }

            if (fileEndIndex == -1)
            {
                fileEndIndex = lines.Count - 1;
            }

            this.fileStartIndex = fileStartIndex;
            this.fileEndIndex = fileEndIndex;
        }

        // 현재 진행 인덱스 반환
        public int GetProgress() => progress;
        // 진행 인덱스 직접 설정 (세이브 로드 등에서 사용)
        public void SetProgress(int value) => progress = value;
        // 진행 인덱스 1 증가 (한 줄 처리 완료 후 호출)
        public void IncrementProgress() => progress++;
        // 전체 줄 수
        public int Count => lines.Count;
        // 줄 목록 전체 반환
        public List<string> GetLines() => lines;
        // 현재 진행 중인 줄 텍스트 반환
        public string CurrentLine() => lines[progress];
        // 모든 줄을 처리했으면 true
        public bool HasReachedEnd() => progress >= lines.Count;

    }
}
