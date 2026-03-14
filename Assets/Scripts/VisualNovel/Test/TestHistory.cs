using UnityEngine;
using System.Collections.Generic;
using History;

namespace TESTING
{
    public class TestHistory : MonoBehaviour
    {
        public HistoryState state = new HistoryState();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                state = HistoryState.Capture();
            }

            if(Input.GetKeyDown(KeyCode.R))
            {
                state.Load();
            }
        }
    }
}