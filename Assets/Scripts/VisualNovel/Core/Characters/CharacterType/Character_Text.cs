using UnityEngine;

namespace CHARACTERS
{
    public class Character_Text : Character
    {
        public Character_Text(string name, CharacterConfigData config) : base(name, config) 
        {
            Debug.Log($"Crated Text Character : '{name}'");
        }
    }
}