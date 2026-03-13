using UnityEngine;
using System.Collections.Generic;
using History;

namespace TESTING
{
    public class TestHistory : MonoBehaviour
    {
        public DialogueData data;
        public List<AudioData> audioData;
        public List<GraphicData> graphicData;

        private void Update()
        {
            data = DialogueData.Capture();
            audioData = AudioData.Capture();
            graphicData = GraphicData.Capture();
        }
    }
}