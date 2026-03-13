using System.Collections.Generic;
using UnityEngine;

namespace History
{
    [System.Serializable]
    public class HistoryState
    {
        public DialogueData dialogue;
        public List<CharacterData> characters;
        public List<AudioData> audio;
        public List<GraphicData> graphics;

        public void Load()
        {

        }
    }
}