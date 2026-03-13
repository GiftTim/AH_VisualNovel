using UnityEngine;
using TMPro;
using System.Collections;

namespace DIALOGUE
{
    [System.Serializable]
    public class DialogueContainer
    {
        // 대화 영역
        public GameObject root;

        //대화 상대
        public NameContainer nameContainer;

        //대화 창
        public GameObject dialogueBox;
        public TextMeshProUGUI dialogueText;

        // 대화창 시각(페이드 인, 아웃) 컨트롤러
        private CanvasGroupController cgController;


        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;

        private bool initialized = false;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            cgController = new CanvasGroupController(DialogueSystem.instance, root.GetComponent<CanvasGroup>());

        }

        public bool isVisible => cgController.isVisible;

        public Coroutine Show(float speed = 1f, bool immediate = false) => cgController.Show(speed, immediate);
        public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);
    }
}