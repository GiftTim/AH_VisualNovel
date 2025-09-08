using UnityEngine;
using TMPro;

namespace DIALOGUE
{
    [System.Serializable]
    public class DialogueContainer
    {
        // ��ȭ ����
        public GameObject root;

        //��ȭ ���
        public NameContainer nameContainer;

        //��ȭ â
        public GameObject dialogueBox;
        public TextMeshProUGUI dialogueText;

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
    }
}