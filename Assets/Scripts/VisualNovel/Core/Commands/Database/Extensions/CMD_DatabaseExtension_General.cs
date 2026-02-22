using DIALOGUE;
using System;
using System.Collections;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_General : CMD_DatabaseExtension
    {
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));

            // Dialogue System Controls
            database.AddCommand("showui", new Func<IEnumerator>(ShowDialogueSystem));
            database.AddCommand("hideui", new Func<IEnumerator>(HideDialogueSystem));

            // Dialogue Box Controls
            database.AddCommand("Showdb", new Func<IEnumerator>(ShowDialogueBox));
            database.AddCommand("Hidedb", new Func<IEnumerator>(HideDialogueBox));
        }

        private static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float time))
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                Debug.LogError($"[CMD_DatabaseExtension_General] Invalid wait time: {data}");
            }
        }

        private static IEnumerator ShowDialogueBox()
        {
            yield return DialogueSystem.instance.dialogueContainer.Show();
        }

        private static IEnumerator HideDialogueBox()
        {
            yield return DialogueSystem.instance.dialogueContainer.Hide();
        }

        private static IEnumerator ShowDialogueSystem()
        {
            yield return DialogueSystem.instance.Show();
        }

        private static IEnumerator HideDialogueSystem()
        {
            yield return DialogueSystem.instance.Hide();
        }
    }
}
