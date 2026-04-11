#if UNITY_EDITOR

using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestCensor : MonoBehaviour
    {
        void Start()
        {
            Check("this line has a badword1 in it?");
            Check("This should be clear of any bad words!");
            Check("this $tinkiNG line should be bad as well.");
        }

        void Check(string line)
        {
            if(CensorManager.Censor(ref line))
            {
                Debug.Log($"<color=red>'{line}'");
            }
            else
            {
                Debug.Log($"<color=green>'{line}'");
            }
        }
    }
}

#endif