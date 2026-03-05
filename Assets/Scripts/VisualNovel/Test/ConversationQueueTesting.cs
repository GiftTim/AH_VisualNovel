using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class ConversationQueueTesting : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(Running());
        }

        IEnumerator Running()
        {
            List<string> lines = new List<string>()
            {
                "This is Line 1 from the {n}original conversation.",
                "This is Line 2 from the original conversation.",
                "This is Line 3 from the original conversation."
            };

            yield return DialogueSystem.instance.Say(lines);

            DialogueSystem.instance.Hide();
        }

        void Update()
        {
            List<string> lines = new List<string>();
            Conversation conversation = null;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                lines = new List<string>()
                {
                "This is the start of an enqueued conversation.",
                "We can keep it going!"
                };
                conversation = new Conversation(lines);
                DialogueSystem.instance.conversationManager.Enqueue(conversation);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                lines = new List<string>()
                {
                "This is an important conversation!",
                "August 26, 2023 is international dog day!"
                };
                conversation = new Conversation(lines);
                DialogueSystem.instance.conversationManager.EnqueuePriority(conversation);
            }
        }
    }
}