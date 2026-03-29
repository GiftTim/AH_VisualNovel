using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_General : CMD_DatabaseExtension
    {
        private static readonly string[] PARAM_SPEED = new string[] { "-s", "-spd" };
        private static readonly string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static readonly string[] PARAM_FILEPATH = new string[] { "-f", "-file", "-filepath" };
        private static readonly string[] PARAM_ENQUEUE = new string[] { "-e", "-enqueue" };
        private static readonly string[] PARAM_FOLDER = new string[] { "-folder", "-dir" };



        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));

            // Dialogue System Controls
            database.AddCommand("showui", new Func<string[], IEnumerator>(ShowDialogueSystem));
            database.AddCommand("hideui", new Func<string[], IEnumerator>(HideDialogueSystem));

            // Dialogue Box Controls
            database.AddCommand("Showdb", new Func<string[], IEnumerator>(ShowDialogueBox));
            database.AddCommand("Hidedb", new Func<string[], IEnumerator>(HideDialogueBox));

            database.AddCommand("load", new Action<string[]>(LoadNewDialogueFile));

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

        private static IEnumerator ShowDialogueBox(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: true);

            yield return DialogueSystem.instance.dialogueContainer.Show(speed, immediate);
        }

        private static IEnumerator HideDialogueBox(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 0f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            yield return DialogueSystem.instance.dialogueContainer.Hide(speed, immediate);
        }

        private static IEnumerator ShowDialogueSystem(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: true);

            yield return DialogueSystem.instance.Show(speed, immediate);
        }

        private static IEnumerator HideDialogueSystem(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 0f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            yield return DialogueSystem.instance.Hide(speed, immediate);
        }

        private static void LoadNewDialogueFile(string[] data)
        {
            string fileName = string.Empty;
            string folderName = string.Empty;
            bool enqueue = false;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_FILEPATH, out fileName);
            parameters.TryGetValue(PARAM_FOLDER, out folderName, defaultValue: string.Empty);
            parameters.TryGetValue(PARAM_ENQUEUE, out enqueue, defaultValue: false);

            string resourcePath = string.IsNullOrEmpty(folderName)
                ? FilePaths.GetPathToResource(FilePaths.resources_dialogueFiles, fileName)
                : FilePaths.GetPathToResource(FilePaths.resources_dialogueFiles, $"{folderName}/{fileName}");

            TextAsset file = Resources.Load<TextAsset>(resourcePath);

            if (file == null)
            {
                Debug.LogError($"대화 파일에서 '{resourcePath}' 파일을 불러올 수 없습니다. 해당 파일이 '{FilePaths.resources_dialogueFiles}' 리소스 폴더 안에 있는지 확인해 주세요.");
                return;
            }

            List<string> lines = FileManager.ReadTextAsset(file, includeBlackLines: true);
            Conversation newComversation = new Conversation(lines, file: resourcePath);

            if (enqueue)
            {
                DialogueSystem.instance.conversationManager.Enqueue(newComversation);
            }
            else
            {
                DialogueSystem.instance.conversationManager.StartConversation(newComversation);
            }
        }
    
    }
}
