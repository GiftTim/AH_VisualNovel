using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
using DIALOGUE;
using TMPro;
using UnityEditor.Rendering;

namespace TESTING
{
    public class TestCharacters : MonoBehaviour
    {
        public TMP_FontAsset tempFont;

        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        // Start is called before the first frame update
        void Start()
        {
            
            //Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");
            //Character Stella2 = CharacterManager.instance.CreateCharacter("Stella");
            //Character Adam = CharacterManager.instance.CreateCharacter("Adam");
            //StartCoroutine(Test7());
        }

        IEnumerator Test()
        {
            //Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            //Character_Sprite GuardRed = CreateCharacter("Guard Red as Generic") as Character_Sprite;
            //Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            //Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Character_Live2D Mao = CreateCharacter("Mao") as Character_Live2D;
            Character_Live2D Natori = CreateCharacter("Natori") as Character_Live2D;
            Character_Live2D Rice = CreateCharacter("Rice") as Character_Live2D;
            Character_Live2D Koharu = CreateCharacter("Koharu") as Character_Live2D;

            Natori.SetPosition(new Vector2(0, 0));
            Mao.SetPosition(new Vector2(0.5f, 0));
            Rice.SetPosition(new Vector2(0.25f, 0));
            Koharu.SetPosition(new Vector2(1, 0));
            //Stella.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1);

            yield return null;
        }

        IEnumerator Test2()
        {
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Character_Live2D Rice = CreateCharacter("Rice") as Character_Live2D;

            yield return new WaitForSeconds(1);

            Stella.Animate("Hop");
            Rice.Animate("Hop");

            Stella.MoveToPosition(Vector2.zero);
            Rice.MoveToPosition(new Vector2(1,0));

            Stella.SetColor(Color.red);

            Stella.SetPriority(1);

            Stella.Hide();

            Stella.Show();

            Stella.Highlight();

            Stella.UnHighlight();

            Stella.FaceLeft();

            Stella.FaceRight();

            Stella.isVisible = false;
            Rice.isVisible = false;

            Stella.Animate("Hop");
            Rice.isVisible = true;
        }

        IEnumerator Test3 () 
        {
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            Character_Sprite Girl = CreateCharacter("Girl as Generic") as Character_Sprite;
            Character_Sprite Man = CreateCharacter("Man as Generic") as Character_Sprite;

            Girl.SetSprite(Girl.GetSprite("Girl"));
            Man.SetSprite(Man.GetSprite("Man"));

            Stella.SetPosition(new Vector2(0.3f,0));
            Raelin.SetPosition(new Vector2(0.4f,0));
            Guard.SetPosition(new Vector2(0.5f,0));
            Girl.SetPosition(new Vector2(0.6f,0));
            Man.SetPosition(new Vector2(0.7f,0));

            yield return new WaitForSeconds(1);

            CharacterManager.instance.SortCharacters(new string[] { "Raelin","Girl","Man","Stella","Guard"});

            yield return new WaitForSeconds(1);

            Stella.SetPriority(6);

        }

        IEnumerator Test4()
        {
            Character_Live2D Rice = CreateCharacter("Rice") as Character_Live2D;
            Character_Live2D Mao = CreateCharacter("Mao") as Character_Live2D;
            Character_Live2D Natori = CreateCharacter("Natori") as Character_Live2D;
            Character_Live2D Koharu = CreateCharacter("Koharu") as Character_Live2D;

            Rice.SetPosition(new Vector2(0.3f, 0));
            Mao.SetPosition(new Vector2(0.4f, 0));
            Natori.SetPosition(new Vector2(0.5f, 0));
            Koharu.SetPosition(new Vector2(0.6f, 0));

            yield return new WaitForSeconds(1);

            CharacterManager.instance.SortCharacters(new string[] { "Koharu", "Natori", "Mao", "Rice" });

            yield return new WaitForSeconds(1);

            Rice.SetPriority(5);

        }

        IEnumerator Test5()
        {
            Character_Model3D Amarya = CreateCharacter("Amarya") as Character_Model3D;

            yield return new WaitForSeconds(1);

            yield return Amarya.MoveToPosition(new Vector2(0.5f, 0));



            yield return new WaitForSeconds(3);

        }

        IEnumerator Test6()
        {
            Character_Model3D Amarya = CreateCharacter("Amarya") as Character_Model3D;
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Character_Live2D Mao = CreateCharacter("Mao") as Character_Live2D;

            Stella.SetPosition(new Vector2(0, 0));
            Mao.SetPosition(new Vector2(0.5f, 0));
            Amarya.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(2f);

            Stella.FaceRight();
            Mao.FaceRight();
            Amarya.FaceRight();

            yield return new WaitForSeconds(2f);

            Stella.FaceLeft();
            Mao.FaceLeft();
            Amarya.FaceLeft();
        }

        IEnumerator Test7()
        {
            Character Monk = CreateCharacter("Monk as Generic");

            yield return Monk.Say("Normal dialogue configuration");

            Monk.SetDialogueColor(Color.red);
            Monk.SetNameColor(Color.blue);

            yield return Monk.Say("Customized dialogue here");

            Monk.ResetConfigurationData();

            yield return Monk.Say("I should be back to normal");
        }

        // Update is called once per frame
        void Update()
        {
            Character Stella = CharacterManager.instance.GetCharacter("Stella");
            if (Input.GetKeyDown(KeyCode.DownArrow))
                Stella.Hide();
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                Stella.Show();
        }
    }
}