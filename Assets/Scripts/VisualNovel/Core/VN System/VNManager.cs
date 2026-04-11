using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VNManager : MonoBehaviour
    {
        public static VNManager instance { get; private set; }
        [SerializeField] private VisualNovelSO config;

        public Camera mainCamera;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            VNDatabaseLinkSetup linkSetup = GetComponent<VNDatabaseLinkSetup>();
            linkSetup.SetupExtrnalLinks();

            if (VNGameSave.activeFile == null)
            {
                VNGameSave.activeFile = new VNGameSave();
            }

        }

        private void Start()
        {
            LoadGame();
        }

        private void LoadGame()
        {
            if(VNGameSave.activeFile.newGame)
            {
                List<string> lines = FileManager.ReadTextAsset(config.startingFile);
                Conversation start = new Conversation(lines);
                DialogueSystem.instance.Say(start);
            }
            else
            {
                VNGameSave.activeFile.Activate();
            }
        }

    }

}

