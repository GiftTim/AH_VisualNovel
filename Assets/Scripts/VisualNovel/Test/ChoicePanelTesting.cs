using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class ChoicePanelTesting : MonoBehaviour
    {
        [SerializeField] private TextAsset fileToRoad = null;

        ChoicePanel panel;

        private void Start()
        {
            //StartCoroutine(Running2());
            StartConversation();

        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileToRoad);
            DialogueSystem.instance.conversationManager.architect.SetBuilderType(TABuilder.BuilderTypes.Typewriter);
            DialogueSystem.instance.Say(lines);
        }

        IEnumerator Running1()
        {
            string[] choices = new string[]
            {
                "Witness? Is that camera on?",
                "Oh, nah!",
                //"I didn't see nothin'!",
                //"Matta' Fact- I'm blind in my left eye and 43% blind in my right eye."
            };

            panel.Show("Did You Witness Anything Strange?", choices);
            
            while (panel.isWaitingOnUserChoice)
                yield return null;
        }

        IEnumerator Running2()
        {
            panel = ChoicePanel.instance;
            string[] choices = new string[]
            {
            //"Witness? Is that camera on?",
            //"I didn't see nothin'!",
            "Oh, nah!",
            "Matta' Fact- I'm blind in my left eye and 43% blind in my right eye."
            };

            panel.Show("왜 안되는 거지??", choices);

            while (panel.isWaitingOnUserChoice)
                yield return null;

            var decision = panel.lastDecision;

            Debug.Log($"Made choice {decision.answerIndex} '{decision.choices[decision.answerIndex]}'");
        }

    }
}