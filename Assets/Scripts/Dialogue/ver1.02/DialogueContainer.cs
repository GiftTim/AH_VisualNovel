using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueContainer 
{
    // 대화 영역
    public GameObject root;

    //대화 상대
    public GameObject dialogueNameBox;
    public TextMeshProUGUI nameText;

    //대화 창
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
}
