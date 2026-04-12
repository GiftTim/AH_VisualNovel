using System.Collections;
using UnityEngine;


namespace DIALOGUE
{
    /*
     * AutoReader
     * ─────────────────────────────────────────────────────────────────────
     * 두 가지 자동 진행 모드를 제공하는 컴포넌트.
     *
     *   Auto (자동 읽기):
     *     텍스트 출력이 완료된 후 읽기 시간을 계산해 자동으로 다음 줄로 진행.
     *     읽기 시간 = 문자 수 / DEFAULT_CHARACTERS_READ_PER_SECOND 기준 계산,
     *     텍스트 출력에 걸린 시간을 차감하고 READ_TIME_PADDING을 더함.
     *
     *   Skip (즉시 넘기기):
     *     ForceComplete()로 텍스트를 즉시 완성하고 0.05초 후 다음으로 진행.
     * ─────────────────────────────────────────────────────────────────────
     */
    public class AutoReader : MonoBehaviour
    {
        // 초당 읽을 수 있다고 가정하는 기본 문자 수 (읽기 대기 시간 계산 기준)
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 18;
        // 읽기 시간에 추가로 더하는 패딩 (읽기가 너무 빠르게 느껴지지 않도록)
        private const float READ_TIME_PADDING = 0.5f;
        // 읽기 대기 시간 최대값 (아무리 긴 텍스트도 이 시간을 넘지 않음)
        private const float MAX_READ_TIME = 99f;
        // 읽기 대기 시간 최소값 (짧은 텍스트도 최소 이 시간만큼 대기)
        private const float MIN_READ_TIME = 1f;

        private ConversationManager conversationManager;
        // TextArchitect 접근 단축 프로퍼티
        private TextArchitect architect => conversationManager.architect;

        // Skip 모드 여부 (true이면 즉시 넘기기, false이면 자동 읽기)
        public bool skip { get; set; } = false;
        // 읽기 속도 배율 (1f = 기본 속도, 2f = 2배 빠름)
        public float speed { get; set; } = 1f;

        // AutoRead 코루틴이 실행 중이면 true (co_running != null)
        public bool isOn => co_running != null;
        // 실행 중인 AutoRead 코루틴 참조 (null이면 비활성)
        private Coroutine co_running = null;

        // Auto가 켜졌을 때 표시할 UI 오브젝트 (Auto 표시)
        [SerializeField] private GameObject autoStopObject;
        // Auto가 꺼졌을 때 표시할 UI 오브젝트 (Stop 표시)
        [SerializeField] private GameObject autoPlayObject;
        /*
         * allowToggle
         * false이면 Toggle_Auto(), Toggle_Skip() 호출이 무시된다.
         * 히스토리 보기 중에 Auto/Skip 모드 전환을 막기 위해 사용.
         */
        [HideInInspector] public bool allowToggle = true;

        // ConversationManager 주입 (DialogueSystem.Initialize()에서 호출)
        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;
        }

        /*
         * Enable
         * AutoRead 코루틴을 시작한다.
         * 이미 실행 중이면 무시.
         */
        public void Enable()
        {
            if (isOn)
                return;

            co_running = StartCoroutine(AutoRead());
        }

        /*
         * Disable
         * AutoRead 코루틴을 중단하고 상태를 초기화한다.
         * skip도 false로 리셋해 다음 Enable 시 자동 읽기 모드로 시작.
         */
        public void Disable()
        {
            if (!isOn)
                return;

            StopCoroutine(co_running);
            skip = false;
            co_running = null;
        }

        /*
         * AutoRead (메인 자동 읽기 루프)
         *
         * [skip = false 경우 - 자동 읽기]
         *   1) 텍스트 출력 중이거나 타이머 대기 중이면 대기
         *   2) 완료 시점의 Time.time을 기록 (timeStarted)
         *   3) 텍스트 출력 완료 및 타이머 종료까지 대기
         *   4) 읽기 시간 계산: 문자 수 / 초당 읽기 속도
         *   5) 텍스트 출력에 걸린 시간(Time.time - timeStarted)을 차감
         *      → 보정 이유: 출력 시간이 이미 읽기 시간에 포함되어 있으므로,
         *        제거하면 빠른 대사도 최소 MIN_READ_TIME(1초) 대기함.
         *   6) WaitForSeconds 후 OnSystemPrompt_Next()로 다음 진행
         *
         * [skip = true 경우 - 즉시 넘기기]
         *   ForceComplete() → 0.05초 대기 → OnSystemPrompt_Next()
         */
        private IEnumerator AutoRead()
        {
            //Do nothing if there is no conversation to monitor.
            if (!conversationManager.isRunning)
            {
                Disable();
                yield break;
            }

            // 이미 텍스트 출력이 완료된 상태라면 즉시 다음으로 진행
            if (!architect.isBuilding && architect.currentText != string.Empty)
                DialogueSystem.instance.OnSystemPrompt_Next();

            while (conversationManager.isRunning)
            {
                //Read and wait
                if (!skip)
                {
                    // 텍스트 출력이 시작되거나 타이머 대기가 시작될 때까지 대기
                    while (!architect.isBuilding && !conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    yield return new WaitForSeconds(0.02f);

                    // 텍스트 출력 시작 시점 기록 (읽기 시간 보정에 사용)
                    float timeStarted = Time.time;

                    // 텍스트 출력 완료 및 타이머 종료까지 대기
                    while (architect.isBuilding || conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    // 읽기 대기 시간 계산
                    float timeToRead = Mathf.Clamp(((float)architect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND), MIN_READ_TIME, MAX_READ_TIME);
                    // 텍스트 출력에 걸린 시간을 읽기 대기에서 차감 (이미 그 시간동안 읽었으므로)
                    timeToRead = Mathf.Clamp((timeToRead - (Time.time - timeStarted)), MIN_READ_TIME, MAX_READ_TIME);
                    timeToRead = (timeToRead / speed) + READ_TIME_PADDING;

                    Debug.Log($"wait [{timeToRead}s] for '{architect.currentText}'");

                    yield return new WaitForSeconds(timeToRead);
                }
                //Skip
                else
                {
                    // 텍스트를 즉시 완성하고 짧게 대기 후 다음으로 진행
                    architect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }

                DialogueSystem.instance.OnSystemPrompt_Next();
            }

            Disable();
        }

        /*
         * Toggle_Auto
         * 3가지 상태를 순환한다:
         *   Skip 중      → Auto로 전환 (skip = false, Enable)
         *   Auto 꺼짐    → Auto 켜기 (Enable)
         *   Auto 켜짐    → Auto 끄기 (Disable)
         */
        public void Toggle_Auto()
        {
            if (!allowToggle)
                return;

            bool prevState = skip;
            skip = false;

            if (prevState)
            {
                // Skip → Auto 전환: Auto를 켠다
                if (autoStopObject != null) autoStopObject.SetActive(false);
                if (autoPlayObject != null)
                {
                    autoPlayObject.SetActive(true);
                    autoPlayObject.GetComponent<Animator>().Play("Enter");
                }
                Enable();
            }
            else
            {
                if (!isOn)
                {
                    // Auto 꺼진 상태 → Auto 켠다
                    if (autoStopObject != null) autoStopObject.SetActive(false);
                    if (autoPlayObject != null)
                    {
                        autoPlayObject.SetActive(true);
                        autoPlayObject.GetComponent<Animator>().Play("Enter");
                    }
                    Enable();
                }
                else
                {
                    // Auto 켜진 상태 → Auto 끈다
                    if (autoPlayObject != null) autoPlayObject.SetActive(false);
                    if (autoStopObject != null) autoStopObject.SetActive(true);
                    Disable();
                }
            }
        }

        /*
         * Toggle_Skip
         * 2가지 상태를 전환한다:
         *   Skip 꺼짐 → Skip 켜기 (skip = true, Enable)
         *   Skip 켜짐 → Skip 끄기 (Disable)
         */
        public void Toggle_Skip()
        {
            if (!allowToggle)
                return;

            bool prevState = skip;
            skip = true;

            if (!prevState)
                Enable();

            else
            {
                if (!isOn)
                    Enable();
                else
                    Disable();
            }
        }
    }
}
