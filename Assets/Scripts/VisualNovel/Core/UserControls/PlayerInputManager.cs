using UnityEngine;
using System;
using History;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput input;
        private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions 
            = new List<(InputAction, Action<InputAction.CallbackContext>)>();

        void Awake()
        {
            input = GetComponent<PlayerInput>();

            InitializeActions();
        }

        private void InitializeActions()
        {
            actions.Add((input.actions["Next"], OnNext));
            actions.Add((input.actions["HistoryBack"], OnHistoryBack));
            actions.Add((input.actions["HistoryForward"], OnHistoryForward));
            actions.Add((input.actions["HistoryLogs"], OnHistoryToggleLog));
        }

        private void OnEnable()
        {
            foreach (var inputAction in actions)
            {
                inputAction.action.performed += inputAction.command;
            }
        }

        private void OnDisable()
        {
            foreach (var inputAction in actions)
            {
                inputAction.action.performed -= inputAction.command;
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }
         public void OnHistoryBack(InputAction.CallbackContext context)
        {
            HistoryManager.instance.GoBack();
        }
        public void OnHistoryForward(InputAction.CallbackContext context)
        {
            HistoryManager.instance.GoForward();
        }

        public void OnHistoryToggleLog(InputAction.CallbackContext c)
        {
            var logs = HistoryManager.instance.logManager;

            if(!logs.isOpen)
            {
                logs.Open();
            }
            else
            {
                logs.Close();
            }
        }
    }
}