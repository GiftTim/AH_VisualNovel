using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using CHARACTERS;
using TMPro;

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
    /* [Testing_CommandSegments]
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
                    Debug.Log($"Command [{i}] : {command.name} has arguments [{string.Join(", ", command.arguments)}]");
                }


                DialogueSystem.instance.Say(lines);
            }
        }
    }
    */
    /* [Testing_Command_1]
    public class CommandTesting_1 : MonoBehaviour
    {

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                CommandManager.instance.Execute("moveCharDemo", "left");
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                CommandManager.instance.Execute("moveCharDemo", "right");

            //StartCoroutine(Running());
        }

        IEnumerator Running()
        {
            yield return CommandManager.instance.Execute("print");
            yield return CommandManager.instance.Execute("print_1p", "Hello World!");
            yield return CommandManager.instance.Execute("print_mp", "Line1", "Line2", "Line3");

            yield return CommandManager.instance.Execute("lambda");
            yield return CommandManager.instance.Execute("lambda_1p", "Hello Lambda!");
            yield return CommandManager.instance.Execute("lambda_mp", "Lambda1", "lambda2", "lambda3");

            yield return CommandManager.instance.Execute("process");
            yield return CommandManager.instance.Execute("process_1p", "3");
            yield return CommandManager.instance.Execute("process_mp", "process Line 1", "process Line 2", "process Line 3");
        }
    }
    */
    /* [Testing_Command 2]
    public class CommandTesting_2 : MonoBehaviour
    {
        [SerializeField] private TextAsset fileToRead = null;

        private void Start()
        {
            StartConversation();
        }

        private void Update()
        {
            
        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileToRead);

            DialogueSystem.instance.Say(lines);
        }
    }
    */
    /* [Testing_Character1]
    public class CharacterMoving_Test : MonoBehaviour
    {
        public TMP_FontAsset tempFont;

        private void Start()
        {


            //Character Mari2 = CharacterManager.instance.CreateCharacter("Mari");
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character Mari = CharacterManager.instance.CreateCharacter("Mari");
            Character Rachel = CharacterManager.instance.CreateCharacter("Rachel");
            Character Ben = CharacterManager.instance.CreateCharacter("Benjamin");

            List<string> lines = new List<string>()
            {
                "Hi, there!",
                "This is a line.",
                "And {wa 1} another.",
                "And a last one."
            };

            yield return Mari.Say(lines);

            Mari.SetNameColor(Color.yellow);
            Mari.SetDialogueColor(Color.cyan);
            Mari.SetNameFont(tempFont);
            Mari.SetDialogueFont(tempFont);

            yield return Mari.Say(lines);

            Mari.ResetConfigurationData();

            yield return Mari.Say(lines);


            lines = new List<string>()
            {
                "I am Rachel",
            
                "한국어도 잘해요"
            };

            yield return Rachel.Say(lines);

            yield return Ben.Say("이 라인은 일단 한국어로 쓰는것과 {a} 제대로 나오는지를 확인 하기 위해서 입니다.");

            Debug.Log("Finished");

                yield return new WaitForSeconds(2f);

            Character Mari = CharacterManager.instance.CreateCharacter("Mari");

            yield return new WaitForSeconds(1f);

            yield return Mari.Hide();

            yield return new WaitForSeconds(1f);

            yield return Mari.Show();

            yield return new WaitForSeconds(1f);

            yield return Mari.Say("Hello!");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
    */
    /* [Testing_Character Moving]
    public class CharacterMoving_Test : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            //Character Mari = CharacterManager.instance.CreateCharacter("Mari");
            //Character Rachel = CharacterManager.instance.CreateCharacter("Rachel");
            //Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character Mastermind1 = CreateCharacter("Mastermind1 as Generic");
            Character Raelin = CreateCharacter("Raelin");
            Character Rachel = CreateCharacter("Rachel");
            Character Mari = CreateCharacter("Mari");
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Mastermind1.SetPosition(Vector2.zero);
            Raelin.SetPosition(new Vector2(0.5f, 0.5f));
            Rachel.SetPosition(Vector2.one);
            Mari.SetPosition(new Vector2(2, 1));
            Mina.SetPosition(new Vector2(0.5f, 0.5f));

            yield return Raelin.Show();
            yield return new WaitForSeconds(2f);
            yield return Raelin.Hide();

            Sprite BodySprite = Mina.GetSprite("Mina-3");
            Sprite faceSprite = Mina.GetSprite("Mina-5");

            Mina.SetSprite(BodySprite, 0);
            Mina.SetSprite(faceSprite, 1);

            yield return Mina.Show();
            yield return Mastermind1.Show();
            yield return Raelin.Show();
            yield return Rachel.Show();


            yield return Mastermind1.MoveToPosition(Vector2.one, smooth: true);
            yield return Mastermind1.MoveToPosition(Vector2.zero, smooth: true);





            yield return null;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
    */

    public class CharacterMoving_Test : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            //Character Mari = CharacterManager.instance.CreateCharacter("Mari");
            //Character Rachel = CharacterManager.instance.CreateCharacter("Rachel");
            //Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Mastermind1 = CreateCharacter("Mastermind1 as Generic") as Character_Sprite;
            //Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            //Character_Sprite Rachel = CreateCharacter("Rachel") as Character_Sprite;
            //Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Mastermind1.Show();

            Sprite s1 = Mastermind1.GetSprite("Characters-Girl");
            Mastermind1.SetSprite(s1);

            yield return null;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}