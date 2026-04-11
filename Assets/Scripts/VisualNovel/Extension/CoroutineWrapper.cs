using UnityEngine;

namespace COMMANDS
{
    public class CoroutineWrapper
    {
        private MonoBehaviour owner;
        private Coroutine coroutine;

        public bool IsDone = false;

        public CoroutineWrapper(MonoBehaviour owner, Coroutine coroutine)
        {
            this.owner = owner;
            this.coroutine = coroutine;
        }

        public void Stop()
        {
            owner.StopCoroutine(coroutine);
            IsDone = true;
        }
    }
}