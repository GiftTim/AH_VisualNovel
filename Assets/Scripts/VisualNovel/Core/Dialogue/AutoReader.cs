using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

        [SerializeField] private GameObject autoStopObject;
        [SerializeField] private GameObject autoPlayObject;
        [HideInInspector] public bool allowToggle = true;

        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;
        }

        public void Enable()
        {
            if (isOn)
                return;

            co_running = StartCoroutine(AutoRead());
        }

        public void Disable()
        {
            if (!isOn)
                return;

            StopCoroutine(co_running);
            skip = false;
            co_running = null;
        }

        private IEnumerator AutoRead()
        {
            //Do nothing if there is no conversation to monitor.
            if (!conversationManager.isRunning)
            {
                Disable();
                yield break;
            }

            if (!architect.isBuilding && architect.currentText != string.Empty)
                DialogueSystem.instance.OnSystemPrompt_Next();

            while (conversationManager.isRunning)
            {
                //Read and wait
                if (!skip)
                {
                    while (!architect.isBuilding && !conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    yield return new WaitForSeconds(0.02f);

                    float timeStarted = Time.time;

                    while (architect.isBuilding || conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    float timeToRead = Mathf.Clamp(((float)architect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND), MIN_READ_TIME, MAX_READ_TIME);
                    timeToRead = Mathf.Clamp((timeToRead - (Time.time - timeStarted)), MIN_READ_TIME, MAX_READ_TIME);
                    timeToRead = (timeToRead / speed) + READ_TIME_PADDING;

                    Debug.Log($"wait [{timeToRead}s] for '{architect.currentText}'");

                    yield return new WaitForSeconds(timeToRead);
                }
                //Skip
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