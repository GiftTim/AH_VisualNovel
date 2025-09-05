using UnityEngine;

[System.Serializable]
public class Danganronpa_Dialogue
{
    public Transform characterTransform; // 대화하는 캐릭터의 Transform
    public string characterName; // 대화하는 캐릭터의 이름
    public string[] contexts; // 대화 내용들

}

[System.Serializable]
public class  DialogueEvent
{
    public string characterName; // 대화하는 캐릭터의 이름

    public Vector2 line;        // 대화의 길이
    public Danganronpa_Dialogue[] dialogues; // 대화 내용들

}
