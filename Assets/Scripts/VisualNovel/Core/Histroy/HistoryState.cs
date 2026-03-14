using System.Collections.Generic;
using System.Data.Common;
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

        public static HistoryState Capture()
        {
            HistoryState state = new HistoryState();

            state.dialogue = DialogueData.Capture();
            state.characters = CharacterData.Capture();
            state.audio = AudioData.Capture();
            state.graphics = GraphicData.Capture();
            
            return state;
        }

        public void Load()
        {
            DialogueData.Apply(dialogue);
            //characters.Apply();
            AudioData.Apply(audio);
            //graphics.Apply();
        }
    }
}