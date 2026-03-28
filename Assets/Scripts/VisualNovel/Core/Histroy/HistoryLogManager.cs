using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace History
{

    public class HistoryLogManager : MonoBehaviour
    {
        private const float LOG_STARTING_HEIGHT = 2f;
        private const float LOG_HEIGHT_PER_LINE = 2f;
        private const float LOG_DEFAULT_HEIGHT = 1f;
        private const float TEXT_DEFAULT_SCALE = 1f;

        private const string NAME_TEXT_NAME = "NameText";
        private const string DIALOGUE_TEXT_NAME = "DialogueText";

        private float logScaling = 1f;

        [SerializeField] private Animator anim;
        [SerializeField] private GameObject logPrefab;

        HistoryManager manager => HistoryManager.instance;
        private List<HistoryLog> logs = new List<HistoryLog>();

        public bool isOpen {get; private set;} = false;
        [SerializeField] private Slider logScaleSlider;

        private float textScaling => logScaling * 3f;

        public void Open()
        {
            if (isOpen) 
                return;

            anim.Play("LogOpen");
            isOpen = true;
        }

        public void Close()
        {
            if (!isOpen) 
                return;

            anim.Play("LogClose");
            isOpen = false;
        }
    
        public void AddLog(HistoryState state)
        {
            if(logs.Count >= HistoryManager.HISTORY_CACHE_LIMIT)
            {
                DestroyImmediate(logs[0].container);
                logs.RemoveAt(0);
            }
            
            CreateLog(state);
        }

        private void CreateLog(HistoryState state)
        {
            HistoryLog log = new HistoryLog();
            log.container = Instantiate(logPrefab, logPrefab.transform.parent);
            log.container.SetActive(true);

            log.nameText     = log.container.transform.Find(NAME_TEXT_NAME).GetComponent<TextMeshProUGUI>();
            log.dialogueText = log.container.transform.Find(DIALOGUE_TEXT_NAME).GetComponent<TextMeshProUGUI>();
        
            if(state.dialogue.currentSpeaker == string.Empty)
            {
                log.nameText.text = string.Empty;
            }
            else
            {
                log.nameText.text = state.dialogue.currentSpeaker;
                log.nameText.font = HistoryCache.LoadFont(state.dialogue.speakerFont);
                log.nameText.color = state.dialogue.speakerNameColor;
                log.nameFontSize = TEXT_DEFAULT_SCALE * state.dialogue.speakerScale;
                log.nameText.fontSize = log.nameFontSize +textScaling;
           }
        
            log.dialogueText.text = state.dialogue.currentDialogue;
            log.dialogueText.font = HistoryCache.LoadFont(state.dialogue.dialogueFont);
            log.dialogueText.color = state.dialogue.dialogueColor;
            log.dialogueFontSize = TEXT_DEFAULT_SCALE * state.dialogue.dialogueScale;
            log.dialogueText.fontSize = log.dialogueFontSize + textScaling;

            FitLogToText(log);

            logs.Add(log);
            
        }

        private void FitLogToText(HistoryLog log)
        {
            RectTransform rect = log.dialogueText.GetComponent<RectTransform>();
            ContentSizeFitter textCSF = log.dialogueText.GetComponent<ContentSizeFitter>();

            textCSF.SetLayoutVertical();

            LayoutElement logLayout = log.container.GetComponent<LayoutElement>();
            float height = rect.rect.height;

            float perc = height / LOG_DEFAULT_HEIGHT;
            float extraScale = (LOG_HEIGHT_PER_LINE * perc) - LOG_HEIGHT_PER_LINE;
            float scale = LOG_STARTING_HEIGHT + extraScale;

            logLayout.preferredHeight = scale + textScaling;

            logLayout.preferredHeight += 2f * logScaling;

        }

        public void SetLogScaling()
        {
            logScaling = logScaleSlider.value;

            foreach(HistoryLog log in logs)
            {
                log.nameText.fontSize = log.nameFontSize + textScaling;
                log.dialogueText.fontSize = log.dialogueFontSize + textScaling;

                FitLogToText(log);
            }
        }

        public void Clear()
        {
            for(int i = 0; i < logs.Count; i++)
            {
                DestroyImmediate(logs[i].container);
            }
            
            logs.Clear();
        }

        public void Rebuild()
        {
            foreach(var state in manager.history)
            {
                CreateLog(state);
            }
        }
    }
}

