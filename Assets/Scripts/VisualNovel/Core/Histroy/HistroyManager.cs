using System;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace History
{
    [RequireComponent(typeof(HistoryLogManager))]
    [RequireComponent(typeof(HistoryNavigation))]
    public class HistoryManager : MonoBehaviour
    {
        public const int HISTORY_CACHE_LIMIT = 15;

        public static HistoryManager instance { get; private set; }
        public List<HistoryState> history = new List<HistoryState>();

        private HistoryNavigation navigation;
        public HistoryLogManager logManager{get; private set;}

        void Awake()
        {
            instance = this;
            navigation = GetComponent<HistoryNavigation>();
            logManager = GetComponentInChildren<HistoryLogManager>();
        }

        void Start()
        {
            DialogueSystem.instance.onClear += LogCurrentState;
        }

        public void LogCurrentState()
        {
            HistoryState state = HistoryState.Capture();
            history.Add(state);
            logManager.AddLog(state);

            if(history.Count > HISTORY_CACHE_LIMIT)
            {
                history.RemoveAt(0);
            }
        }

        public void LoadState(HistoryState state)
        {
            state.Load();

        }

        public void GoForward() => navigation.GoForward();
        public void GoBack() => navigation.GoBack();

    }

}
