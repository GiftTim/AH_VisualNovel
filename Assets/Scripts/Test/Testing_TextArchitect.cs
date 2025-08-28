using UnityEngine;

namespace TESTING
{
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
            "이것이 첫 번째 줄입니다.",
            "첫 번째 줄보다 조금 더 긴 두 번째 줄이 나옵니다.",
            "마지막으로, 이것은 텍스트 아키텍트의 능력을 테스트하기 위해 세 번째 줄 중에서 가장 긴 줄입니다."
        };

        string[] lines2 = new string[1]
        {
            "이건 졸라게 길게 적어야지 진행할 수 있는 글입니다. 이걸 진행해야지 어떻게든 허리 업을 진행할 수 있지요 그래서 이렇게 길게 적는 거랍니다. 현재는 어느정도의 길이까지 진행한지 잘 모르곘지만 이정도면 될 듯합니다."
        };
    }
}

