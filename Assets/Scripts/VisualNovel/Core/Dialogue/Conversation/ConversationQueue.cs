using System.Collections.Generic;

namespace DIALOGUE
{
    /*
     * ConversationQueue
     * ─────────────────────────────────────────────────────────────────────
     * Queue<Conversation>의 래퍼 클래스.
     * 기본 Queue 기능에 더해 '우선 삽입(Priority Enqueue)' 기능을 제공한다.
     *
     * 사용 흐름:
     *   - Enqueue()         : 대화를 맨 뒤에 추가 (일반적인 순서 추가)
     *   - EnqueuePriority() : 대화를 맨 앞에 추가 (즉시 실행 필요 시)
     *   - top               : 현재 진행 중인(가장 앞의) 대화 조회
     *   - Dequeue()         : 완료된 대화를 큐에서 제거
     * ─────────────────────────────────────────────────────────────────────
     */
    public class ConversationQueue
    {
        private Queue<Conversation> conversationQueue = new Queue<Conversation>();

        // 큐의 가장 앞 대화를 제거 없이 조회 (Peek 사용)
        // 큐가 비어있으면 InvalidOperationException이 발생하므로 IsEmpty() 확인 후 호출할 것
        public Conversation top => conversationQueue.Peek();

        // 대화를 큐 맨 뒤에 추가 (정상적인 순서 추가)
        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);

        /*
         * EnqueuePriority
         * 대화를 큐 맨 앞에 삽입한다.
         * Queue는 맨 앞 삽입을 직접 지원하지 않으므로, 새 Queue를 생성해
         * 새 대화를 먼저 넣은 뒤 기존 대화들을 순서대로 이어 붙이는 방식을 사용한다.
         */
        public void EnqueuePriority(Conversation conversation)
        {
            Queue<Conversation> queue = new Queue<Conversation>();
            queue.Enqueue(conversation); // 새 대화를 맨 앞에

            // 기존 대화들을 순서를 유지하며 뒤에 추가
            while (conversationQueue.Count > 0)
            {
                queue.Enqueue(conversationQueue.Dequeue());
            }
            conversationQueue = queue;
        }

        // 큐에서 가장 앞 대화를 제거 (완료된 대화 처리 후 호출)
        public void Dequeue()
        {
            if(conversationQueue.Count > 0)
            {
                conversationQueue.Dequeue();
            }
        }

        // 큐가 비어있으면 true
        public bool IsEmpty() => conversationQueue.Count == 0;

        // 큐 전체 비우기
        public void Clear() => conversationQueue.Clear();

        /*
         * GetReadOnly
         * 현재 큐의 스냅샷을 배열로 반환한다.
         * ToArray()는 내부 배열의 복사본을 반환하므로 외부에서 수정해도 큐에 영향 없음.
         */
        public Conversation[] GetReadOnly() => conversationQueue.ToArray();

    }
}
