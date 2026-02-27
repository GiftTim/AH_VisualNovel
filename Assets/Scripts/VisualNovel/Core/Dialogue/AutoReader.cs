using UnityEngine;
using System.Collections;

namespace DIALOGUE
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 18;
        private const float READ_TIME_PADDING = 0.5f;
        private const float MAX_READ_TIME = 99f;
        private const float MIN_READ_TIME = 1f;

        private ConversationManager conversationManager;
        private TextArchitect architect => conversationManager.architect;

        public bool skip { get; set; } = false;
        public float speed { get; set; } = 1f;
        public bool isOn => co_running != null;
        private Coroutine co_running = null;

        [Header("Auto UI")]
        [SerializeField] private GameObject autoStop;     // Auto-Stop 오브젝트
        [SerializeField] private GameObject autoPlay;     // Auto-Play 오브젝트
        [SerializeField] private Animator autoPlayAnimator; // Auto-Play에 붙은 Animator
        [SerializeField] private string autoPlayStateName = "Play_Auto";

        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;

            // Animator 자동 연결(Inspector에 안 넣었을 때 대비)
            if (autoPlayAnimator == null && autoPlay != null)
                autoPlayAnimator = autoPlay.GetComponent<Animator>();

            SetAutoVisual(false);
        }

        public void Enable()
        {
            if (isOn) return;
            co_running = StartCoroutine(AutoRead());

            // Enable은 Skip에서도 호출되므로, 실제 표시 여부는 아래 SetAutoVisual에서 skip 고려
            SetAutoVisual(isOn && !skip);
        }

        public void Disable()
        {
            if (!isOn)
            {
                return;
            }

            StopCoroutine(co_running);
            skip = false;
            co_running = null;

            SetAutoVisual(false);
        }

        private IEnumerator AutoRead()
        {
            if (!conversationManager.isRunning)
            {
                Disable();
                yield break;
            }

            if (!architect.isBuilding && architect.currentText != string.Empty)
                DialogueSystem.instance.OnSystemPrompt_Next();

            while (conversationManager.isRunning)
            {
                if (!skip)
                {
                    while (!architect.isBuilding && !conversationManager.isWaitingOnSegmentTimer)
                    {
                        yield return null;
                    }

                    float timeStarted = Time.time;

                    while (architect.isBuilding || conversationManager.isWaitingOnSegmentTimer)
                    {
                        yield return null;
                    }


                    // (기존 코드의 Clamp 오타 수정: MAX_READ_TIME로)
                    float timeToRead = Mathf.Clamp(
                        (float)architect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND,
                        MIN_READ_TIME,
                        MAX_READ_TIME
                    );

                    timeToRead = Mathf.Clamp((timeToRead - (Time.time - timeStarted)), MIN_READ_TIME, MAX_READ_TIME);
                    timeToRead = (timeToRead / speed) + READ_TIME_PADDING;

                    yield return new WaitForSeconds(timeToRead);
                }
                else
                {
                    architect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }

                DialogueSystem.instance.OnSystemPrompt_Next();
            }

            Disable();
        }

        public void Toggle_Auto()
        {
            if (skip)
            {
                Enable();
            }
            else
            {
                if (!isOn) Enable();
                else Disable();
            }

            skip = false;

            // Auto 모드 ON일 때만 Auto-Play 표시 + 애니 재생
            SetAutoVisual(isOn && !skip);
        }

        public void Toggle_Skip()
        {
            skip = true;
            
            if (!skip)
            {
                Enable();
            }
            else
            {
                if (!isOn)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }

            // Skip일 때는 Auto-Play 애니 안 돌리고 Auto-Stop 상태로 표시(원하면 별도 Skip UI 추가)
            SetAutoVisual(isOn && !skip);
        }

        private void SetAutoVisual(bool autoPlaying)
        {
            if (autoStop != null) autoStop.SetActive(!autoPlaying);
            if (autoPlay != null) autoPlay.SetActive(autoPlaying);

            if (autoPlaying && autoPlayAnimator != null)
            {
                // 클릭할 때마다/켜질 때마다 처음부터 재생
                autoPlayAnimator.Play(autoPlayStateName, 0, 0f);
            }
        }
    }
}