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
        public GameObject dialogueNameBox;
        public TextMeshProUGUI nameText;

        //��ȭ â
        public GameObject dialogueBox;
        public TextMeshProUGUI dialogueText;
    }
}