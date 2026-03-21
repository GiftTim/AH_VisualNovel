using System.Collections.Generic;
using DIALOGUE;
using History;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class VNGameSave
    {
        public static VNGameSave activeFile = null;

        public const string FILE_TYPE = ".vns";
        public const string SCREENSHOT_FILE_TYPE = ".jpg";
        public const bool ENCRYPT_FILES = false;

        public string filePath => $"{FilePaths.gameSaves}{slotNumber}{FILE_TYPE}";
        public string screenshotPath => $"{FilePaths.gameSaves}{slotNumber}{SCREENSHOT_FILE_TYPE}";


        public string playerName;
        public int slotNumber = 1;

        public string[] activeConversations;
        public HistoryState activeState;
        public HistoryState[] historyLog;

        public void Save()
        {
            activeState = HistoryState.Capture();
            historyLog = HistoryManager.instance.history.ToArray();
            activeConversations = GetConversationData();

            string saveJSON = JsonUtility.ToJson(this);
            FileManager.Save(filePath, saveJSON);
        }

        public void Load()
        {

        }

        private string[] GetConversationData()
        {
            List<string> returnData = new List<string>();

            var conversations = DialogueSystem.instance.conversationManager.GetConversationQueue();

            for (int i = 0; i < conversations.Length; i++)
            {
                var conversation = conversations[i];
                string data = "";

                if (conversation.file != string.Empty)
                {
                    var compressedData = new VN_ConversationDataCompressed();
                    compressedData.fileName = conversation.file;
                    compressedData.progress = conversation.GetProgress();
                    compressedData.startIndex = conversation.fileStartIndex;
                    compressedData.endIndex = conversation.fileEndIndex;
                    data = JsonUtility.ToJson(compressedData);
                }
                else
                {
                    var fullData = new VN_ConversationData();
                    fullData.conversation = conversation.GetLines();
                    fullData.progress = conversation.GetProgress();
                    data = JsonUtility.ToJson(fullData);
                }

                returnData.Add(data);
            }
            return returnData.ToArray();
        }
    }
}

