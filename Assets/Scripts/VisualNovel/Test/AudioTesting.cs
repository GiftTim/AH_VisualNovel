using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
using DIALOGUE;

namespace TESTING
{
    public class AudioTesting : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(Running());
        }

        Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        IEnumerator Running2()
        {
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Character Me = CreateCharacter("Me");
            Stella.Show();

            AudioManager.instance.PlaySoundEffect("Audio/SFX/RadioStatic", loop: true);

            yield return Me.Say("Please turn off the radio.");

            AudioManager.instance.StopSoundEffect("RadioStatic");
            AudioManager.instance.PlayVoice("Audio/Voices/Stella/OhOk");

            Stella.Say("Okay!");
        }

        IEnumerator Running3()
        {
            yield return new WaitForSeconds(1);

            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Stella.Show();

            yield return DialogueSystem.instance.Say("Narrator", "Can we see your ship?");

            GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/5");
            AudioManager.instance.PlayTrack("Audio/Music/Electric Drift", volumeCap: 0.5f);
            AudioManager.instance.PlayVoice("Audio/Voices/Stella/Yeah_Laugh");

            Stella.SetSprite(Stella.GetSprite("4"), 0);
            Stella.SetSprite(Stella.GetSprite("laugh 4"), 1);
            Stella.MoveToPosition(new Vector2(0.7f, 0), speed: 0.5f);
            yield return Stella.Say("Yes, of course!");

            yield return Stella.Say("Let me show you the engine room.");

            GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/EngineRoom");
            AudioManager.instance.PlayTrack("Audio/Music/Fractured Soul", volumeCap: 0.5f);

            yield return null;
        }


        IEnumerator Running()
        {
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Character Me = CreateCharacter("Me");
            Stella.Show();

            GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/villagenight");

            AudioManager.instance.PlayTrack("Audio/Ambience/RainyMood", 0);
            AudioManager.instance.PlayTrack("Audio/Music/Calm", 1, pitch: 0.7f);

            yield return Stella.Say("We can have multiple channels for playing ambience as well as music!");

            AudioManager.instance.StopTrack(1);
        }
    }
}