#if UNITY_EDITOR

using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestRunFile : MonoBehaviour
    {
        [SerializeField] private TextAsset file;

        void Start()
        {
            LoadFile();
        }

        void LoadFile()
        {
            List<string> lines = FileManager.ReadTextAsset(file);
            Conversation conversation = new Conversation(lines);
            DialogueSystem.instance.Say(conversation);
        }
    }
}

#endif