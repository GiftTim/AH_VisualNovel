using UnityEngine;

namespace CHARACTERS
{
    public class Character_Live2D : Character
    {
        public Character_Live2D(string name, CharacterConfigData config) : base(name, config)
        {
            Debug.Log($"Crated Text Character : '{name}'");
        }
    }
}