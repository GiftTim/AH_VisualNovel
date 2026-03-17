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
        private const string STATUS_TEXT_AUTO = "Auto";
        private const string STATUS_TEXT_SKIP = "Skipping";

        private ConversationManager conversationManager;
        private TextArchitect architect => conversationManager.architect;

        public bool skip { get; set; } = false;
        public float speed { get; set; } = 1f;
        
        public bool isOn => co_running != null;
        private Coroutine co_running = null;

        [Header("Auto UI")]
        [SerializeField] private GameObject autoStop;     // Auto-Stop ������Ʈ
        [SerializeField] private GameObject autoPlay;     // Auto-Play ������Ʈ
        [SerializeField] private Animator autoPlayAnimator; // Auto-Play�� ���� Animator
        [SerializeField] private string autoPlayStateName = "Play_Auto";

        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;

            // Animator �ڵ� ����(Inspector�� �� �־��� �� ���)
            if (autoPlayAnimator == null && autoPlay != null)
                autoPlayAnimator = autoPlay.GetComponent<Animator>();

            SetAutoVisual(false);
        }

        public void Enable()
        {
            if (isOn) return;
            co_running = StartCoroutine(AutoRead());

            // Enable�� Skip������ ȣ��ǹǷ�, ���� ǥ�� ���δ� �Ʒ� SetAutoVisual���� skip ����
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


                    // (���� �ڵ��� Clamp ��Ÿ ����: MAX_READ_TIME��)
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
                if (!isOn) 
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }

            statusText.text = STATUS_TEXT_AUTO;

            skip = false;

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

            // Skip�� ���� Auto-Play �ִ� �� ������ Auto-Stop ���·� ǥ��(���ϸ� ���� Skip UI �߰�)
            SetAutoVisual(isOn && !skip);
        }

        private void SetAutoVisual(bool autoPlaying)
        {
            if (autoStop != null) autoStop.SetActive(!autoPlaying);
            if (autoPlay != null) autoPlay.SetActive(autoPlaying);

            if (autoPlaying && autoPlayAnimator != null)
            {
                // Ŭ���� ������/���� ������ ó������ ���
                autoPlayAnimator.Play(autoPlayStateName, 0, 0f);
            }
        }
    }
}