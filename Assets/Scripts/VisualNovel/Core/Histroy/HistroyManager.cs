using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace History
{
    [RequireComponent(typeof(HistoryNavigation))]
    public class HistoryManager : MonoBehaviour
    {
        public static HistoryManager instance { get; private set; }
        public List<HistoryState> history = new List<HistoryState>();

        private HistoryNavigation navigation;

        void Awake()
        {
            instance = this;
            navigation = GetComponent<HistoryNavigation>();
        }

        void Start()
        {
            DialogueSystem.instance.onClear += LogCurrentState;
        }

        public void LogCurrentState()
        {
            HistoryState state = HistoryState.Capture();
            history.Add(state);
        }

        public void LoadState(HistoryState state)
        {
            state.Load();

        }

        public void GoForward() => navigation.GoForward();
        public void GoBack() => navigation.GoBack();

        
    }

}
