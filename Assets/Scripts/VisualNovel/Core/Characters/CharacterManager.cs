using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        private CharacterConfigurationSO config => DialogueSystem.instance.config.CharacterConfigurationAsset;

        private void Awake()
        {
            instance = this;
        }

        public Character CreateCharater(string characterName)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"A Character called '{characterName}' already exists. Did not create the character.");
                return null;
            }

            CHARACTER_INFO info = GetCharacterInfo(characterName);

            Character character = CreateCharacterFromInfo(info);

            characters.Add(characterName.ToLower(), character);

            return character;
        }

        private CHARACTER_INFO GetCharacterInfo(string CharacterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();

            result.name = CharacterName;

            result.config = config.GetConfig(CharacterName);

            return result;
        }

        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            switch (info.config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name);

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name);

                default:
                    return null;
            }

        }

        private class CHARACTER_INFO
        {
            public string name = "";

            public CharacterConfigData config = null;
        }
    }
}