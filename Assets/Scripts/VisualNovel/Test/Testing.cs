using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

namespace TESTING
{
    /* [Testing_TextArchitect]
    public class Testing_TextArchitect : MonoBehaviour
    {
        DialogueSystem ds;
        TextArchitect architect;

        public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.instant;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.fade;
        }

        // Update is called once per frame
        void Update()
        {
            if(bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                architect.Stop();
            }

            if(Input.GetKeyDown(KeyCode.S))
            {
                architect.Stop();
            }
            

            if (Input.GetKeyDown(KeyCode.Space))
            {
                architect.Build(lines1[Random.Range(0, lines1.Length)]);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                architect.Append(lines1[Random.Range(0, lines1.Length)]);
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                if (architect.isBuilding)
                {
                    if(!architect.hurryUp)
                    {
                        architect.hurryUp = true;
                    }
                    else
                        architect.ForceComplete();

                }
                else
                {
                    architect.Build(lines2[0]);
                }

            }
        }


        string[] lines1 = new string[3]
        {
            "�̰��� ù ��° ���Դϴ�.",
            "ù ��° �ٺ��� ���� �� �� �� ��° ���� ���ɴϴ�.",
            "����������, �̰��� �ؽ�Ʈ ��Ű��Ʈ�� �ɷ��� �׽�Ʈ�ϱ� ���� �� ��° �� �߿��� ���� �� ���Դϴ�."
        };

        string[] lines2 = new string[1]
        {
            "�̰� ����� ��� ������� ������ �� �ִ� ���Դϴ�. �̰� �����ؾ��� ��Ե� �㸮 ���� ������ �� ������ �׷��� �̷��� ��� ���� �Ŷ��ϴ�. ����� ��������� ���̱��� �������� �� �𸣁����� �������� �� ���մϴ�."
        };
    }
    */
    /* [Testing_DialogueSystem]
    public class TestFiles: MonoBehaviour
    {
        [SerializeField] private TextAsset fileName;

        void Start()
        {
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            List<string> lines = FileManager.ReadTextAsset(fileName, false);

            foreach (string line in lines)
            {
                Debug.Log(line);
            }

            yield return null;
        }
    }
    */
    /* [Testing_Parsing]
    public class Testing_Parsing : MonoBehaviour
    {
        void Start() 
        { 
            
            //string line = "Speaker \"Danganronpa_Dialogue \\\"Goes in\\\" here!\" Command(arguments)";
            //DialogueParser.Parse(line);

            StartConversation();

        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset("testFile", false);

            foreach (string line in lines)
            {
                DIALOGUE_LINE dl = DialogueParser.Parse(line);
            }
        }

    }
    */
    /* [Testing_DialogueFile]
    public class Testing_DialogueFile : MonoBehaviour
    {
        [SerializeField] private TextAsset fileName;

        void Start()
        {

            StartConversation();

        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileName, false);

            DialogueSystem.instance.Say(lines);
        }
    }
    */
    /* [Testing_Conversation Segment Manager]
    public class DialogueSegments : MonoBehaviour
    {
        [SerializeField] private TextAsset fileName = null;

        void Start()
        {
            StartConversation();
        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileName);
            
            DialogueSystem.instance.Say(lines);
        }
    }
    */
    /* [Testing_SpeakerSegments]
    public class SpeakerSegments : MonoBehaviour
    {
        [SerializeField] private TextAsset fileToRoad = null;

        void Start()
        {
            StartConversation();
        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileToRoad);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if(string.IsNullOrWhiteSpace(line))
                    continue; 

                DIALOGUE_LINE dl = DialogueParser.Parse(line);

                Debug.Log($"{dl.speakerData.name} as [{(dl.speakerData.castName != string.Empty ? dl.speakerData.castName : dl.speakerData.name)}] at {dl.speakerData.castPosition}");

                List<(int l, string ex)> expr = dl.speakerData.CastExpressions;
                for(int c = 0; c < expr.Count; c++)
                {
                    Debug.Log($"[Layer[{expr[c].l}] = '{expr[c].ex}']");
                }
            }


            //DialogueSystem.instance.Say(lines);
        }
    }
    */

    public class CommandSegments : MonoBehaviour
    {
        [SerializeField] private TextAsset fileToRoad = null;
        void Start()
        {
            StartConversation();
        }
        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileToRoad);

            foreach (string line in lines)
            {

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                DIALOGUE_LINE dl = DialogueParser.Parse(line);

                for (int i = 0; i < dl.commandData.commands.Count; i++)
                {
                    DL_COMMAND_DATA.Command command = dl.commandData.commands[i];
                    Debug.Log($"Command [{i}] : {command.name} has arguments [{string.Join(", " , command.arguments)}]");
                }


                //DialogueSystem.instance.Say(lines);
            }
        }
    }
}

