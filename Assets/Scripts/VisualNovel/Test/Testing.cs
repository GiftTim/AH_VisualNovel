using CHARACTERS;
using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle;
using UnityEngine;

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
            Character Mina = CharacterManager.instance.CreateCharacter("Mina");
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
                "I am Mina",

                "한국어도 잘해요"
            };

            yield return Mina.Say(lines);

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
            //Character Mina = CharacterManager.instance.CreateCharacter("Mina");
            //Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character Mastermind1 = CreateCharacter("Mastermind1 as Generic");
            Character Raelin = CreateCharacter("Raelin");
            Character Mina = CreateCharacter("Mina");
            Character Mari = CreateCharacter("Mari");
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Mastermind1.SetPosition(Vector2.zero);
            Raelin.SetPosition(new Vector2(0.5f, 0.5f));
            Mina.SetPosition(Vector2.one);
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
            yield return Mina.Show();


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
    /* [Testing Character Moving2]
    public class CharacterMoving_Test : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            //Character Mari = CharacterManager.instance.CreateCharacter("Mari");
            //Character Mina = CharacterManager.instance.CreateCharacter("Mina");
            //Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Mastermind1 = CreateCharacter("Mastermind1 as Generic") as Character_Sprite;
            //Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            //Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;
            //Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Mastermind1.Show();

            Sprite s1 = Mastermind1.GetSprite("Characters-Girl");
            Mastermind1.SetSprite(s1);

            Debug.Log($"Visible = {Mastermind1.isVisible}");        

            yield return null;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
    */
    /* [Testing Character layer]
    public class CharacterLayer_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            //Character_Sprite Mastermind1 = CreateCharacter("Mastermind1 as Generic") as Character_Sprite;
            //Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Mina   = CreateCharacter("Mina") as Character_Sprite;
            Mina.isVisible = true;

            //Mina.Hide();

            //yield return new WaitForSeconds(1);

            //yield return Raelin.TransitionSprite(Raelin.GetSprite("B_Embarrassed"), 1);
            //yield return Raelin.TransitionSprite(Raelin.GetSprite("B2"));

            //Raelin.Hide();

            //yield return new WaitForSeconds(1);

            //Mina.Show();

            yield return new WaitForSeconds(1);

            yield return Mina.TransitionSprite(Mina.GetSprite("Mina-A_ShyFace"), 1);
            yield return Mina.TransitionSprite(Mina.GetSprite("Mina-A2"));

            yield return null;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
    */
    /* [Testing Character Color]
    public class CharacterLayer_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {

            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;

            yield return new WaitForSeconds(1);

            //Raelin.layers[1].SetColor(Color.red);
            yield return Raelin.TransitionToColor(Color.red, speed: 0.2f);
            yield return Raelin.TransitionToColor(Color.blue);
            yield return Raelin.TransitionToColor(Color.yellow);
            yield return Raelin.TransitionToColor(Color.black);
            yield return Raelin.TransitionToColor(Color.white);

            yield return null;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
    */
    /* [Testing Character Highlighting]
    public class CharacterLayer_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {

            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Raelin.SetPosition(Vector2.zero);
            Mina.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1);

            Mina.UnHighlight();
            yield return Raelin.Say("I want to say something");

            Raelin.UnHighlight();
            Mina.Highlight();
            yield return Mina.Say("But I want to say something too! {c} Can I go first?");

            Raelin.Highlight();
            Mina.UnHighlight();
            yield return Raelin.Say("Sure, {a} be my guest.");

            Mina.Highlight();
            Raelin.UnHighlight();
            Mina.TransitionSprite(Mina.GetSprite("Mina-A_ShyFace"), layer :1);
            yield return Mina.Say("Yay!");

            yield return null;
        }
    }
    */
    /* [Testing Charater Flipping]
    public class CharacterFlipping_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {

            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Raelin.SetPosition(Vector2.zero);
            Mina.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1);

            yield return Raelin.Flip(0.3f);

            yield return Mina.FaceRight(immediate: true);

            yield return Raelin.FaceLeft(immediate: true);

            Mina.UnHighlight();
            yield return Raelin.Say("I want to say something");

            Raelin.UnHighlight();
            Mina.Highlight();
            yield return Mina.Say("But I want to say something too! {c} Can I go first?");

            Raelin.Highlight();
            Mina.UnHighlight();
            yield return Raelin.Say("Sure, {a} be my guest.");

            Mina.Highlight();
            Raelin.UnHighlight();
            Mina.TransitionSprite(Mina.GetSprite("Mina-A_ShyFace"), layer: 1);
            yield return Mina.Say("Yay!");

            yield return null;
        }
    }
    */
    /* [Testing Character Sorting]
    public class CharacterSorting_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {

            Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            Character_Sprite GuardRed = CreateCharacter("Guard Red as Generic") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;
            //Mina.isVisible = false;

            GuardRed.SetColor(Color.red);

            Raelin.SetPosition(new Vector2(0.3f, 0));
            Mina.SetPosition(new Vector2(0.45f, 0));
            Guard.SetPosition(new Vector2(0.6f, 0));
            GuardRed.SetPosition(new Vector2(0.85f, 0));

            GuardRed.SetPriority(1000);
            Mina.SetPriority(15);
            Raelin.SetPriority(8);
            Guard.SetPriority(30);

            yield return new WaitForSeconds(1);

            CharacterManager.instance.SortCharacters(new string[] { "Mina", "Raelin" });

            yield return new WaitForSeconds(1);

            CharacterManager.instance.SortCharacters();

            yield return new WaitForSeconds(1);

            CharacterManager.instance.SortCharacters(new string[] { "Raelin", "Guard red", "Guard", "Mina" });

            yield return null;
        }
    }
    */
    /* [Testing_Character Animation]
    public class Animation_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;

            Raelin.SetPosition(new Vector2(0, 0));
            Mina.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1);

            Mina.TransitionSprite(Mina.GetSprite("Mina-A1"));
            Mina.TransitionSprite(Mina.GetSprite("Mina-A_SmileFace"), layer: 1);
            Mina.Animate("Hop");
            yield return Mina.Say("Where did this wind chill come from?");

            Raelin.FaceRight();
            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Shocked"), layer: 1);
            Raelin.MoveToPosition(new Vector2(0.1f, 0));
            Raelin.Animate("Shiver",true);
            yield return Raelin.Say("I don't know -- but I hate it!{a} It's freezing!");

            Mina.TransitionSprite(Mina.GetSprite("Mina-A_ShyFace"), layer: 1);
            yield return Mina.Say("Oh, it's over!");

            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Shocked"), layer: 1);
            Raelin.Animate("Shiver", false);
            yield return Raelin.Say("Thank the Lord...{a} I'm not wearing enough clothes for that crap.");

            yield return null;
        }
    }
    */
    /* [Testing Live2D Character]
    public class Live2D_Testing : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        private void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Live2D Mao = CreateCharacter("Mao") as Character_Live2D;

            Raelin.SetPosition(new Vector2(0, 0));
            Mao.SetPosition(new Vector2(1, 0));


            yield return null;
        }
    }
    */
    /* [Testing Graphic Layers]

    public class GraphicLayerTesting : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(RunningLayers());
        }
        IEnumerator Running()
        {
            //GraphicPanelManager.Instance.GetPanel("Background").GetLayer(0, true);

            //layer.currentGraphic.renderer.material.SetColor("_Color", Color.red);
            //layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", transitionSpeed: 0.07f, useAudio: true);

            GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
            GraphicLayer layer = panel.GetLayer(0, true);

            yield return new WaitForSeconds(1f);

            Texture blendTex = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
            layer.SetTexture("Graphics/BG Images/2", blendingTexture: blendTex);

            yield return new WaitForSeconds(3f);

            layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", blendingTexture: blendTex);

            yield return new WaitForSeconds(5f);

            layer.currentGraphic.FadeOut();

            yield return new WaitForSeconds(5f);

            Debug.Log(layer.currentGraphic);
        }

        IEnumerator RunningLayers()
        {
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
            GraphicLayer layer0 = panel.GetLayer(0, true);
            GraphicLayer layer1 = panel.GetLayer(1, true);

            GraphicPanel cinematic = GraphicPanelManager.instance.GetPanel("Cinematic");
            GraphicLayer cinLayer = cinematic.GetLayer(0, true);

            yield return new WaitForSeconds(1f);

            layer0.SetVideo("Graphics/BG Videos/Nebula");
            layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

            Character Mina = CharacterManager.instance.CreateCharacter("Mina", true);

            yield return Mina.Say("Let's take a look at a picture on the cinematic layer.");

            cinLayer.SetTexture("Graphics/Gallery/pup");

            yield return DialogueSystem.instance.Say("Narrator", "We truly don't deserve dogs");

            cinLayer.Clear();

            yield return new WaitForSeconds(1f);

            panel.Clear();
        }
    }
        */
    /* [Testing Graphic Panel Commands]
    public class GraphicPanelCommands : MonoBehaviour
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
    /*[Testing Audio Commands]
    public class AudioTesting : MonoBehaviour
    {
        [SerializeField] private TextAsset fileName = null;

        private void Start()
        {
            StartConversation();
            //StartCoroutine(Running());
        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileName);

            DialogueSystem.instance.Say(lines);
        }

        Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        IEnumerator Running()
        {
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;
            Character Me = CreateCharacter("Me");
            Mina.Show();

            GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/villagenight");

            AudioManager.instance.PlayTrack("Audio/Ambience/RainyMood", 0);
            AudioManager.instance.PlayTrack("Audio/Music/Calm", 1, pitch: 0.7f);

            yield return Mina.Say("제발 게임 성공해라");

            AudioManager.instance.StopTrack(1);       
        }

        IEnumerator Running1()
        {
            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;
            Character Me = CreateCharacter("Me");
            Mina.Show();

            yield return new WaitForSeconds(2f);

            AudioManager.instance.PlaySoundEffect("Audio/SFX/RadioStatic", loop: true);

            yield return Me.Say("I'm going to turn off the radio.");

            AudioManager.instance.StopSoundEffect("RadioStatic");
            AudioManager.instance.PlayVoice("Audio/Voices/exclamation");

            Mina.Say("Oh!");

            AudioManager.instance.PlaySoundEffect("Audio/SFX/thunder_strong_01");

            yield return new WaitForSeconds(1f);

            Mina.Animate("Hop");
            Mina.TransitionSprite(Mina.GetSprite("Mina-A2"));
            Mina.TransitionSprite(Mina.GetSprite("Mina-A_ShyFace"), 1);
            Mina.Say("Yikes!");
        }

        IEnumerator Running2()
        {
            AudioChannel channel = new AudioChannel(1);

            yield return null;
        }

        IEnumerator Running3()
        {
            yield return new WaitForSeconds(5);

            Character_Sprite Mina = CreateCharacter("Mina") as Character_Sprite;
            Mina.Show();

            yield return DialogueSystem.instance.Say("Narrator", "Can we see your ship?");

            GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/5");
            AudioManager.instance.PlayTrack("Audio/Music/Calm", volumeCap: 0.5f);
            AudioManager.instance.PlayVoice("Audio/Voices/wakeup");

            Mina.SetSprite(Mina.GetSprite("Mina-A2"), 0);
            Mina.SetSprite(Mina.GetSprite("Mina-A_ShyFace"), 1);
            Mina.MoveToPosition(new Vector2(0.7f, 0), speed: 0.5f);
            yield return Mina.Say("Yes, of course!");

            yield return Mina.Say("좋아, 엔진룸으로 가보자");

            GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/EngineRoom");
            AudioManager.instance.PlayTrack("Audio/Music/Happy2", volumeCap: 0.8f);

            yield return null;
        }
    }*/

    public class DialogueClosingTesting : MonoBehaviour
    {

        [SerializeField] private TextAsset fileToRoad = null;

        void Start()
        {
            StartConversation();
        }

        void StartConversation()
        {
            List<string> lines = FileManager.ReadTextAsset(fileToRoad);
            DialogueSystem.instance.Say(lines);
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                DialogueSystem.instance.dialogueContainer.Hide();
            }
            else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                DialogueSystem.instance.dialogueContainer.Show();
            }
        }
    }
}