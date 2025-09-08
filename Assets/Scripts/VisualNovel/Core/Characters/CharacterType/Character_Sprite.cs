using UnityEngine;

namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        public Character_Sprite(string name, CharacterConfigData config) : base(name, config)
        {
            Debug.Log($"Crated Sprite Character : '{name}'");
        }
    }
}