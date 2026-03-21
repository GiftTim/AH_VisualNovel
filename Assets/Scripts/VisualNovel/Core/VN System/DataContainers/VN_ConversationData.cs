using UnityEngine;
using System.Collections.Generic;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class VN_ConversationData
    {

        public List<string> conversation = new List<string>(); 
        public int progress;
    }
}