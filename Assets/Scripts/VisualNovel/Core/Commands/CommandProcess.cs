using System;
using UnityEngine;
using UnityEngine.Events;

namespace COMMANDS
{
    public class CommandProcess : MonoBehaviour
    {
        public Guid ID;
        public string processName;
        public Delegate command;
        public CoroutineWrapper runningProcess;
        public string[] args;

        public UnityEvent onTerminateAction;



        public CommandProcess(Guid id, string processName, Delegate command, CoroutineWrapper runningProcess, string[] args, UnityEvent onTerminateAction = null)
        {
            ID = id;
            this.processName = processName;
            this.command = command;
            this.runningProcess = runningProcess;
            this.args = args;
            this.onTerminateAction = onTerminateAction;
        }
    }
}