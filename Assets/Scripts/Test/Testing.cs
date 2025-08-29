using System.Collections;
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
    public class Testing_Parsing : MonoBehaviour
    {
        void Start() 
        { 
            string line = "Speaker \"Dialogue \\\"Goes in\\\" here!\" Command(arguments)";

            DialogueParser.Parse(line);
        }

    }
}

